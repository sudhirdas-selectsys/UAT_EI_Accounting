using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.QBO;
using Accounting.Utilities.External.Sage;
using Accounting.Utilities.Internal;
using Accounting.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace Accounting.Web.Controllers
{
    public class SystemConfigurationController : BaseController
    {
        public static IMemoryCache _memoryCache = WebAppReferenceUtility._memoryCache;
        public IActionResult SelectCompany()
        {
            List<Clients> clients = _accountRepository.GetAllClientsAssociatedToUser_Id(userSession.User_Id);
            Users user = _usersRepository.GetUserByUserId(userSession.User_Id);
            ViewBag.User = user;
            return PartialView(clients);
        }
        public IActionResult SelectClient()
        {
            List<Clients> clients = _accountRepository.GetAllClientsAssociatedToUser_Id(userSession.User_Id);
            return PartialView(clients);
        }
        [HttpPost]
        public ServiceResponse SaveDefaultClientSettings(long Client_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (Client_Id > 0)
                {
                    _usersRepository.UpdateDefaultClient_Id(userSession.User_Id, Client_Id);
                    UserSession userSessionUpdated = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
                    if (userSessionUpdated != null)
                    {
                        userSessionUpdated.Client_Id = Client_Id;
                        userSessionUpdated.ConfigurationClient_Id = Client_Id;
                        var client = _accountRepository.GetClients(Client_Id);
                        userSessionUpdated.CompanyName = client?.CompanyName ?? "Unknown";
                        userSessionUpdated.ConfigurationCompanyName = client?.CompanyName ?? "Unknown";
                        HttpContext.Session.SetObjectAsJson("UserSession", userSessionUpdated);
                        serviceResponse.Status = true;
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "User session expired. Please log in again.";
                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Default client updated successfully.";
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Please select a default client to proceed.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse SelectCompany(long Client_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (Client_Id > 0)
                {
                    UserSession userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
                    if (userSession != null)
                    {
                        userSession.Client_Id = Client_Id;
                        var client = _accountRepository.GetClients(Client_Id);
                        userSession.ConfigurationClient_Id = Client_Id;
                        userSession.CompanyName = client?.CompanyName ?? "Unknown";
                        userSession.ConfigurationCompanyName = client?.CompanyName ?? "Unknown";
                        HttpContext.Session.SetObjectAsJson("UserSession", userSession);
                        serviceResponse.Status = true;
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "User session expired. Please log in again.";
                    }
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Please select a client to proceed.";
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse SelectClient(long Client_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (Client_Id > 0)
                {
                    UserSession userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
                    if (userSession != null)
                    {
                        userSession.ConfigurationClient_Id = Client_Id;
                        var client = _accountRepository.GetClients(Client_Id);
                        userSession.ConfigurationCompanyName = client?.CompanyName ?? "Unknown";
                        HttpContext.Session.SetObjectAsJson("UserSession", userSession);
                        serviceResponse.Status = true;
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "User session expired. Please log in again.";
                    }
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Please select a client to proceed.";
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public IActionResult Index()
        {
            if (clients != null)
            {
                ViewBag.ServiceId = clients.AccountingService_Id;
                ViewBag.ClientId = userSession.ConfigurationClient_Id;
            }
            return View();
        }
        public IActionResult GetChartofAccounts()
        {
            List<ChartofAccounts> Accountlist = new List<ChartofAccounts>();
            Accountlist = _chartofAccountsRepository.ChartofAccountsList(clients.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.ExpertInsured);
            return PartialView(Accountlist);
        }
        public async Task<ServiceResponse> SyncChartofAccounts()
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            List<Account> result = null;
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            List<ChartofAccounts> Accountlist = null;
            HashSet<string> incomingIds = new HashSet<string>();
            try
            {
                if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                {
                    QBOUtility qboUtility = new QBOUtility(userSession.ConfigurationClient_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                    AccountsResponse accountsRes = qboUtility.GetAllAccounts();
                    if (accountsRes != null && accountsRes.QueryResponse != null)
                    {
                        result = accountsRes.QueryResponse.Account;
                        if (result?.Count() > 0)
                        {
                            foreach (var account in result)
                            {
                                ChartofAccounts chartofAccounts = new ChartofAccounts();
                                chartofAccounts.Name = account.Name;
                                chartofAccounts.ReferenceID = account.Id;
                                chartofAccounts.AccountType = account.AccountType;
                                chartofAccounts.AccountSubType = account.AccountSubType;
                                chartofAccounts.Classicfication = account.Classification;
                                chartofAccounts.Client_Id = userSession.ConfigurationClient_Id;
                                chartofAccounts.Service_Id = clients.AccountingService_Id;
                                _chartofAccountsRepository.InsertChartofAccounts(chartofAccounts);
                            }
                            _memoryCache.Remove("ChartofAccounts");
                            List<ChartofAccounts> ExistingChartofAccounts = _chartofAccountsRepository.GetChartOfAccountsByService_Id(clients.AccountingService_Id, clients.Client_Id);
                            if (ExistingChartofAccounts?.Count() > 0)
                            {
                                var QBOList = result.Select(x => x.Id).ToHashSet();
                                var NotInQBOList = ExistingChartofAccounts.Where(x => !QBOList.Contains(x.ReferenceID)).ToList();
                                if (NotInQBOList?.Count() > 0)
                                {
                                    foreach (var coa in NotInQBOList)
                                    {
                                        int Result = _chartofAccountsRepository.InactiveChartOfAccounts(coa.ChartOfAccounts_Id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Failed to retrieve chart of accounts";
                            return serviceResponse;
                        }
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"Failed to retrieve chart of accounts";
                        return serviceResponse;
                    }
                }
                else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                {
                    SageUtility sageUtility = new SageUtility(userSession.ConfigurationClient_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                    var sageAccounts = await sageUtility.GetAllAccounts_Intacct();

                    if (sageAccounts == null || !sageAccounts.Any())
                    {
                        return new ServiceResponse { Status = false, Message = "Failed to retrieve Sage accounts" };
                    }

                    foreach (var account in sageAccounts)
                    {
                        incomingIds.Add(account.AccountNo);

                        ChartofAccounts chart = new ChartofAccounts
                        {
                            Name = account.Title,
                            ReferenceID = account.AccountNo,
                            AccountType = null,
                            AccountSubType = null,
                            Classicfication = null,
                            Client_Id = userSession.ConfigurationClient_Id,
                            Service_Id = clients.AccountingService_Id,
                            Status = account.Status == "active"
                        };

                        _chartofAccountsRepository.InsertChartofAccounts(chart);
                    }
                    List<ChartofAccounts> ExistingChartofAccounts = _chartofAccountsRepository.GetChartOfAccountsByService_Id(clients.AccountingService_Id, clients.Client_Id);
                    if (ExistingChartofAccounts?.Any() == true)
                    {
                        var toDeactivate = ExistingChartofAccounts
                            .Where(x => !incomingIds.Contains(x.ReferenceID))
                            .ToList();

                        foreach (var coa in toDeactivate)
                        {
                            _chartofAccountsRepository.InactiveChartOfAccounts(coa.ChartOfAccounts_Id);
                        }
                    }

                    _memoryCache.Remove("ChartofAccounts");

                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Failed to retrieve chart of accounts";
                    return serviceResponse;
                }
                serviceResponse.Status = true;
                serviceResponse.Message = "Chart of accounts synced successfully.";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public IActionResult LineItemsChartofAccounts()
        {
            List<LU_LineItems_ChartOfAccounts> LineItemsChartOfAccountsList = new List<LU_LineItems_ChartOfAccounts>();
            LineItemsChartOfAccountsList = _lookupRepository.GetAllLineItems_ChartOfAccounts(clients.AccountingService_Id, userSession.ConfigurationClient_Id);
            foreach (var item in LineItemsChartOfAccountsList)
            {
                if (item.Client_Id == 0)
                {
                    item.Client_Id = userSession.ConfigurationClient_Id;
                }
                if (item.Service_Id == 0)
                {
                    Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
                    item.Service_Id = clients.AccountingService_Id;
                }
            }
            return PartialView(LineItemsChartOfAccountsList);
        }
        [HttpPost]
        public ServiceResponse BillingLineItemsChartofAccounts(LU_BillingType_LineItems_Directions BillingLineItems)
        {
            SqlTransaction sqlTransaction = null;
            try
            {
                LU_LineItems_ChartOfAccounts IncomeExpenceCOA = new LU_LineItems_ChartOfAccounts();
                LU_LineItems_ChartOfAccounts ARAPCOA = new LU_LineItems_ChartOfAccounts();
                if (BillingLineItems != null)
                {
                    if (BillingLineItems.IncomeExpenseLiabilityCOA_Id > 0 || (BillingLineItems.IncomeExpenseLiabilityCOA_Id > 0 && BillingLineItems.ARAPCOA_Id > 0))
                    {
                        sqlTransaction = _baseRepository.GetNewTransaction();
                        if (BillingLineItems.Is_Passthrough)
                        {
                            if (!string.IsNullOrEmpty(BillingLineItems.PassthroughBillingLineItemDirections))
                            {
                                List<int> BillingTypeLineItemsDirections_Ids = new List<int>();
                                BillingTypeLineItemsDirections_Ids = BillingLineItems.PassthroughBillingLineItemDirections.Split(',').Select(d => int.Parse(d.Trim())).ToList();
                                foreach (var Id in BillingTypeLineItemsDirections_Ids)
                                {
                                    if (BillingLineItems.IncomeExpenseLiabilityCOA_Id > 0)
                                    {
                                        LU_LineItems_ChartOfAccounts IncomeExpenceLiabilityCOA = new LU_LineItems_ChartOfAccounts();
                                        IncomeExpenceLiabilityCOA.Client_Id = BillingLineItems.Client_Id;
                                        IncomeExpenceLiabilityCOA.Service_Id = BillingLineItems.Service_Id;
                                        IncomeExpenceLiabilityCOA.BillingType_LineItems_Directions_Id = Id;
                                        IncomeExpenceLiabilityCOA.ChartOfAccount_Id = BillingLineItems.IncomeExpenseLiabilityCOA_Id ?? 0;
                                        IncomeExpenceLiabilityCOA.LOB_Carrier_Grouping_Id = BillingLineItems.LOB_Carrier_Grouping_Id;
                                        IncomeExpenceLiabilityCOA.ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                                        _lookupRepository.Insert_LU_LineItems_ChartOfAccountsWithTransaction(sqlTransaction, IncomeExpenceLiabilityCOA);
                                    }
                                    if (BillingLineItems.ARAPCOA_Id > 0)
                                    {
                                        LU_LineItems_ChartOfAccounts ARAPpassthroughCOA = new LU_LineItems_ChartOfAccounts();
                                        ARAPpassthroughCOA.Client_Id = BillingLineItems.Client_Id;
                                        ARAPpassthroughCOA.Service_Id = BillingLineItems.Service_Id;
                                        ARAPpassthroughCOA.BillingType_LineItems_Directions_Id = Id;
                                        ARAPpassthroughCOA.ChartOfAccount_Id = BillingLineItems.ARAPCOA_Id ?? 0;
                                        ARAPpassthroughCOA.LOB_Carrier_Grouping_Id = BillingLineItems.LOB_Carrier_Grouping_Id;
                                        ARAPpassthroughCOA.ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                                        _lookupRepository.Insert_LU_LineItems_ChartOfAccountsWithTransaction(sqlTransaction, ARAPpassthroughCOA);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (BillingLineItems.IncomeExpenseLiabilityCOA_Id > 0)
                            {
                                IncomeExpenceCOA.Client_Id = BillingLineItems.Client_Id;
                                IncomeExpenceCOA.Service_Id = BillingLineItems.Service_Id;
                                IncomeExpenceCOA.BillingType_LineItems_Directions_Id = BillingLineItems.BillingType_LineItems_Directions_Id;
                                IncomeExpenceCOA.ChartOfAccount_Id = BillingLineItems.IncomeExpenseLiabilityCOA_Id ?? 0;
                                IncomeExpenceCOA.LOB_Carrier_Grouping_Id = BillingLineItems.LOB_Carrier_Grouping_Id;
                                IncomeExpenceCOA.ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                                _lookupRepository.Insert_LU_LineItems_ChartOfAccountsWithTransaction(sqlTransaction, IncomeExpenceCOA);
                            }
                            if (BillingLineItems.ARAPCOA_Id > 0)
                            {
                                ARAPCOA.Client_Id = BillingLineItems.Client_Id;
                                ARAPCOA.Service_Id = BillingLineItems.Service_Id;
                                ARAPCOA.BillingType_LineItems_Directions_Id = BillingLineItems.BillingType_LineItems_Directions_Id;
                                ARAPCOA.ChartOfAccount_Id = BillingLineItems.ARAPCOA_Id ?? 0;
                                ARAPCOA.LOB_Carrier_Grouping_Id = BillingLineItems.LOB_Carrier_Grouping_Id;
                                ARAPCOA.ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                                _lookupRepository.Insert_LU_LineItems_ChartOfAccountsWithTransaction(sqlTransaction, ARAPCOA);
                            }
                        }
                        sqlTransaction.Commit();
                        serviceResponse.Status = true;
                        serviceResponse.Message = "Chart of Account record for Line Item has been updated successfully.";
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "Please choose Chart of Accounts to proceed.";
                    }

                }


            }
            catch (Exception ex)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = "Error while inserting Chart of Account record for the Line Item.";
            }
            return serviceResponse;
        }
        public IActionResult OperatingBankChartofAccounts()
        {
            List<LU_OperatingBanks_ChartOfAccounts> OperatingBanksChartOfAccountsList = new List<LU_OperatingBanks_ChartOfAccounts>();
            OperatingBanksChartOfAccountsList = _lookupRepository.GetAllOperatingBanksChartOfAccounts(clients.AccountingService_Id, userSession.ConfigurationClient_Id);
            foreach (var item in OperatingBanksChartOfAccountsList)
            {
                if (item.Client_Id == 0)
                {
                    item.Client_Id = userSession.ConfigurationClient_Id;
                }
                if (item.Service_Id == 0)
                {
                    Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
                    item.Service_Id = clients.AccountingService_Id;
                }
            }
            return PartialView(OperatingBanksChartOfAccountsList);
        }
        [HttpPost]
        public ServiceResponse OperatingBankChartofAccounts(LU_OperatingBanks_ChartOfAccounts OperatingBanks_ChartOfAccounts)
        {
            try
            {
                if (OperatingBanks_ChartOfAccounts.ChartOfAccounts_Id > 0)
                {
                    _lookupRepository.Insert_Update_OperatingBanksChartOfAccounts(OperatingBanks_ChartOfAccounts);
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Chart of Account record for Operating Bank has been updated successfully.";
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Please select a valid Chart of Account for the Operating Bank.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Error while inserting Chart of Account record for the Operating Bank.";
            }
            return serviceResponse;

        }
        public IActionResult PaymentDirectionChartofAccounts()
        {
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            ViewBag.ChartOfAccountList = LookupUtility.GetAllChartofAccounts().Where(x => x.Client_Id == userSession.ConfigurationClient_Id && x.Service_Id == clients.AccountingService_Id).ToList();
            List<LU_PaymentDirections_ChartOfAccounts> PaymentDirectionsChartOfAccountsList = new List<LU_PaymentDirections_ChartOfAccounts>();
            PaymentDirectionsChartOfAccountsList = _lookupRepository.GetAllPaymentDirectionsChartOfAccounts(clients.AccountingService_Id, userSession.ConfigurationClient_Id);
            if (PaymentDirectionsChartOfAccountsList?.Count() > 0)
            {
                foreach (var item in PaymentDirectionsChartOfAccountsList)
                {
                    if (item.Client_Id == 0)
                    {
                        item.Client_Id = clients.Client_Id;
                    }
                    if (item.Service_Id == 0)
                    {
                        item.Service_Id = clients.AccountingService_Id;
                    }
                }
            }
            return PartialView(PaymentDirectionsChartOfAccountsList);
        }

        [HttpPost]
        public ServiceResponse PaymentDirectionChartofAccounts(LU_PaymentDirections_ChartOfAccounts PaymentDirections_ChartOfAccounts)
        {
            try
            {
                LU_PaymentDirections_ChartOfAccounts StandardCOA = new LU_PaymentDirections_ChartOfAccounts();
                LU_PaymentDirections_ChartOfAccounts Writeoff = new LU_PaymentDirections_ChartOfAccounts();
                if (PaymentDirections_ChartOfAccounts != null)
                {
                    if (PaymentDirections_ChartOfAccounts.StandardCOA_Id > 0 || PaymentDirections_ChartOfAccounts.WriteoffCOA_Id > 0)
                    {
                        if (PaymentDirections_ChartOfAccounts.StandardCOA_Id > 0)
                        {
                            StandardCOA.Client_Id = PaymentDirections_ChartOfAccounts.Client_Id;
                            StandardCOA.Service_Id = PaymentDirections_ChartOfAccounts.Service_Id;
                            StandardCOA.ChartOfAccounts_Id = PaymentDirections_ChartOfAccounts.StandardCOA_Id ?? 0;
                            StandardCOA.PaymentDirection_Id = PaymentDirections_ChartOfAccounts.PaymentDirection_Id;
                            StandardCOA.PaymentDirectionsSubType_Id = MasterLookup._PaymentDirectionsSubTypes.Standard;
                            _lookupRepository.Insert_Update_PaymentDirectionsChartOfAccounts(StandardCOA);
                        }
                        if (PaymentDirections_ChartOfAccounts.WriteoffCOA_Id > 0)
                        {
                            Writeoff.Client_Id = PaymentDirections_ChartOfAccounts.Client_Id;
                            Writeoff.Service_Id = PaymentDirections_ChartOfAccounts.Service_Id;
                            Writeoff.ChartOfAccounts_Id = PaymentDirections_ChartOfAccounts.WriteoffCOA_Id ?? 0;
                            Writeoff.PaymentDirection_Id = PaymentDirections_ChartOfAccounts.PaymentDirection_Id;
                            Writeoff.PaymentDirectionsSubType_Id = MasterLookup._PaymentDirectionsSubTypes.Writeoff;
                            _lookupRepository.Insert_Update_PaymentDirectionsChartOfAccounts(Writeoff);
                        }
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "Please choose Chart of Accounts to proceed.";
                    }

                }

                serviceResponse.Status = true;
                serviceResponse.Message = "Chart of Account record for Payment Direction has been updated successfully.";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Error while inserting Chart of Account record for the Payment Direction.";
            }
            return serviceResponse;
        }
        public IActionResult LobCarrierGrouings()
        {
            List<LOBCarrierGrouping> lobcarriergroungs = _lobcarriergroupingRepository.GetAllLOBCarrierGrouping(userSession.ConfigurationClient_Id, clients.AccountingService_Id);
            ViewBag.ClientLobsList = _lobMappingRepository.GetAllLobsByClient_Id(userSession.ConfigurationClient_Id);
            ViewBag.ClientCarrierList = _installmentPlansRepository.GetAllCarriersByClient_Id(userSession.ConfigurationClient_Id);
            ViewBag.Client = userSession.ConfigurationClient_Id;
            return PartialView(lobcarriergroungs);
        }
        public IActionResult AddCarrier()
        {
            return PartialView();
        }
        [HttpPost]
        public ServiceResponse AddCarrier(CarrierDetails carrierDetails)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            if (carrierDetails != null)
            {
                carrierDetails.Client_Id = userSession.ConfigurationClient_Id;
                var carrierdetails = _policyDetailsRepository.InsertCarrierDetails(carrierDetails);
                if (carrierdetails != null)
                {
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Carrier Inserted Successfully";
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Unable to insert Carrier Details";
                }
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse LobCarrierGrouings(LOBCarrierGrouping lobcarriergrouping)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction LOBCarrierTransaction = null;
            LOBCarrierTransaction = _baseRepository.GetNewTransaction();
            if (LOBCarrierTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
                if (lobcarriergrouping.LOB_Ids.Count > 0 && lobcarriergrouping.Carrier_Ids.Count > 0)
                {
                    lobcarriergrouping.Client_Id = clients.Client_Id;
                    lobcarriergrouping.Service_Id = clients.AccountingService_Id;
                    if (lobcarriergrouping.LOB_Carrier_Grouping_Id > 0)
                    {
                        _lobcarriergroupingRepository.InactiveAllLOBCarrierDetails(lobcarriergrouping.LOB_Carrier_Grouping_Id);
                        lobcarriergrouping = _lobcarriergroupingRepository.InsertUpdateLOBCarrierGrouping(LOBCarrierTransaction, lobcarriergrouping, userSession.User_Id);
                        //var AllLobs = LookupUtility.GetAllLOBs().Select(l => l.LOB_Id).ToList();
                        //bool AllSelectedLobs = AllLobs.All(ids => lobcarriergrouping.LOB_Ids != null && lobcarriergrouping.LOB_Ids.Contains(ids));
                        bool AllSelectedLobs = lobcarriergrouping.LOB_Ids != null && lobcarriergrouping.LOB_Ids.Contains(0);
                        bool AllSelectedCarriers = lobcarriergrouping.Carrier_Ids != null && lobcarriergrouping.Carrier_Ids.Contains(0);
                        LOBGroupingDetails lobgroupingdetails = new LOBGroupingDetails();
                        if (AllSelectedLobs)
                        {
                            lobgroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                            lobgroupingdetails.LOB_Id = null;
                            lobgroupingdetails = _lobgroupingDetailsRepository.InsertLOBGrouping(LOBCarrierTransaction, lobgroupingdetails);
                        }
                        else
                        {
                            foreach (var lobitem in lobcarriergrouping.LOB_Ids)
                            {
                                lobgroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                                lobgroupingdetails.LOB_Id = LookupUtility.GetAllLOBs().FirstOrDefault(l => l.LOB_Id == lobitem)?.LOB_Id;
                                lobgroupingdetails = _lobgroupingDetailsRepository.InsertLOBGrouping(LOBCarrierTransaction, lobgroupingdetails);
                            }
                        }
                        CarrierGroupingDetails carriergroupingdetails = new CarrierGroupingDetails();
                        if (AllSelectedCarriers)
                        {
                            carriergroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                            carriergroupingdetails.Carrier_Id = null;
                            carriergroupingdetails = _carriergroupingDetailsRepository.InsertCarrierGrouping(LOBCarrierTransaction, carriergroupingdetails);
                        }
                        else
                        {
                            foreach (var Carrieritem in lobcarriergrouping.Carrier_Ids)
                            {
                                carriergroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                                carriergroupingdetails.Carrier_Id = _installmentPlansRepository.GetAllCarriers().FirstOrDefault(x => x.CarrierDetail_Id == Carrieritem)?.CarrierDetail_Id;
                                carriergroupingdetails = _carriergroupingDetailsRepository.InsertCarrierGrouping(LOBCarrierTransaction, carriergroupingdetails);
                            }
                        }
                        //carriergroupingdetails = _carriergroupingDetailsRepository.InsertCarrierGrouping(LOBCarrierTransaction, carriergroupingdetails);
                        serviceResponse.Status = true;
                        serviceResponse.Message = "LOB_Carrier_Grouping details updated successfully.";
                    }
                    else
                    {
                        lobcarriergrouping = _lobcarriergroupingRepository.InsertUpdateLOBCarrierGrouping(LOBCarrierTransaction, lobcarriergrouping, userSession.User_Id);
                        if (lobcarriergrouping?.LOB_Carrier_Grouping_Id > 0)
                        {
                            //var AllLobs = LookupUtility.GetAllLOBs().Select(l => l.LOB_Id).ToList();
                            //bool AllSelectedLobs = AllLobs.All(ids => lobcarriergrouping.LOB_Ids != null && lobcarriergrouping.LOB_Ids.Contains(ids));
                            bool AllSelectedLobs = lobcarriergrouping.LOB_Ids != null && lobcarriergrouping.LOB_Ids.Contains(0);
                            bool AllSelectedCarriers = lobcarriergrouping.Carrier_Ids != null && lobcarriergrouping.Carrier_Ids.Contains(0);
                            LOBGroupingDetails lobgroupingdetails = new LOBGroupingDetails();
                            if (AllSelectedLobs)
                            {
                                lobgroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                                lobgroupingdetails.LOB_Id = null;
                                lobgroupingdetails = _lobgroupingDetailsRepository.InsertLOBGrouping(LOBCarrierTransaction, lobgroupingdetails);
                            }
                            else
                            {
                                foreach (var lobitem in lobcarriergrouping.LOB_Ids)
                                {
                                    lobgroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                                    lobgroupingdetails.LOB_Id = LookupUtility.GetAllLOBs().FirstOrDefault(l => l.LOB_Id == lobitem)?.LOB_Id;
                                    lobgroupingdetails = _lobgroupingDetailsRepository.InsertLOBGrouping(LOBCarrierTransaction, lobgroupingdetails);
                                }
                            }
                            CarrierGroupingDetails carriergroupingdetails = new CarrierGroupingDetails();
                            if (AllSelectedCarriers)
                            {
                                carriergroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                                carriergroupingdetails.Carrier_Id = null;
                                carriergroupingdetails = _carriergroupingDetailsRepository.InsertCarrierGrouping(LOBCarrierTransaction, carriergroupingdetails);
                            }
                            else
                            {
                                foreach (var Carrieritem in lobcarriergrouping.Carrier_Ids)
                                {
                                    carriergroupingdetails.LOB_Carrier_Grouping_Id = lobcarriergrouping.LOB_Carrier_Grouping_Id;
                                    carriergroupingdetails.Carrier_Id = _installmentPlansRepository.GetAllCarriers().FirstOrDefault(x => x.CarrierDetail_Id == Carrieritem)?.CarrierDetail_Id;
                                    carriergroupingdetails = _carriergroupingDetailsRepository.InsertCarrierGrouping(LOBCarrierTransaction, carriergroupingdetails);
                                }
                            }
                            serviceResponse.Status = true;
                            serviceResponse.Message = "LOB_Carrier_Grouping details created successfully.";
                        }
                    }
                    LOBCarrierTransaction.Commit();
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Unable to create the LOB_carrier_Grouping.";
                }
            }
            catch (Exception ex)
            {
                if (LOBCarrierTransaction.Connection != null)
                {
                    LOBCarrierTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = "Error while inserting LOB_Carrier_Grouping details.";
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse DeleteLobcarrierGroupDetails(int LOB_Carrier_Grouping_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (LOB_Carrier_Grouping_Id == 0)
                {
                    serviceResponse.Status = false;
                    return serviceResponse;
                }
                else
                {
                    _lobcarriergroupingRepository.DeleteLOBCarrierGrouping(LOB_Carrier_Grouping_Id);
                    serviceResponse.Status = true;
                    serviceResponse.Message = "LOB_Carrier_Grouping details deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public IActionResult LineItemsCOA()
        {
            List<LOBCarrierGrouping> LOBCarrierList = new List<LOBCarrierGrouping>();
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            LOBCarrierList = _lobcarriergroupingRepository.GetAllLOBCarrierGrouping(userSession.ConfigurationClient_Id, clients.AccountingService_Id);
            return PartialView(LOBCarrierList);
        }
        public IActionResult BillingLineItemsCOA(string TabName, int LOB_Carrier_Grouping_Id)
        {
            List<LU_BillingType_LineItems_Directions> LineItemsList = new List<LU_BillingType_LineItems_Directions>();
            List<ChartofAccounts> COAList = LookupUtility.GetAllChartofAccounts();
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            ViewBag.Client_Id = userSession.ConfigurationClient_Id;
            ViewBag.Service_Id = clients.AccountingService_Id;
            ViewBag.TabName = TabName;
            var Clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            List<ChartofAccounts> Accountlist = new List<ChartofAccounts>();
            Accountlist = _chartofAccountsRepository.ChartofAccountsList(clients.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.ExpertInsured);
            ViewBag.ChartOfAccountList = Accountlist;
            if (!string.IsNullOrEmpty(TabName))
            {
                if (TabName == "ARAPLineItems")
                {
                    LineItemsList = _lookupRepository.GetAllLineItems_ByChartOfAccounts_ARAP(userSession.ConfigurationClient_Id);
                }
                else
                {
                    LineItemsList = _lookupRepository.GetAllLineItems_ByChartOfAccounts_FeesTaxes(userSession.ConfigurationClient_Id);
                }
            }
            if (LineItemsList?.Count() > 0)
            {
                foreach (var lineItem in LineItemsList)
                {
                    LU_LineItems_ChartOfAccounts LineCOAIncomeExpense = null;
                    LU_LineItems_ChartOfAccounts LineCOAARAP = null;

                    // BillingType specific AccountingType
                    //var accountingConfig =_lookupRepository.GetClients_AccountingTypes_BillingTypes(userSession.ConfigurationClient_Id,(byte)lineItem.BillingType_Id);
                    //lineItem.AccountingType_Id = accountingConfig?.AccountingType_Id ;

                    LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(userSession.ConfigurationClient_Id, (byte)lineItem.BillingType_Id);
                    if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                    {
                        lineItem.AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                    }
                    else
                    {
                        lineItem.AccountingType_Id = Clients.AccountingType_Id;
                    }

                    var incomeExpenseCOA = _lookupRepository.GetLineItems_ChartOfAccounts(lineItem.BillingType_LineItems_Directions_Id, clients.AccountingService_Id, userSession.ConfigurationClient_Id, LOB_Carrier_Grouping_Id, MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                    lineItem.IncomeExpenseLiabilityCOA_Id = incomeExpenseCOA?.ChartOfAccount_Id;

                    if (lineItem.AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                    {
                        var arapCOA =
                            _lookupRepository.GetLineItems_ChartOfAccounts(lineItem.BillingType_LineItems_Directions_Id, clients.AccountingService_Id, userSession.ConfigurationClient_Id, LOB_Carrier_Grouping_Id, MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable);

                        lineItem.ARAPCOA_Id = arapCOA?.ChartOfAccount_Id;
                    }
                    else
                    {
                        lineItem.ARAPCOA_Id = null; // safety
                    }

                    lineItem.LOB_Carrier_Grouping_Id = LOB_Carrier_Grouping_Id;
                }
            }
            return PartialView(LineItemsList);
        }
        public IActionResult BillingLineItems_Grouping(string TabName)
        {
            ViewBag.TabName = TabName;
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            List<LOBCarrierGrouping> LOBCarrierList = _lobcarriergroupingRepository.GetAllLOBCarrierGrouping(userSession.ConfigurationClient_Id, clients.AccountingService_Id);
            return PartialView(LOBCarrierList);
        }

        public IActionResult LobMapping()
        {
            List<LOBMapping> lobmappings = _lobMappingRepository.GetAllLOBMappings(userSession.ConfigurationClient_Id);
            return PartialView(lobmappings);
        }
        [HttpPost]
        public ServiceResponse LobMapping(LOBMapping lobmapping)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
                if (lobmapping.LOB_Ids.Count > 0)
                {
                    lobmapping.Client_Id = clients.Client_Id;
                    foreach (var lobitem in lobmapping.LOB_Ids)
                    {
                        lobmapping.LOB_Id = (byte)lobitem;
                        lobmapping = _lobMappingRepository.InsertUpdateLOBMapping(lobmapping, userSession.User_Id);
                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = "LOB_Mapping details created successfully.";
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Unable to create the LOB_Mapping.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Error while inserting LOB_Mapping details.";
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse DeleteLobMapping(int LOB_Mapping_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (LOB_Mapping_Id == 0)
                {
                    serviceResponse.Status = false;
                    return serviceResponse;
                }
                else
                {
                    _lobMappingRepository.DeleteLOBMapping(LOB_Mapping_Id);
                    serviceResponse.Status = true;
                    serviceResponse.Message = "LOB_Mapping details deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        #region OperatingBanks Module
        public IActionResult OperatingBanksIndex(int PageNumber, int PageSize, string? SearchValue)
        {
            SearchValue = string.Equals(SearchValue, "null", StringComparison.OrdinalIgnoreCase) ? null : SearchValue;
            int TotalRecords = 0;
            if (PageNumber == 0 || PageSize == 0)
            {
                PageNumber = 1;
                PageSize = 10;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.PageNumber = PageNumber;
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            ViewBag.Client_Id = clients.Client_Id;
            ViewBag.Service_Id = clients.AccountingService_Id;
            ViewBag.PaymentService_Id = clients.PaymentService_Id;
            List<LU_OperatingBanks> operatingbanks = _lookupRepository.GetAllOperatingBanks(PageNumber, PageSize, ref TotalRecords, SearchValue, clients.Client_Id);
            ViewBag.TotalRecords = TotalRecords;
            if (!Is_AjaxRequest)
                return View(operatingbanks);
            return PartialView(operatingbanks);
        }
        [HttpPost]
        public ServiceResponse DefaultOperatingBank(int OperatingBank_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                ClientServiceDefaultOperatingBank defaultbank = new ClientServiceDefaultOperatingBank();
                defaultbank.Client_Id = userSession.ConfigurationClient_Id;
                defaultbank.Service_Id = clients.PaymentService_Id;
                defaultbank.ServiceProviderType_Id = MasterLookup._ServiceProviderTypes.Payments;
                defaultbank.OperatingBank_Id = OperatingBank_Id;
                if (OperatingBank_Id > 0)
                {
                    _clientServiceDefaultOperatingBankRepository.insertUpdateClientServiceDefaultOperatingBank(defaultbank);
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        public IActionResult OperatingBanks(int OperatingBank_Id)
        {
            Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
            LU_OperatingBanks operatingbank = new LU_OperatingBanks();
            if (OperatingBank_Id > 0)
            {
                operatingbank = _lookupRepository.GetOperatingBank(OperatingBank_Id, clients.Client_Id);
            }
            else
            {
                if (operatingbank.Client_Id == 0)
                {
                    operatingbank.Client_Id = clients.Client_Id;
                }
                if (operatingbank.Service_Id == 0)
                {
                    operatingbank.Service_Id = clients.AccountingService_Id;
                }
            }
            if (!Is_AjaxRequest)
                return RedirectToAction("OperatingBanksIndex");
            return PartialView(operatingbank);
        }

        [HttpPost]
        public ServiceResponse OperatingBanks(LU_OperatingBanks operatingbanks)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (operatingbanks != null)
                {
                    Clients clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
                    operatingbanks.Client_Id = clients.Client_Id;
                    operatingbanks = _lookupRepository.InsertUpdateOperatingBanks(operatingbanks);
                    if (operatingbanks?.OperatingBank_Id > 0)
                    {
                        if (operatingbanks.Service_Id == MasterLookup._Services.Intuit || operatingbanks.Service_Id == MasterLookup._Services.Sage)
                        {
                            LU_OperatingBanks_ChartOfAccounts OperatingBanks_ChartOfAccounts = new LU_OperatingBanks_ChartOfAccounts();
                            OperatingBanks_ChartOfAccounts.Client_Id = operatingbanks.Client_Id;
                            OperatingBanks_ChartOfAccounts.Service_Id = operatingbanks.Service_Id;
                            OperatingBanks_ChartOfAccounts.OperatingBank_Id = operatingbanks.OperatingBank_Id;
                            OperatingBanks_ChartOfAccounts.ChartOfAccounts_Id = operatingbanks.OperatingBankCOA_Id;
                            _lookupRepository.Insert_Update_OperatingBanksChartOfAccounts(OperatingBanks_ChartOfAccounts);
                        }

                        serviceResponse.Status = true;
                        serviceResponse.Data = operatingbanks;
                        _memoryCache.Remove("LU_OperatingBanks");
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }

        [HttpPost]
        public ServiceResponse DeleteOperatingBank(int OperatingBank_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (OperatingBank_Id > 0)
                {
                    _lookupRepository.DeleteOperatingBank(OperatingBank_Id, userSession.ConfigurationClient_Id);
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        #endregion

        #region Installment Plan  Module
        public IActionResult InstallmentPlanIndex(int PageNumber, int PageSize, string? Name, string? Code, string? NoOfPayments, string? Carrier)
        {

            Name = string.Equals(Name, "null", StringComparison.OrdinalIgnoreCase) ? null : Name;
            Code = string.Equals(Code, "null", StringComparison.OrdinalIgnoreCase) ? null : Code;
            NoOfPayments = string.Equals(NoOfPayments, "null", StringComparison.OrdinalIgnoreCase) ? null : NoOfPayments;
            Carrier = string.Equals(Carrier, "null", StringComparison.OrdinalIgnoreCase) ? null : Carrier;
            int TotalRecords = 0;
            if (PageNumber == 0 || PageSize == 0)
            {
                PageNumber = 1;
                PageSize = 50;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.PageNumber = PageNumber;
            var hasUniqueCarrierNames = LookupUtility.GetClientSettings(userSession.ConfigurationClient_Id, MasterLookup._ClientSettings.HasUniqueCarrierNames);
            List<LU_PaymentPlans> installmentPlan = _lookupRepository.GetAllInstallmentPlan(userSession.ConfigurationClient_Id, PageNumber, PageSize, ref TotalRecords, Name, Code, NoOfPayments, Carrier, !string.IsNullOrEmpty(hasUniqueCarrierNames));
            ViewBag.TotalRecords = TotalRecords;
            if (!Is_AjaxRequest)
                return View(installmentPlan);
            return PartialView(installmentPlan);
        }
        public IActionResult InstallmentPlan(byte InstallmentPlan_Id)
        {
            LU_PaymentPlans installmentPlan = new LU_PaymentPlans();
            if (InstallmentPlan_Id > 0)
            {
                installmentPlan = _lookupRepository.GetInstallmentPlan(InstallmentPlan_Id);
            }
            var hasUniqueCarrierNames = LookupUtility.GetClientSettings(userSession.ConfigurationClient_Id, MasterLookup._ClientSettings.HasUniqueCarrierNames);
            ViewBag.ClientCarrierList = _installmentPlansRepository.GetAllCarriersByClient_Id(userSession.ConfigurationClient_Id, !string.IsNullOrEmpty(hasUniqueCarrierNames));

            if (!Is_AjaxRequest)
                return RedirectToAction("InstallmentPlanIndex");
            return PartialView(installmentPlan);
        }
        [HttpPost]
        public ServiceResponse InstallmentPlan(LU_PaymentPlans installmentPlan)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                if (installmentPlan != null)
                {

                    var paymentPlansList = LookupUtility.GetAllPaymentPlans();

                    int nextSequence = 1; // default
                    if (paymentPlansList != null && paymentPlansList.Any())
                    {
                        nextSequence = paymentPlansList.Max(p => p.SequenceNo) + 1;
                    }


                    installmentPlan.Client_Id = userSession.ConfigurationClient_Id;
                    installmentPlan.Code = CommonUtility.ConvertPlanNameToCode(installmentPlan.Name);
                    if (installmentPlan.PaymentPlan_Id <= 0)
                    {
                        installmentPlan.SequenceNo = nextSequence;
                    }
                    installmentPlan.IsActive = installmentPlan.Status;
                    var dd = _lookupRepository.GetAllPaymentPlans();
                    bool exists = _lookupRepository.GetAllPaymentPlans()
                                .Any(x => x.Client_Id == userSession.ConfigurationClient_Id
                                && x.Carrier_Id == installmentPlan.Carrier_Id
                                && x.Name == installmentPlan.Name);

                    if (exists && installmentPlan.PaymentPlan_Id < 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "An installment plan with the selected company and carrier already exists.";
                        return serviceResponse;
                    }
                    else if (exists && installmentPlan.PaymentPlan_Id == 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "An installment plan with the selected company and carrier already exists.";
                        return serviceResponse;

                    }

                    installmentPlan = _lookupRepository.InsertUpdateInstallmentPlan(installmentPlan);

                    if (installmentPlan?.PaymentPlan_Id > 0)
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Message = "Installment plan saved successfully.";
                        serviceResponse.Data = installmentPlan;

                        // Clear cache so it reloads with updated data
                        _memoryCache.Remove("LU_PaymentPlans");
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse DeleteInstallmentPlan(byte InstallmentPlan_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (InstallmentPlan_Id > 0)
                {
                    _lookupRepository.DeleteInstallmentPlan(InstallmentPlan_Id);
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        #endregion


        #region LineofBusiness Module
        public IActionResult LineofBusinessIndex(int PageNumber, int PageSize, string? SearchValue)
        {
            SearchValue = string.Equals(SearchValue, "null", StringComparison.OrdinalIgnoreCase) ? null : SearchValue;
            int TotalRecords = 0;
            if (PageNumber == 0 || PageSize == 0)
            {
                PageNumber = 1;
                PageSize = 10;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.PageNumber = PageNumber;
            List<LU_LOBs> lineofbusiness = _lookupRepository.GetAllLineofBusiness(PageNumber, PageSize, ref TotalRecords, SearchValue);
            ViewBag.TotalRecords = TotalRecords;
            if (!Is_AjaxRequest)
                return View(lineofbusiness);
            return PartialView(lineofbusiness);
        }
        public IActionResult LineofBusiness(byte LOB_Id)
        {
            LU_LOBs lineofbusiness = new LU_LOBs();
            if (LOB_Id > 0)
            {
                lineofbusiness = _lookupRepository.GetLineofBusiness(LOB_Id);
            }
            if (!Is_AjaxRequest)
                return RedirectToAction("LineofBusinessIndex");
            return PartialView(lineofbusiness);
        }
        [HttpPost]
        public ServiceResponse LineofBusiness(LU_LOBs lineofbusiness)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (lineofbusiness != null)
                {
                    lineofbusiness = _lookupRepository.InsertUpdateLineofBusiness(lineofbusiness);
                    if (lineofbusiness?.LOB_Id > 0)
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Data = lineofbusiness;
                        _memoryCache.Remove("LU_LOBs");
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse DeleteLineofBusiness(byte LOB_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (LOB_Id > 0)
                {
                    _lookupRepository.DeleteLineofBusiness(LOB_Id);
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        #endregion

        #region LineItems (Fee/Tax)
        public IActionResult LineItemsList(int PageNumber, int PageSize, string? SearchValue)
        {
            SearchValue = string.Equals(SearchValue, "null", StringComparison.OrdinalIgnoreCase) ? null : SearchValue;
            int TotalRecords = 0;
            if (PageNumber == 0 || PageSize == 0)
            {
                PageNumber = MasterLookup._DefaultPageNumber;
                PageSize = MasterLookup._DefaultPageSize;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.PageNumber = PageNumber;
            List<LU_LineItems> LineItems = _lookupRepository.GetPagedLineItemsList(PageNumber, PageSize, ref TotalRecords, SearchValue);
            ViewBag.TotalRecords = TotalRecords;
            if (!Is_AjaxRequest)
                return View(LineItems);
            return PartialView(LineItems);
        }
        public IActionResult LineItem(long LineItem_Id)
        {
            LU_LineItems lineitem = new LU_LineItems();
            if (LineItem_Id > 0)
            {
                lineitem = _lookupRepository.GetLineItem(LineItem_Id);
            }
            if (!Is_AjaxRequest)
                return RedirectToAction("LineItemsList");
            return PartialView(lineitem);
        }
        [HttpPost]
        public ServiceResponse LineItem(LU_LineItems lineItems)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (lineItems != null)
                {
                    lineItems = _lookupRepository.InsertUpdateLineItem(lineItems);
                    if (lineItems?.LineItem_Id > 0)
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Data = lineItems;
                        _memoryCache.Remove("LU_LineItems");
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse DeleteLineItem(long LineItem_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (LineItem_Id > 0)
                {
                    _lookupRepository.DeleteLineItem(LineItem_Id);
                    _memoryCache.Remove("LU_LineItems");
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        #endregion
        public IActionResult LineItemMapping(long LineItems_Clients_Id)
        {
            LU_LineItems_Clients lineItemClient = null;

            if (LineItems_Clients_Id > 0)
            {
                var lineItems_Clients = _lookupRepository.GetLineItemsDirections(LineItems_Clients_Id);

                if (lineItems_Clients != null && lineItems_Clients.Any())
                {
                    // Group all rows by LineItems_Clients_Id — since we want one record per client line item
                    var groupedItem = lineItems_Clients
                        .GroupBy(x => x.LineItems_Clients_Id)
                        .Select(g =>
                        {
                            var first = g.First();

                            // if passthrough, pick EntityType_Id where Direction_Id == 2 (A/P)
                            byte? entityTypeForPassthrough = null;
                            if (first.Is_Passthrough)
                            {
                                entityTypeForPassthrough = g
                                    .Where(x => x.Direction_Id == MasterLookup._Directions.AP)
                                    .Select(x => (byte?)x.EntityType_Id)
                                    .FirstOrDefault();
                            }

                            return new LU_LineItems_Clients
                            {
                                LineItems_Clients_Id = g.Key,
                                Client_Id = first.Client_Id,
                                LineItem_Id = first.LineItem_Id,
                                LineItemName = first.LineItemName,
                                OverridenLineItemName = first.OverridenLineItemName,
                                Is_Passthrough = first.Is_Passthrough,
                                Is_Excluded = first.Is_Excluded,
                                LineItemType_Id = first.LineItemType_Id,
                                Is_Commissionable = first.Is_Commissionable,
                                // only assign EntityType_Id if passthrough and we found a valid value
                                EntityType_Id = entityTypeForPassthrough > 0 ? entityTypeForPassthrough ?? 0 : first.EntityType_Id,

                                LineItems_Directions_Ids = string.Join(",", g.Select(x => x.LineItems_Directions_Id).Distinct()),
                                BillingType_LineItems_Directions_Ids = string.Join(",", g.Select(x => x.BillingType_LineItems_Directions_Id).Distinct()),

                                // ✅ Send ID lists instead of names
                                DirectionList = string.Join(",", g.Select(x => x.Direction_Id).Distinct()),
                                BillTypeList = string.Join(",", g.Select(x => x.BillingType_Id).Distinct())
                            };
                        })
                        .FirstOrDefault();

                    lineItemClient = groupedItem;
                }
            }
            ViewBag.Client_Id = userSession.ConfigurationClient_Id;
            return PartialView(lineItemClient);
        }
        [HttpPost]
        public ServiceResponse LineItemMapping(LU_LineItems_Clients lineItems_Clients)
        {
            ServiceResponse response = new ServiceResponse();
            SqlTransaction sqlTransaction = null;
            try
            {
                bool Is_Update = lineItems_Clients.LineItems_Clients_Id > 0 ? true : false;
                if (lineItems_Clients != null)
                {
                    var allowIsExcludeLineItemMapping = LookupUtility.GetClientSettings(userSession.ConfigurationClient_Id, MasterLookup._ClientSettings.AllowIsExcludeLineItemMapping);
                    var directBillId = MasterLookup._BillingTypes.Direct;
                    if (lineItems_Clients.Is_Excluded && !lineItems_Clients.BillTypeList.Contains(directBillId.ToString()))
                    {
                        if (string.IsNullOrEmpty(allowIsExcludeLineItemMapping))
                        {
                            response.Status = false;
                            response.Message = "The Line Item Excluded option is allowed only for Direct Bill. Please check.";
                            return response;
                        }
                    }
                    var existinglineitems = _lookupRepository.GetLineItemsMapping(lineItems_Clients);
                    if (existinglineitems != null)
                    {
                        response.Status = false;
                        response.Message = "This Line item Mapping already exists. Please check!";
                        return response;
                    }
                    lineItems_Clients.Client_Id = lineItems_Clients.Client_Id == 0 ? userSession.ConfigurationClient_Id : lineItems_Clients.Client_Id;
                    List<byte> BillingtypeList = new List<byte>();
                    BillingtypeList = lineItems_Clients.BillTypeList?.Split(',')?.Select(d => byte.Parse(d.Trim()))?.ToList() ?? new List<byte>();
                    List<byte> DirectionListInts = new List<byte>();
                    DirectionListInts = lineItems_Clients.DirectionList.Split(',').Select(d => byte.Parse(d.Trim())).ToList();
                    List<LU_BillingType_LineItems_Directions> ExistingBillTyps = _lookupRepository.GetExistingBillTypesByLineItems_Clients_Id(lineItems_Clients.LineItems_Clients_Id);
                    ExistingBillTyps = ExistingBillTyps?.Where(x => !BillingtypeList.Contains(x.BillingType_Id))?.ToList();
                    List<LU_LineItems_Directions> ExistingDirections = _lookupRepository.GetExistingDirectionsByLineItems_Clients_Id(lineItems_Clients.LineItems_Clients_Id);
                    ExistingDirections = ExistingDirections?.Where(x => !DirectionListInts.Contains(x.Direction_Id))?.ToList();
                    sqlTransaction = _baseRepository.GetNewTransaction();
                    if (ExistingBillTyps?.Count() > 0)
                    {
                        foreach (var billtype in ExistingBillTyps)
                        {
                            var details = _invoiceRepository.GetInvoiceDetailsByBillingLineItemDirections_Id(billtype.BillingType_LineItems_Directions_Id);
                            if (details?.Count > 0)
                            {
                                response.Status = false;
                                response.Message = "Invoices already exist for the selected combination of Line Item, Direction, and Billing Type. Please check!";
                                return response;
                            }
                            else
                            {
                                _lookupRepository.InactiveExistingBillTypesByBillingType_LineItems_Directions_Id(sqlTransaction, billtype.BillingType_LineItems_Directions_Id);
                            }
                        }
                    }
                    if (ExistingDirections?.Count() > 0)
                    {
                        foreach (var direction in ExistingDirections)
                        {
                            var invoicedetails = _invoiceRepository.GetInvoiceDetailsByLineitemDirection_Id(direction.LineItems_Directions_Id);
                            if (invoicedetails?.Count > 0)
                            {
                                response.Status = false;
                                response.Message = "Invoices already exist for the selected combination of Line Item, Direction, and Billing Type. Please check!";
                                return response;
                            }
                            else
                            {
                                _lookupRepository.InactiveExistingDirectionsByLineItems_Directions_Id(sqlTransaction, direction.LineItems_Directions_Id);
                            }
                        }
                    }
                    lineItems_Clients = _lookupRepository.InsertUpdateLineItems_Clients(sqlTransaction, lineItems_Clients);
                    if (lineItems_Clients.LineItems_Clients_Id > 0)
                    {
                        if (DirectionListInts?.Count > 0)
                        {
                            foreach (var id in DirectionListInts)
                            {
                                if (BillingtypeList.Count > 0)
                                {
                                    foreach (var billingtypeid in BillingtypeList)
                                    {
                                        LU_LineItems_Directions lU_LineItems_Directions = _lookupRepository.GetLineItems_Directions(sqlTransaction, lineItems_Clients.LineItems_Clients_Id, id);
                                        if (lU_LineItems_Directions != null)
                                        {
                                            if (lineItems_Clients.Is_Passthrough)
                                            {
                                                if (billingtypeid == MasterLookup._BillingTypes.Agency || billingtypeid == MasterLookup._BillingTypes.Insured || billingtypeid == MasterLookup._BillingTypes.Direct || billingtypeid == MasterLookup._BillingTypes.AgencyDetailed)
                                                {
                                                    if (id == MasterLookup._Directions.AP)
                                                    {
                                                        lU_LineItems_Directions.EntityType_Id = lineItems_Clients.EntityType_Id;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lU_LineItems_Directions.EntityType_Id = lineItems_Clients.EntityType_Id;
                                            }
                                            lU_LineItems_Directions = _lookupRepository.InsertUpdateLineItems_Directions(sqlTransaction, lU_LineItems_Directions);
                                            LU_BillingType_LineItems_Directions billingType_LineItems_Directions = _lookupRepository.GetBillingType_LineItems_Directions(lU_LineItems_Directions.LineItems_Directions_Id, billingtypeid);
                                            if (billingType_LineItems_Directions != null && !Is_Update)
                                            {
                                                if (sqlTransaction?.Connection != null)
                                                {
                                                    sqlTransaction.Rollback();
                                                }
                                                response.Status = false;
                                                response.Message = "The combination of Line Item, Direction and Billing Type already exists.";
                                                return response;
                                            }
                                            else if (!(billingType_LineItems_Directions != null && Is_Update))
                                            {
                                                billingType_LineItems_Directions = new LU_BillingType_LineItems_Directions();
                                                billingType_LineItems_Directions.LineItems_Directions_Id = lU_LineItems_Directions.LineItems_Directions_Id;
                                                billingType_LineItems_Directions.BillingType_Id = billingtypeid;
                                                _lookupRepository.InsertBillingType_LineItems_Directions(sqlTransaction, billingType_LineItems_Directions);
                                            }
                                        }
                                        else
                                        {
                                            lU_LineItems_Directions = new LU_LineItems_Directions();
                                            lU_LineItems_Directions.LineItems_Clients_Id = lineItems_Clients.LineItems_Clients_Id;
                                            lU_LineItems_Directions.Direction_Id = id;
                                            if (lineItems_Clients.Is_Passthrough)
                                            {
                                                if (billingtypeid == MasterLookup._BillingTypes.Agency || billingtypeid == MasterLookup._BillingTypes.Insured || billingtypeid == MasterLookup._BillingTypes.Direct || billingtypeid == MasterLookup._BillingTypes.AgencyDetailed)
                                                {
                                                    if (id == MasterLookup._Directions.AP)
                                                    {
                                                        lU_LineItems_Directions.EntityType_Id = lineItems_Clients.EntityType_Id;
                                                    }
                                                }
                                            }
                                            lU_LineItems_Directions = _lookupRepository.InsertUpdateLineItems_Directions(sqlTransaction, lU_LineItems_Directions);
                                            if (lU_LineItems_Directions?.LineItems_Directions_Id > 0)
                                            {
                                                LU_BillingType_LineItems_Directions BillTypeDirections = new LU_BillingType_LineItems_Directions();
                                                BillTypeDirections.LineItems_Directions_Id = lU_LineItems_Directions.LineItems_Directions_Id;
                                                BillTypeDirections.BillingType_Id = billingtypeid;
                                                _lookupRepository.InsertBillingType_LineItems_Directions(sqlTransaction, BillTypeDirections);
                                            }
                                        }
                                    }
                                }

                            }
                        }

                    }
                    sqlTransaction.Commit();
                    response.Status = true;
                    response.Message = "Line item mapping saved successfully.";
                }
                else
                {
                    if (sqlTransaction?.Connection != null)
                    {
                        sqlTransaction.Rollback();
                    }
                    response.Status = false;
                    response.Message = "The line items object is null, cannot proceed with insertion.";
                }
            }
            catch (Exception ex)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }
                response.Status = false;
                response.Message = ex.Message;
            }
            _memoryCache.Remove("LU_LineItems");
            _memoryCache.Remove("LU_LineItems_Clients");
            _memoryCache.Remove("LU_LineItems_Directions");
            _memoryCache.Remove("LU_BillingType_LineItems_Directions");
            return response;
        }
        public IActionResult LineItemMappingIndex()
        {
            return PartialView();
        }
        public IActionResult LineItemMappingList()
        {
            List<LU_LineItems_Clients> LineItemClientList = _lookupRepository.GetLineItems_ClientsByClient_Id(userSession.ConfigurationClient_Id);
            return PartialView(LineItemClientList);
        }
        public ServiceResponse DeleteLineItemMapping(long LineItems_Clients_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction sqlTransaction = null;
            try
            {
                if (LineItems_Clients_Id > 0)
                {
                    sqlTransaction = _baseRepository.GetNewTransaction();
                    List<InvoiceDetails> ExistingInvoiceDetails = _invoiceRepository.GetInvoiceDetailsByLineItems_Clients_Id(LineItems_Clients_Id);
                    if (ExistingInvoiceDetails?.Count > 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "There already exists invoices which were generated with this line item combonitaion. Please contact system administrator.";
                        return serviceResponse;
                    }
                    else
                    {
                        _lookupRepository.InactiveLineitems_Clients(sqlTransaction, LineItems_Clients_Id);
                    }
                    sqlTransaction.Commit();
                    _memoryCache.Remove("LU_LineItems_Clients");
                    _memoryCache.Remove("LU_LineItems_Directions");
                    _memoryCache.Remove("LU_BillingType_LineItems_Directions");
                    serviceResponse.Status = true;
                }
                else
                {
                    serviceResponse.Message = "LineItems_Clients_Id cannot be zero.";
                }
            }
            catch (Exception ex)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse ChartofAccountsUsage(List<ChartOfAccountUsageRequest> chartOfAccounts)
        {
            try
            {
                if (chartOfAccounts == null || !chartOfAccounts.Any())
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "No Chart of Accounts found.";
                    return serviceResponse;
                }
                foreach (var item in chartOfAccounts)
                {
                    ChartOfAccountsUsage chartOfAccountsUsage = new ChartOfAccountsUsage
                    {
                        ChartOfAccounts_Id = item.ChartOfAccount_Id,
                        Source_Id = MasterLookup._Sources.ExpertInsured,
                        Status = item.Is_Checked
                    };
                    _chartofAccountsRepository.InsertUpdateChartOfAccountsUsage(chartOfAccountsUsage);
                }
                serviceResponse.Status = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public IActionResult UsersList(int PageNumber, int PageSize, string? SearchValue)
        {
            SearchValue = string.Equals(SearchValue, "null", StringComparison.OrdinalIgnoreCase) ? null : SearchValue;
            int TotalRecords = 0;
            if (PageNumber == 0 || PageSize == 0)
            {
                PageNumber = MasterLookup._DefaultPageNumber;
                PageSize = MasterLookup._DefaultPageSize;
            }
            ViewBag.PageSize = PageSize;
            ViewBag.PageNumber = PageNumber;
            List<Users> users = _usersRepository.GetUserList(PageNumber, PageSize, ref TotalRecords, SearchValue);
            ViewBag.TotalRecords = TotalRecords;
            if (!Is_AjaxRequest)
                return View(users);
            return PartialView(users);
        }
        public IActionResult User(long User_Id)
        {
            Users users = new Users();
            users = _usersRepository.GetUserById(User_Id) ?? new Users();
            List<Clients> clients = _accountRepository.GetAllClientsAssociatedToUser_Id(userSession.User_Id);
            ViewBag.ClientList = clients;
            List<Clients> SelectedClientsList = _accountRepository.GetAllClientsAssociatedToUser_Id(User_Id);
            ViewBag.SelectedClientsList = SelectedClientsList;
            if (SelectedClientsList?.Count > 0)
            {
                users.ClientIdList = string.Join(",", SelectedClientsList.Select(x => x.Client_Id).Distinct());
            }
            else
            {
                users.ClientIdList = users.Client_Id.ToString();
            }
            return PartialView(users);
        }
        [HttpPost]
        public ServiceResponse User(Users users)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (users != null)
                {
                    var selectedClientIds = users.ClientIdList?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)?
            .Select(x => long.Parse(x.Trim()))?
            .Distinct()?
            .ToList() ?? new List<long>();
                    long? Client_Id = null;
                    if (selectedClientIds?.Count > 0)
                    {
                        Client_Id = selectedClientIds[0];
                    }
                    users = _usersRepository.InsertUpdateUsers(users, Client_Id, selectedClientIds);
                    if (users?.User_Id > 0)
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Data = users;
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }
        public ServiceResponse DeleteUser(long User_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (User_Id > 0)
                {
                    _usersRepository.DeleteUser(User_Id);
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }

        #region Commision Rules
        public IActionResult CommissionRulesList(int PageNumber, int PageSize, string SearchValue = "")
        {
            SearchValue = string.Equals(SearchValue, "null", StringComparison.OrdinalIgnoreCase) ? string.Empty : SearchValue;
            int TotalRecords = 0;
            if (PageNumber == 0 || PageSize == 0)
            {
                PageNumber = MasterLookup._DefaultPageNumber;
                PageSize = MasterLookup._DefaultPageSize;
            }

            ViewBag.PageSize = PageSize;
            ViewBag.PageNumber = PageNumber;

            List<CommissionRules> details = _commissionRulesRepository.GetCommissionRules(PageNumber, PageSize, ref TotalRecords, SearchValue, userSession.ConfigurationClient_Id, MasterLookup._EntityTypes.Underwriter);

            ViewBag.TotalRecords = TotalRecords;

            if (!Is_AjaxRequest)
                return View(details);
            return PartialView(details);
        }

        public IActionResult GetCommissionRule(long CommissionRule_Id)
        {
            CommissionRules details = new();
            if (CommissionRule_Id > 0)
            {
                details = _commissionRulesRepository.GetCommissionRule(CommissionRule_Id);
            }

            var clientId = userSession.ConfigurationClient_Id;

            ViewBag.Client_Id = clientId;
            ViewBag.Underwriters = _commissionRulesRepository.GetAllUnderWriters(clientId);
            ViewBag.Lobs = _lobMappingRepository.GetAllLOBMappings(userSession.ConfigurationClient_Id).OrderBy(l => l.Name).ToList();
            if (!Is_AjaxRequest)
                return RedirectToAction("CommissionRulesList");
            return PartialView("CommissionRule", details);
        }

        public ServiceResponse DeleteCommissionRule(long CommissionRule_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (CommissionRule_Id > 0)
                {
                    int result = _commissionRulesRepository.DeleteCommissionRule(CommissionRule_Id);
                    if (result > 0)
                        serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }

        [HttpPost]
        public ServiceResponse SaveCommissionRules(CommissionRules commissionRules)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction commissionRuleTransaction = _baseRepository.GetNewTransaction();

            try
            {
                if (commissionRules == null)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid Data Received";
                }

                if (commissionRules != null)
                {
                    string addedOrUpdated = string.Empty;

                    if (commissionRules.CommissionRule_Id == 0)
                    {
                        List<LU_SubmissionTypes> submissionTypes = LookupUtility.GetAllSubmissionTypes();

                        if (IsCommissionRuleAlreadyExists(commissionRules))
                        {
                            serviceResponse.Message = "Commission Rule Aleady Exists";
                            serviceResponse.Status = false;
                            return serviceResponse;
                        }

                        SetSubmissionTypeFlags(commissionRules, submissionTypes);

                        addedOrUpdated = "added";
                    }

                    if (commissionRules.CommissionRule_Id > 0)
                    {
                        addedOrUpdated = "updated";
                    }


                    commissionRules.EntityType_Id = commissionRules.EntityType_Id;

                    commissionRules = _commissionRulesRepository.InsertUpdateCommissionRule(commissionRuleTransaction, commissionRules);

                    if (commissionRules?.CommissionRule_Id > 0)
                    {
                        commissionRuleTransaction.Commit();
                        serviceResponse.Status = true;
                        serviceResponse.Data = commissionRules;
                        serviceResponse.Message = $"Commission Rule has been {addedOrUpdated} successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                commissionRuleTransaction.Rollback();
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = false;
            }
            return serviceResponse;
        }

        [NonAction]
        private void SetSubmissionTypeFlags(CommissionRules commissionRules, List<LU_SubmissionTypes> submissionTypes)
        {
            if (commissionRules.SubmissionType_Ids.Count == submissionTypes.Count)
            {
                commissionRules.ApplyOnAllTransactions = true;
                commissionRules.SubmissionTypeIds = null;
            }
            else
            {
                commissionRules.ApplyOnAllTransactions = false;
                commissionRules.SubmissionTypeIds = string.Join(",", commissionRules.SubmissionType_Ids);
            }
        }

        [NonAction]
        private bool IsCommissionRuleAlreadyExists(CommissionRules commissionRules)
        {
            var existingCommissionRules = _commissionRulesRepository.GetCommissionRules(MasterLookup._EntityTypes.Underwriter, commissionRules.Entity_Id, commissionRules.Client_Id);

            if (existingCommissionRules == null || !existingCommissionRules.Any())
                return false;

            // Already exists for ALL submission types
            bool existsForAll = existingCommissionRules.Any(rule =>
                rule.Client_Id == commissionRules.Client_Id &&
                rule.EntityType_Id == MasterLookup._EntityTypes.Underwriter &&
                rule.Entity_Id == commissionRules.Entity_Id &&
                rule.SubmissionType_Id == null &&
                rule.Status == true
            );

            if (existsForAll)
                return true;

            // Specific submission type match

            bool existsForSpecific = false;

            foreach (int submissionTypeId in commissionRules.SubmissionType_Ids.ToList())
            {
                existsForSpecific = existingCommissionRules.Any(rule =>
                                            rule.Client_Id == commissionRules.Client_Id &&
                                            rule.EntityType_Id == MasterLookup._EntityTypes.Underwriter &&
                                            rule.Entity_Id == commissionRules.Entity_Id &&
                                            rule.SubmissionType_Id == submissionTypeId &&
                                            rule.Status == true
                                        );

                if (existsForSpecific)
                    commissionRules.SubmissionType_Ids.Remove(submissionTypeId);

            }

            return commissionRules.SubmissionType_Ids.Count > 0 ? false : true;
        }
        #endregion
    }
}
