using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.CorePay;
using Accounting.Entities.External.CoverPay;
using Accounting.Entities.External.EI;
using Accounting.Entities.External.ePay;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.External.Sage.Journal;
using Accounting.Entities.Internal;
using Accounting.Entities.Internal.Enums;
using Accounting.Utilities.External.QBO;
using Accounting.Utilities.External.Sage;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Abstract;
using Accounting.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Accounting.Web.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FinancialServicesController : ControllerBase
    {
        private readonly ViewRender view;
        private readonly IWebHostEnvironment _env;
        private readonly IPDFOptions _IPDFOptions;
        internal readonly IPolicyDetailsRepository _policyDetailsRepository = new PolicyDetailsRepository();
        internal readonly ILOBMappingRepository _lobMappingRepository = new LOBMappingRepository();
        internal readonly IInvoiceRepository _invoiceRepository = new InvoiceRepository();
        internal readonly ILineItemTypesRepository _lineItemRepository = new LineItemTypesRepository();
        internal readonly IPaymentsRepository _paymentsRepository = new PaymentsRepository();
        internal readonly IBaseRepository _baseRepository = new BaseRepository();
        internal readonly IClientsRepository _accountRepository = new ClientsRepository();
        internal readonly IUsersRepository _usersRepository = new UsersRepository();
        internal readonly ILookupRepository _lookupRepository = new LookupRepository();
        internal readonly IJournalEntryLogRepository _journalEntryLogRepository = new JournalEntryLogRepository();
        internal readonly IInstallmentPlansRepository _installmentPlansRepository = new InstallmentPlansRepository();
        internal readonly IPostCashReceiptsRepository _postCashReceiptsRepository = new PostCashReceiptsRepository();
        internal readonly IIncomingRequestsRepository _incomingRequestsRepository = new IncomingRequestsRepository();
        internal readonly IClientServiceDefaultOperatingBankRepository _clientServiceDefaultOperatingBankRepository = new ClientServiceDefaultOperatingBankRepository();
        internal readonly IExceptionLogInfoRepository _exceptionLogInfoRepository = new ExceptionLogInfoRepository();
        internal readonly IExternalAccountInformationRepository _externalAccountInformationRepository = new ExternalAccountInformationRepository();
        internal readonly ICommissionRulesRepository _commissionRulesRepository = new CommissionRulesRepository();
        private readonly ICoverPayService coverPayService;
        public FinancialServicesController(ViewRender view, IPDFOptions iPDFOptions, IWebHostEnvironment env, ICoverPayService coverPayService)
        {
            this.view = view;
            _IPDFOptions = iPDFOptions;
            _env = env;
            this.coverPayService = coverPayService;
        }
        [HttpPost]
        public ServiceResponse GenerateInvoice([FromBody] JsonElement data)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            CreateJournalRes createInvoiceJournalResponse = new CreateJournalRes();
            CreateJournalRes updateInvoiceJournalResponse = new CreateJournalRes();
            DeleteJournalResponse deleteJournalResponse = new DeleteJournalResponse();
            AccountingPost_Req accountingPost_Req = new AccountingPost_Req();
            string InvoiceNumber = "";
            string? InvoiceDate = null;
            string? PaymentDueOn = null;
            string? NoOfInstallments = null;
            long Invoice_Id = 0;
            List<long> invoicesList = new List<long>();
            byte[] pdfbytes = null;
            string username = User.Identity?.Name;
            byte _AccountingType_Id = 0;
            DateTime cancelDate;
            if (!string.IsNullOrEmpty(username))
            {
                var AuthenticatedUser = _usersRepository.GetUsers(username);
                var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);
                if (Clients != null)
                {
                    IncomingRequests incomingRequests = new IncomingRequests();
                    incomingRequests.Service_Id = MasterLookup._Services.ePay;
                    incomingRequests.RequestBody = data.ToString();
                    incomingRequests.CreatedBy = AuthenticatedUser.Username;
                    incomingRequests = _incomingRequestsRepository.InsertIncomingRequests(incomingRequests);
                    SqlTransaction PolicyDetailsTransaction = null;
                    PolicyDetailsTransaction = _baseRepository.GetNewTransaction();
                    if (PolicyDetailsTransaction == null)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                        return serviceResponse;
                    }
                    try
                    {
                        string rawJson = data.GetRawText();
                        accountingPost_Req = JsonConvert.DeserializeObject<AccountingPost_Req>(rawJson);
                        if (accountingPost_Req != null)
                        {
                            if (!string.IsNullOrEmpty(accountingPost_Req.SubmissionId) && !string.IsNullOrEmpty(accountingPost_Req.PolicyNumber) && !string.IsNullOrEmpty(accountingPost_Req.QuoteId))
                            {
                                PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetailsBySubmission_Id(Convert.ToInt64(accountingPost_Req.SubmissionId), Clients.Client_Id) ?? new PolicyDetails();
                                AgentDetails agentDetails = new AgentDetails();
                                AgentBrokerContactDetails agentBrokerContactDetails = new AgentBrokerContactDetails();
                                ParentAgentDetails parentAgentDetails = new ParentAgentDetails();
                                CarrierDetails carrierDetails = new CarrierDetails();
                                WholesalerDetails wholesalerDetails = new WholesalerDetails();
                                FinanceDetails financeDetails = new FinanceDetails();
                                UnderWriterDetails underWriterDetails = new UnderWriterDetails();
                                InsuredDetails insuredDetails = new InsuredDetails();
                                InterestDetails interestDetails = new InterestDetails();
                                MortgageeDetails mortgageeDetails = new MortgageeDetails();
                                SalesRepDetails salesRepDetails = new();
                                ReferralUnderwriter referralUnderwriter = new ReferralUnderwriter();
                                byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == accountingPost_Req.BillType)?.BillingType_Id ?? 0;
                                var PaymentApplicationsList = _paymentsRepository.GetAllPaymentApplicationLineItemsByPolicyDetail_Id(policyDetails.Id);
                                if (PaymentApplicationsList?.Count() > 0)
                                {
                                    if (PolicyDetailsTransaction.Connection != null)
                                    {
                                        PolicyDetailsTransaction.Rollback();
                                    }
                                    serviceResponse.Status = false;
                                    serviceResponse.Message = $"Payment applications already exist for Invoice number(s): {string.Join(", ", PaymentApplicationsList.Select(x => x.InvoiceNo).Distinct())} (Policy Number '{policyDetails.PolicyNumber}').";
                                    return serviceResponse;
                                }
                                if (BillingType_Id == MasterLookup._BillingTypes.DoNotBill)
                                {
                                    if (policyDetails.Id > 0)
                                    {
                                        var policyInvoices = _invoiceRepository.GetAllInvoicesByPolicyDetail_Id(policyDetails.Id);
                                        foreach (var invoice in policyInvoices)
                                        {
                                            JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                            if (journalEntries != null)
                                            {
                                                if (Clients.AccountingService_Id == MasterLookup._Services.Intuit && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                {
                                                    QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                    if (qBOUtility != null)
                                                    {
                                                        DeleteJournalResponse deleteExistingJournalResponse = new DeleteJournalResponse();
                                                        deleteExistingJournalResponse = qBOUtility.DeleteJournalEntry(journalEntries);
                                                        if (deleteExistingJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(deleteExistingJournalResponse?.JournalEntry?.Id))
                                                        {
                                                            if (PolicyDetailsTransaction?.Connection != null)
                                                            {
                                                                PolicyDetailsTransaction.Rollback();
                                                            }
                                                            serviceResponse.Status = false;
                                                            serviceResponse.Message = "Cannot sync invoice with Intuit!" + deleteExistingJournalResponse?.Fault?.Error?[0]?.Detail;
                                                            return serviceResponse;
                                                        }
                                                        else if (deleteJournalResponse?.JournalEntry?.Id != null && !string.IsNullOrEmpty(deleteJournalResponse?.JournalEntry?.Id))
                                                        {
                                                            _journalEntryLogRepository.DeleteJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                                        }
                                                    }

                                                }
                                                else if (Clients.AccountingService_Id == MasterLookup._Services.Sage && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                {
                                                    SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                    if (sageUtility != null)
                                                    {
                                                        var deleteJournalRes = Task.Run(() => sageUtility.DeleteJournalEntry_Intacct(journalEntries)).Result;
                                                        var serializer = new XmlSerializer(typeof(SageResponse));
                                                        SageResponse createSageJournalResponse = new SageResponse();
                                                        if (!string.IsNullOrEmpty(deleteJournalRes))
                                                        {
                                                            using (var reader = new StringReader(deleteJournalRes))
                                                            {
                                                                createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                            }
                                                            if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                            {
                                                                if (PolicyDetailsTransaction?.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }
                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Sage!";
                                                                return serviceResponse;
                                                            }

                                                            var xmlDoc = XDocument.Parse(deleteJournalRes);
                                                            var status = xmlDoc.Descendants("status").FirstOrDefault(x => x.Parent?.Name == "result")?.Value;
                                                            if (status == "failure")
                                                            {
                                                                if (PolicyDetailsTransaction?.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }
                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Sage!";
                                                                return serviceResponse;
                                                            }
                                                            else if (status == "success")
                                                            {
                                                                _journalEntryLogRepository.DeleteJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                                            }
                                                            else
                                                            {
                                                                if (PolicyDetailsTransaction?.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Unexpected response from Sage.";
                                                                return serviceResponse;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                                    {
                                                        QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                        if (qBOUtility != null)
                                                        {
                                                            DeleteJournalResponse deleteExistingJournalResponse = new DeleteJournalResponse();
                                                            deleteExistingJournalResponse = qBOUtility.DeleteJournalEntry(journalEntries);
                                                            if (deleteExistingJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(deleteExistingJournalResponse?.JournalEntry?.Id))
                                                            {
                                                                if (PolicyDetailsTransaction?.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Intuit!" + deleteExistingJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                return serviceResponse;
                                                            }
                                                        }
                                                    }
                                                    else if (Clients.AccountingService_Id == MasterLookup._Services.Sage)
                                                    {
                                                        SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                        if (sageUtility != null)
                                                        {
                                                            DeleteJournalResponse deleteExistingJournalResponse = new();
                                                            var deleteExistingJournalRes = Task.Run(() => sageUtility.DeleteJournalEntry_Intacct(journalEntries)).Result;

                                                            var serializer = new XmlSerializer(typeof(SageResponse));
                                                            SageResponse deleteSageJournalResponse = new SageResponse();
                                                            using (var reader = new StringReader(deleteExistingJournalRes))
                                                            {
                                                                deleteSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                            }
                                                            if (deleteSageJournalResponse == null || deleteSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                            {
                                                                if (PolicyDetailsTransaction?.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Sage!";
                                                                return serviceResponse;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        _policyDetailsRepository.InactivePolicyDetails(PolicyDetailsTransaction, policyDetails.Id, Clients.Client_Id);
                                    }

                                    if (PolicyDetailsTransaction.Connection != null)
                                    {
                                        PolicyDetailsTransaction.Rollback();
                                    }
                                    serviceResponse.Status = true; //continue
                                    serviceResponse.Message = "Do Not Bill";
                                    return serviceResponse;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Agent) && !string.IsNullOrEmpty(accountingPost_Req.AgentName))
                                {
                                    long? parentAgencyId = null;

                                    if (!string.IsNullOrEmpty(accountingPost_Req.ParentAgent) &&
                                        !string.IsNullOrEmpty(accountingPost_Req.ParentAgentName))
                                    {
                                        var parentAgency = new ParentAgentDetails
                                        {
                                            AgentReference_Id = accountingPost_Req.ParentAgent,
                                            Name = accountingPost_Req.ParentAgentName,
                                            Email = accountingPost_Req.ParentAgentEmail,
                                            FEIN = accountingPost_Req.ParentAgentFEIN,
                                            Address1 = accountingPost_Req.ParentAgentAddress1,
                                            Address2 = accountingPost_Req.ParentAgentAddress2,
                                            County = accountingPost_Req.ParentAgentCounty,
                                            City = accountingPost_Req.ParentAgentCity,
                                            State = accountingPost_Req.ParentAgentState,
                                            Country = accountingPost_Req.ParentAgentCountry,
                                            ZipCode = accountingPost_Req.ParentAgentZipCode,
                                            Client_Id = AuthenticatedUser.Client_Id
                                        };

                                        parentAgency = _policyDetailsRepository.InsertUpdateParentAgentDetails(PolicyDetailsTransaction, parentAgency);

                                        parentAgencyId = parentAgency.ParentAgentDetail_Id;
                                    }
                                    if (!string.IsNullOrEmpty(accountingPost_Req.AgentSalesrep) && !string.IsNullOrEmpty(accountingPost_Req.SalesRepReferenceId))
                                    {
                                        salesRepDetails = new SalesRepDetails
                                        {
                                            Name = accountingPost_Req.AgentSalesrep,
                                            Client_Id = Clients.Client_Id,
                                            SalesRepReference_Id = accountingPost_Req.SalesRepReferenceId
                                        };
                                        salesRepDetails = _policyDetailsRepository.InsertUpdateSalesRepDetails(PolicyDetailsTransaction, salesRepDetails);
                                    }
                                    policyDetails.SalesRepDetails = salesRepDetails;

                                    agentDetails.AgentReference_Id = accountingPost_Req.Agent;
                                    agentDetails.Name = accountingPost_Req.AgentName;
                                    agentDetails.Address1 = accountingPost_Req.AgentAddress1;
                                    agentDetails.Address2 = accountingPost_Req.AgentAddress2;
                                    agentDetails.City = accountingPost_Req.AgentCity;
                                    agentDetails.Country = accountingPost_Req.AgentCountry;
                                    agentDetails.County = accountingPost_Req.AgentCounty;
                                    agentDetails.State = accountingPost_Req.AgentState;
                                    agentDetails.ZipCode = accountingPost_Req.AgentZipCode;
                                    agentDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    agentDetails.Email = accountingPost_Req.AgentEmail;
                                    agentDetails.FEIN = accountingPost_Req.AgentFEIN;
                                    agentDetails.EI_Accounting_Id = accountingPost_Req.AgentAccountingID;
                                    agentDetails.AgentAccountNumber = accountingPost_Req.AgentAccountNumber;
                                    agentDetails.AgentRoutingNumber = accountingPost_Req.AgentRoutingNumber;
                                    agentDetails.BankName = accountingPost_Req.AgentBankName;
                                    agentDetails.AccountTypeName = accountingPost_Req.AgentAccountTypeName;
                                    agentDetails.SalesRep = accountingPost_Req.AgentSalesrep;
                                    agentDetails.AssignedSalesRep_Id = salesRepDetails.Id;
                                    agentDetails.ParentAgencyDetails_Id = parentAgencyId;
                                    agentDetails.ProducerType = accountingPost_Req.ProducerType;
                                    agentDetails.DBA = accountingPost_Req.AgentDBA;
                                    agentDetails.AgreementDate = accountingPost_Req.AgentAgreementDate;
                                    agentDetails.CompanyType = accountingPost_Req.AgentCompanyType;
                                    agentDetails.AgentStatus = accountingPost_Req.AgentStatus;
                                    agentDetails.Phone = accountingPost_Req.AgentPhone;
                                    agentDetails = _policyDetailsRepository.InsertUpdateAgentDetails(PolicyDetailsTransaction, agentDetails);
                                    policyDetails.AgentDetails = agentDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.AgentBrokerContactName) && !string.IsNullOrEmpty(accountingPost_Req.AgentBrokerContactEmail))
                                {
                                    agentBrokerContactDetails.AgentBrokerContactReference_Id = accountingPost_Req?.AgentBrokerContactId ?? 0;
                                    agentBrokerContactDetails.Name = accountingPost_Req.AgentBrokerContactName;
                                    agentBrokerContactDetails.Email = accountingPost_Req.AgentBrokerContactEmail;
                                    agentBrokerContactDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    agentBrokerContactDetails = _policyDetailsRepository.InsertUpdateAgentBrokerContactDetails(PolicyDetailsTransaction, agentBrokerContactDetails, AuthenticatedUser.User_Id);
                                    policyDetails.AgentBrokerContactDetails = agentBrokerContactDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Carrier) && !string.IsNullOrEmpty(accountingPost_Req.CarrierName))
                                {
                                    carrierDetails.CarrierReference_Id = accountingPost_Req.Carrier;
                                    carrierDetails.Name = accountingPost_Req.CarrierName;
                                    carrierDetails.Address1 = accountingPost_Req.CarrierAddress1;
                                    carrierDetails.Address2 = accountingPost_Req.CarrierAddress2;
                                    carrierDetails.City = accountingPost_Req.CarrierCity;
                                    carrierDetails.Country = accountingPost_Req.CarrierCountry;
                                    carrierDetails.County = accountingPost_Req.CarrierCounty;
                                    carrierDetails.State = accountingPost_Req.CarrierState;
                                    carrierDetails.ZipCode = accountingPost_Req.CarrierZipCode;
                                    carrierDetails.CarrierStatus = accountingPost_Req.CarrierStatus;
                                    carrierDetails.NAIC = accountingPost_Req.CarrierNAIC;
                                    carrierDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    carrierDetails = _policyDetailsRepository.InsertUpdateCarrierDetails(PolicyDetailsTransaction, carrierDetails);
                                    policyDetails.CarrierDetails = carrierDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Wholesaler) && !string.IsNullOrEmpty(accountingPost_Req.WholesalerName))
                                {
                                    wholesalerDetails.WholesalerReference_Id = accountingPost_Req.Wholesaler;
                                    wholesalerDetails.Name = accountingPost_Req.WholesalerName;
                                    wholesalerDetails.DisplayName = accountingPost_Req.WholesalerDisplayName;
                                    wholesalerDetails.ParentID = string.IsNullOrEmpty(accountingPost_Req.WholesalerParentID) ? null : Convert.ToInt32(accountingPost_Req.WholesalerParentID);
                                    wholesalerDetails.IsActive = accountingPost_Req.WholesalerIsActive == "1";
                                    wholesalerDetails.AlternateID = accountingPost_Req.WholesalerAlternateID;
                                    wholesalerDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    wholesalerDetails = _policyDetailsRepository.InsertUpdateWholesalerDetails(PolicyDetailsTransaction, wholesalerDetails);
                                    policyDetails.WholesalerDetails = wholesalerDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Finance) && !string.IsNullOrEmpty(accountingPost_Req.FinanceName))
                                {
                                    financeDetails.FinanceReference_Id = accountingPost_Req.Finance;
                                    financeDetails.Name = accountingPost_Req.FinanceName;
                                    financeDetails.Address1 = accountingPost_Req.FinanceAddress1;
                                    financeDetails.Address2 = accountingPost_Req.FinanceAddress2;
                                    financeDetails.City = accountingPost_Req.FinanceCity;
                                    financeDetails.Country = accountingPost_Req.FinanceCountry;
                                    financeDetails.County = accountingPost_Req.FinanceCounty;
                                    financeDetails.State = accountingPost_Req.FinanceState;
                                    financeDetails.ZipCode = accountingPost_Req.FinanceZipCode;
                                    financeDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    financeDetails = _policyDetailsRepository.InsertUpdateFinanceDetails(PolicyDetailsTransaction, financeDetails);
                                    policyDetails.FinanceDetails = financeDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.UW) && !string.IsNullOrEmpty(accountingPost_Req.UWName))
                                {
                                    underWriterDetails.UWReference_Id = accountingPost_Req.UW;
                                    underWriterDetails.Name = accountingPost_Req.UWName;
                                    underWriterDetails.Address1 = accountingPost_Req.UWAddress1;
                                    underWriterDetails.Address2 = accountingPost_Req.UWAddress2;
                                    underWriterDetails.City = accountingPost_Req.UWCity;
                                    underWriterDetails.Country = accountingPost_Req.UWCountry;
                                    underWriterDetails.County = accountingPost_Req.UWCounty;
                                    underWriterDetails.State = accountingPost_Req.UWState;
                                    underWriterDetails.ZipCode = accountingPost_Req.UWZipCode;
                                    underWriterDetails.ConsolidatedGuid = accountingPost_Req.ConsolidatedGuid;
                                    underWriterDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    underWriterDetails.AssignUserName = accountingPost_Req.AssignUserName;
                                    underWriterDetails.AssignUser_Id = Convert.ToInt64(accountingPost_Req.AssignUser_Id);
                                    underWriterDetails = _policyDetailsRepository.InsertUpdateUnderWriterDetails(PolicyDetailsTransaction, underWriterDetails);
                                    policyDetails.UnderWriterDetails = underWriterDetails;


                                    // Inserting Default Underwriter Commision Rule 
                                    if (underWriterDetails.UnderWriterDetail_Id > 0)
                                    {
                                        string defCommissionVal = LookupUtility.GetClientSettings(AuthenticatedUser.Client_Id, MasterLookup._ClientSettings.DefaultCommissionRule);

                                        if (!string.IsNullOrWhiteSpace(defCommissionVal) && decimal.TryParse(defCommissionVal, out decimal defVal))
                                        {
                                            long entityID = underWriterDetails.UnderWriterDetail_Id;
                                            long clientID = AuthenticatedUser.Client_Id;

                                            var existingCommissionRules = _commissionRulesRepository.GetCommissionRules(MasterLookup._EntityTypes.Underwriter, entityID, clientID);

                                            if (existingCommissionRules == null || existingCommissionRules?.Count == 0)
                                            {
                                                SqlTransaction commissionRuleTransaction = _baseRepository.GetNewTransaction();
                                                try
                                                {
                                                    CommissionRules commissionRules = new()
                                                    {
                                                        Client_Id = clientID,
                                                        EntityType_Id = MasterLookup._EntityTypes.Underwriter,
                                                        Entity_Id = entityID,
                                                        CommissionType_Id = MasterLookup._CommissionTypes.PERCENT,
                                                        Value = defVal,
                                                        ApplyOnAllTransactions = true,
                                                        CommissionBasisType_Id = MasterLookup._CommissionBasisTypes.HouseCommission,
                                                    };

                                                    commissionRules = _commissionRulesRepository.InsertUpdateCommissionRule(commissionRuleTransaction, commissionRules);

                                                    if (commissionRules?.CommissionRule_Id > 0)
                                                    {
                                                        commissionRuleTransaction.Commit();

                                                    }
                                                }
                                                catch
                                                {
                                                    commissionRuleTransaction.Rollback();
                                                }

                                            }
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Insured) && !string.IsNullOrEmpty(accountingPost_Req.InsuredName))
                                {
                                    insuredDetails = _policyDetailsRepository.GetInsuredDetails(policyDetails.InsuredDetail_Id) ?? new InsuredDetails();
                                    insuredDetails.InsuredReference_Id = accountingPost_Req.Insured;
                                    insuredDetails.Name = accountingPost_Req.InsuredName;
                                    insuredDetails.Address1 = accountingPost_Req.InsuredAddress1;
                                    insuredDetails.Address2 = accountingPost_Req.InsuredAddress2;
                                    insuredDetails.City = accountingPost_Req.InsuredCity;
                                    insuredDetails.Country = accountingPost_Req.InsuredCountry;
                                    insuredDetails.County = accountingPost_Req.InsuredCounty;
                                    insuredDetails.State = accountingPost_Req.InsuredState;
                                    insuredDetails.ZipCode = accountingPost_Req.InsuredZipCode;
                                    insuredDetails.FEIN = accountingPost_Req.InsuredFEIN;
                                    insuredDetails.InsuredDBA = accountingPost_Req.InsuredDBA;
                                    insuredDetails.Physical_Address1 = accountingPost_Req.InsuredPhysicalAddress1;
                                    insuredDetails.Physical_Address2 = accountingPost_Req.InsuredPhysicalAddress2;
                                    insuredDetails.Physical_City = accountingPost_Req.InsuredPhysicalCity;
                                    insuredDetails.Physical_State = accountingPost_Req.InsuredPhysicalState;
                                    insuredDetails.Physical_ZipCode = accountingPost_Req.InsuredPhysicalZipCode;
                                    insuredDetails.Physical_Country = accountingPost_Req.InsuredPhysicalZipCode;
                                    insuredDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    insuredDetails.BusinessDescription = accountingPost_Req.InsuredBusinessDescription;
                                    insuredDetails.Email = accountingPost_Req?.InsuredEmail;
                                    insuredDetails.Phone = accountingPost_Req.InsuredPhone;
                                    insuredDetails.ReferralSource = accountingPost_Req.InsuredReferralSource;
                                    insuredDetails = _policyDetailsRepository.InsertUpdateInsuredDetails(PolicyDetailsTransaction, insuredDetails);
                                    policyDetails.InsuredDetails = insuredDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Interest) && !string.IsNullOrEmpty(accountingPost_Req.InterestTypeID) && !string.IsNullOrEmpty(accountingPost_Req.InterestName))
                                {
                                    interestDetails = _policyDetailsRepository.GetInterestDetails(policyDetails.InterestDetail_Id ?? 0) ?? new InterestDetails();
                                    interestDetails.InterestReference_Id = accountingPost_Req.Interest;
                                    interestDetails.TypeID = accountingPost_Req.InterestTypeID;
                                    interestDetails.Name = accountingPost_Req.InterestName;
                                    interestDetails.Address1 = accountingPost_Req.InterestAddress1;
                                    interestDetails.Address2 = accountingPost_Req.InterestAddress2;
                                    interestDetails.City = accountingPost_Req.InterestCity;
                                    interestDetails.Country = accountingPost_Req.InterestCountry;
                                    interestDetails.County = accountingPost_Req.InterestCounty;
                                    interestDetails.State = accountingPost_Req.InterestState;
                                    interestDetails.ZipCode = accountingPost_Req.InterestZipCode;
                                    interestDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    interestDetails = _policyDetailsRepository.InsertUpdateInterestDetails(PolicyDetailsTransaction, interestDetails);
                                    policyDetails.InterestDetails = interestDetails;
                                }
                                if (!string.IsNullOrEmpty(accountingPost_Req.Mortgagee) && !string.IsNullOrEmpty(accountingPost_Req.MortgageeTypeID) && !string.IsNullOrEmpty(accountingPost_Req.MortgageeName))
                                {
                                    mortgageeDetails = _policyDetailsRepository.GetMortgageeDetails(policyDetails.MortgageeDetail_Id ?? 0) ?? new MortgageeDetails();
                                    mortgageeDetails.MortgageeReference_Id = accountingPost_Req.Mortgagee;
                                    mortgageeDetails.MortgageeType_Id = accountingPost_Req.MortgageeTypeID;
                                    mortgageeDetails.Name = accountingPost_Req.MortgageeName;
                                    mortgageeDetails.Address1 = accountingPost_Req?.MortgageeAddress1;
                                    mortgageeDetails.Address2 = accountingPost_Req?.MortgageeAddress2;
                                    mortgageeDetails.City = accountingPost_Req?.MortgageeCity;
                                    mortgageeDetails.Country = accountingPost_Req?.MortgageeCountry;
                                    mortgageeDetails.County = accountingPost_Req?.MortgageeCounty;
                                    mortgageeDetails.State = accountingPost_Req?.MortgageeState;
                                    mortgageeDetails.ZipCode = accountingPost_Req?.MortgageeZipCode;
                                    mortgageeDetails.Email = accountingPost_Req?.MortgageeEmail;
                                    mortgageeDetails.Client_Id = AuthenticatedUser.Client_Id;
                                    mortgageeDetails.CreatedBy = AuthenticatedUser.User_Id.ToString();
                                    mortgageeDetails = _policyDetailsRepository.InsertUpdateMortgageeDetails(PolicyDetailsTransaction, mortgageeDetails);
                                    policyDetails.MortgageeDetails = mortgageeDetails;
                                }

                                if (!string.IsNullOrEmpty(accountingPost_Req.ReferralUnderwriter_Broker))
                                {
                                    referralUnderwriter.Client_Id = AuthenticatedUser.Client_Id;
                                    referralUnderwriter.Name = accountingPost_Req.ReferralUnderwriter_Broker;
                                    referralUnderwriter = _policyDetailsRepository.InsertUpdateReferralUnderwriter(PolicyDetailsTransaction, referralUnderwriter);
                                    policyDetails.ReferralUnderwriter_Id = referralUnderwriter.ReferralUnderwriter_Id;
                                }

                                policyDetails.Client_Id = AuthenticatedUser.Client_Id;
                                policyDetails.Quote_Id = Convert.ToInt64(accountingPost_Req.QuoteId);
                                policyDetails.Submission_Id = Convert.ToInt64(accountingPost_Req.SubmissionId);
                                policyDetails.PreviousSubmission_Id = !string.IsNullOrEmpty(accountingPost_Req.PreviousSubmissionID) ? Convert.ToInt64(accountingPost_Req.SubmissionId) : 0;
                                policyDetails.OriginalSubmission_Id = !string.IsNullOrEmpty(accountingPost_Req.OriginalSubmissionID) ? Convert.ToInt64(accountingPost_Req.OriginalSubmissionID) : 0;
                                policyDetails.PolicyNumber = accountingPost_Req.PolicyNumber;
                                policyDetails.PolicyInstallmentCode = accountingPost_Req.PolicyInstallmentCode;
                                policyDetails.Corporation = accountingPost_Req.Corporation;
                                policyDetails.Location = accountingPost_Req.Location;
                                policyDetails.RiskState = accountingPost_Req.Risk;
                                policyDetails.PaymentPlan = accountingPost_Req.PaymentPlan;
                                policyDetails.BillType = accountingPost_Req.BillType;
                                policyDetails.EffectiveDate = Convert.ToDateTime(accountingPost_Req.EffectiveDate);
                                //policyDetails.ExpirationDate = Convert.ToDateTime(accountingPost_Req.ExpirationDate);
                                if (accountingPost_Req.TransactionType == MasterLookup._EITransactionTypes.CAN)
                                {
                                    if (DateTime.TryParse(accountingPost_Req.PolicyCancellationDate, out cancelDate))
                                    {
                                        policyDetails.ExpirationDate = cancelDate;
                                    }
                                }
                                else
                                {
                                    policyDetails.ExpirationDate = Convert.ToDateTime(accountingPost_Req.ExpirationDate);
                                }
                                // policyDetails.CancelDate = Convert.ToDateTime(accountingPost_Req.CancelDate);

                                if (DateTime.TryParse(accountingPost_Req.PolicyCancellationDate, out cancelDate))
                                {
                                    policyDetails.CancelDate = cancelDate;
                                }
                                else if (DateTime.TryParse(accountingPost_Req.CancelDate, out cancelDate))
                                {
                                    policyDetails.CancelDate = cancelDate;
                                }
                                if (!string.IsNullOrWhiteSpace(accountingPost_Req.EndorsementEffectiveDate) && DateTime.TryParse(accountingPost_Req.EndorsementEffectiveDate, out var dt))
                                {
                                    policyDetails.EndorsementEffectiveDate = dt;
                                }
                                else
                                {
                                    policyDetails.EndorsementEffectiveDate = null;
                                }
                                policyDetails.PolicyAction = accountingPost_Req.PolicyAction;
                                policyDetails.LOBOverride = accountingPost_Req.LOBOverride;
                                policyDetails.PolicyType = accountingPost_Req.PolicyType;
                                policyDetails.TransactionType = accountingPost_Req.TransactionType;
                                policyDetails.PolicyDescription = accountingPost_Req.PolicyDescription;
                                policyDetails.InvoiceNote = accountingPost_Req.InvoiceNote;
                                policyDetails.MasterPolicy = accountingPost_Req.MasterPolicy;
                                policyDetails.LoanNum = accountingPost_Req.LoanNum;
                                policyDetails.PostAsShell = accountingPost_Req.PostAsShell == "1";
                                policyDetails.CustomerType = accountingPost_Req.CustomerType;
                                policyDetails.WholesalerDetail_Id = wholesalerDetails?.WholesalerDetail_Id == 0 ? null : wholesalerDetails?.WholesalerDetail_Id;
                                policyDetails.UnderWriterDetail_Id = underWriterDetails.UnderWriterDetail_Id;
                                policyDetails.InterestDetail_Id = interestDetails?.InterestDetail_Id == 0 ? null : interestDetails?.InterestDetail_Id;
                                policyDetails.InsuredDetail_Id = insuredDetails.InsuredDetail_Id;
                                policyDetails.FinanceDetail_Id = financeDetails?.FinanceDetail_Id == 0 ? null : financeDetails?.FinanceDetail_Id;
                                policyDetails.CarrierDetail_Id = carrierDetails.CarrierDetail_Id;
                                policyDetails.AgentDetail_Id = agentDetails.AgentDetail_Id;
                                policyDetails.AgentBrokerContactDetail_Id = agentBrokerContactDetails?.AgentBrokerContactDetail_Id;
                                policyDetails.MortgageeDetail_Id = mortgageeDetails.MortgageeDetail_Id == 0 ? null : mortgageeDetails.MortgageeDetail_Id;
                                policyDetails.Source_Id = MasterLookup._Sources.ExpertInsured;
                                if (accountingPost_Req.TransactionType == MasterLookup._EITransactionTypes.END)
                                {
                                    policyDetails.EndorsementNumber = accountingPost_Req.EndorsementNumber;
                                }
                                policyDetails = _policyDetailsRepository.InsertUpdatePolicyDetails(PolicyDetailsTransaction, policyDetails);
                                policyDetails.Clients = Clients;
                                if (policyDetails.Id > 0)
                                {
                                    if (!string.IsNullOrEmpty(accountingPost_Req.PreviousSubmissionID) && !string.IsNullOrEmpty(accountingPost_Req.OriginalSubmissionID))
                                    {
                                        if (Convert.ToInt64(accountingPost_Req.PreviousSubmissionID) > 0 && Convert.ToInt64(accountingPost_Req.OriginalSubmissionID) > 0 && accountingPost_Req.TransactionType != MasterLookup._EITransactionTypes.REN)
                                        {
                                            _policyDetailsRepository.UpdatePolicyNumberByPreviousSubmissionIDWithTransaction(PolicyDetailsTransaction, Convert.ToInt64(accountingPost_Req.PreviousSubmissionID), accountingPost_Req.PolicyNumber, Clients.Client_Id, Convert.ToInt64(accountingPost_Req.OriginalSubmissionID));
                                        }

                                    }
                                    LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(Clients.Client_Id, BillingType_Id);
                                    if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                                    {
                                        _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                                    }
                                    else
                                    {
                                        _AccountingType_Id = Clients.AccountingType_Id;
                                    }
                                    _policyDetailsRepository.InactivePolicyDetailsLineItems(PolicyDetailsTransaction, policyDetails.Id, Clients.Client_Id);
                                    if (accountingPost_Req.PolicyDetailTransfer?.Count > 0)
                                    {
                                        foreach (var item in accountingPost_Req.PolicyDetailTransfer)
                                        {
                                            if (!string.IsNullOrEmpty(item.SubmissionId))
                                            {
                                                if (!string.IsNullOrEmpty(item.SubmissionId))
                                                {
                                                    var carrierPremiumItem = accountingPost_Req.PolicyDetailTransfer.FirstOrDefault(x => string.Equals(x.LineType?.Trim(), "PREMIUM", StringComparison.OrdinalIgnoreCase) && !string.Equals(x.PolicyDetailDescription?.Trim(), MasterLookup._CustomLineitems.FullyEarnedPremium?.Trim(), StringComparison.OrdinalIgnoreCase));
                                                    bool isFullyEarnedPremium = string.Equals(item.LineType?.Trim(), "FEE", StringComparison.OrdinalIgnoreCase) && string.Equals(item.PolicyDetailDescription?.Trim(), MasterLookup._CustomLineitems.FullyEarnedPremium?.Trim(), StringComparison.OrdinalIgnoreCase);
                                                    double feeTaxAmount = string.IsNullOrEmpty(item.FeeTaxAmount) ? 0.0 : Convert.ToDouble(item.FeeTaxAmount);
                                                    double agentCommissionPercentage = carrierPremiumItem != null && !string.IsNullOrEmpty(carrierPremiumItem.AgentCommission) ? Convert.ToDouble(carrierPremiumItem.AgentCommission) : 0.0;
                                                    double houseCommissionPercentage = carrierPremiumItem != null && !string.IsNullOrEmpty(carrierPremiumItem.HouseCommission) ? Convert.ToDouble(carrierPremiumItem.HouseCommission) : 0.0;

                                                    if (isFullyEarnedPremium)
                                                    {
                                                        item.LineType = "PREMIUM";
                                                        item.Premium = feeTaxAmount.ToString();
                                                        item.FeeTaxAmount = "0.00";
                                                        item.AgentCommission = agentCommissionPercentage.ToString("0.00");
                                                        item.BrokerCommissionAmount = Math.Round(feeTaxAmount * agentCommissionPercentage / 100, 2).ToString("0.00");
                                                        item.HouseCommission = houseCommissionPercentage.ToString("0.00");
                                                        item.CommissionAmount = Math.Round(feeTaxAmount * houseCommissionPercentage / 100, 2).ToString("0.00");
                                                    }
                                                    PolicyDetailsLineItems policyDetailsLineItems = new PolicyDetailsLineItems();
                                                    policyDetailsLineItems.PolicyDetails_Id = policyDetails.Id;
                                                    policyDetailsLineItems.Submission_Id = Convert.ToInt64(item.SubmissionId);
                                                    policyDetailsLineItems.LineType = item.LineType;
                                                    policyDetailsLineItems.HouseCommission = string.IsNullOrEmpty(item.HouseCommission) ? 0.0 : Convert.ToDouble(item.HouseCommission);
                                                    policyDetailsLineItems.AgentCommission = string.IsNullOrEmpty(item.AgentCommission) ? 0.0 : Convert.ToDouble(item.AgentCommission);
                                                    policyDetailsLineItems.BrokerCommissionAmount = string.IsNullOrEmpty(item.BrokerCommissionAmount) ? 0 : Convert.ToDouble(item.BrokerCommissionAmount);
                                                    policyDetailsLineItems.CommissionAmount = string.IsNullOrEmpty(item.CommissionAmount) ? 0 : Convert.ToDouble(item.CommissionAmount);
                                                    policyDetailsLineItems.Coverage = item.Coverage;
                                                    policyDetailsLineItems.CustomerType = item.CustomerType;
                                                    policyDetailsLineItems.PolicyDetailDescription = item.PolicyDetailDescription;
                                                    policyDetailsLineItems.FeeTaxAmount = string.IsNullOrEmpty(item.FeeTaxAmount) ? 0.0 : Convert.ToDouble(item.FeeTaxAmount);
                                                    policyDetailsLineItems.Premium = string.IsNullOrEmpty(item.Premium) ? 0.0 : Convert.ToDouble(item.Premium);
                                                    policyDetailsLineItems.FeeTaxable = item.FeeTaxable == 1;
                                                    policyDetailsLineItems.EffectiveDate = Convert.ToDateTime(item.EffectiveDate);
                                                    policyDetailsLineItems.State = item.State;
                                                    policyDetailsLineItems.Is_Commissionable = item.Is_Commissionable == "1";
                                                    policyDetailsLineItems.MinPremium = string.IsNullOrEmpty(item.MinPremium) ? 0.0 : Convert.ToDouble(item.MinPremium);
                                                    policyDetailsLineItems.Client_Id = Clients.Client_Id;

                                                    _policyDetailsRepository.InsertUpdatePolicyDetailsLineItems(PolicyDetailsTransaction, policyDetailsLineItems);
                                                    policyDetails.PolicyDetailsLineItemsList.Add(policyDetailsLineItems);
                                                }
                                                else
                                                {
                                                    serviceResponse.Status = false;
                                                    serviceResponse.Message = "SubmissionId not found.";
                                                    return serviceResponse;
                                                }

                                            }
                                        }
                                        bool isNonMonetary = IsNonMonetary(policyDetails.PolicyDetailsLineItemsList);
                                        var TotalAmount = GetTotalPolicyAmounts(policyDetails, policyDetails.PolicyDetailsLineItemsList);
                                        var CommissionServiceResponse = GetCommissionsServiceResponse(policyDetails, policyDetails.PolicyDetailsLineItemsList);
                                        if (isNonMonetary)
                                        {
                                            if (PolicyDetailsTransaction.Connection != null)
                                            {
                                                PolicyDetailsTransaction.Rollback();
                                            }
                                            if (!string.IsNullOrEmpty(accountingPost_Req.PreviousSubmissionID) && !string.IsNullOrEmpty(accountingPost_Req.OriginalSubmissionID))
                                            {
                                                if (Convert.ToInt64(accountingPost_Req.PreviousSubmissionID) > 0 && Convert.ToInt64(accountingPost_Req.OriginalSubmissionID) > 0)
                                                {
                                                    _policyDetailsRepository.UpdatePolicyNumberByPreviousSubmissionID(Convert.ToInt64(accountingPost_Req.PreviousSubmissionID), accountingPost_Req.PolicyNumber, Clients.Client_Id, Convert.ToInt64(accountingPost_Req.OriginalSubmissionID));
                                                }

                                            }

                                            serviceResponse.Status = true; //continue
                                            serviceResponse.Message = "Non-monetary transaction";
                                            return serviceResponse;
                                        }
                                        if (CommissionServiceResponse != null)
                                        {
                                            if (!string.IsNullOrEmpty(CommissionServiceResponse.Message))
                                            {
                                                if (PolicyDetailsTransaction.Connection != null)
                                                {
                                                    PolicyDetailsTransaction.Rollback();
                                                }
                                                serviceResponse.Status = CommissionServiceResponse.Status;
                                                serviceResponse.Message = CommissionServiceResponse.Message;
                                                return serviceResponse;
                                            }
                                        }
                                        var PaymentPlan_Id = LookupUtility.GetAllPaymentPlans().FirstOrDefault(o => o.Code == accountingPost_Req.PaymentPlan)?.PaymentPlan_Id;

                                        List<PolicyPaymentSchedule> insertedInvoices = new List<PolicyPaymentSchedule>();

                                        List<PolicyDetailsLineItems> PolicyDetailsLineItemsList = policyDetails.PolicyDetailsLineItemsList;

                                        //if (!string.IsNullOrEmpty(accountingPost_Req.PaymentPlan)/* || PaymentPlan_Id == MasterLookup._PaymentPlans.AnnualFullPay*/)
                                        //{
                                        double totalInvoiceAmount = 0;
                                        if (PolicyDetailsLineItemsList?.Count > 0)
                                        {
                                            PolicyDetailsLineItemsList = PolicyDetailsLineItemsList.Where(x => !((x.LineType == "FEE" || x.LineType == "TAX" ) && Convert.ToDecimal(x.FeeTaxAmount) == 0)).ToList();
                                            //turn off the validation for CannGen Client for time being just to allow customer to check the flow end-to-end in UAT [2026-March-04] 
                                            //if (AuthenticatedUser.Client_Id != 10011)
                                            //{
                                            List<LU_LineItems_Clients> LineItemsClientsList = _lookupRepository.GetLineItems_ClientsByBillingType_Id(AuthenticatedUser.Client_Id, BillingType_Id);
                                            if (LineItemsClientsList == null)
                                            {
                                                if (PolicyDetailsTransaction.Connection != null)
                                                {
                                                    PolicyDetailsTransaction.Rollback();
                                                }
                                                serviceResponse.Status = false;
                                                serviceResponse.Message = $"The specified bill type '{policyDetails.BillType}' has no line item configuration. Please contact the administrator.";
                                                return serviceResponse;
                                            }
                                            List<string> missingDescriptions = new List<string>();
                                            bool AreAllDescriptionsInLineTypes(
                                                    List<LU_LineItems_Clients> firstList,
                                                    List<PolicyDetailsLineItems> secondList)
                                            {
                                                var lineTypeNames = new HashSet<string>(firstList.Select(x => x.LineItemName.ToLower().Trim()));
                                                var distinctDescriptions = secondList.Where(z => z.LineType.ToLower().Trim() == "fee" || z.LineType.ToLower().Trim() == "tax")
                                                    .Select(x => x.PolicyDetailDescription.ToLower().Trim())
                                                    .Distinct();
                                                missingDescriptions = distinctDescriptions
                                                    .Where(desc => !lineTypeNames.Contains(desc))
                                                    .ToList();
                                                return distinctDescriptions.All(desc => lineTypeNames.Contains(desc));
                                            }
                                            if (!AreAllDescriptionsInLineTypes(LineItemsClientsList, PolicyDetailsLineItemsList))
                                            {
                                                if (PolicyDetailsTransaction.Connection != null)
                                                {
                                                    PolicyDetailsTransaction.Rollback();
                                                }
                                                serviceResponse.Status = false;
                                                serviceResponse.Message = $"The Line Items: {string.Join(", ", missingDescriptions)} are not configured in Accounting. Please contact system administrator.";
                                                return serviceResponse;
                                            }
                                            //}
                                            if (BillingType_Id > 0)
                                            {
                                                var PolicyInvoices = _invoiceRepository.GetAllInvoicesByPolicyDetail_Id(policyDetails.Id);

                                                if (PolicyInvoices != null && PolicyInvoices.Count != 0)
                                                {
                                                    foreach (var invoice in PolicyInvoices)
                                                    {
                                                        PolicyDetails PolicyDetailsJournalEntry = policyDetails;
                                                        PolicyDetailsJournalEntry.Invoices = invoice;
                                                        JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                                        if (Clients.AccountingService_Id == MasterLookup._Services.Intuit && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                        {
                                                            QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                            if (journalEntries != null)
                                                            {
                                                                deleteJournalResponse = qBOUtility.DeleteJournalEntry(journalEntries);
                                                                if (deleteJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(deleteJournalResponse?.JournalEntry?.Id))
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Intuit!" + deleteJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                    return serviceResponse;
                                                                }
                                                                else if (deleteJournalResponse?.JournalEntry?.Id != null && !string.IsNullOrEmpty(deleteJournalResponse?.JournalEntry?.Id))
                                                                {

                                                                    _journalEntryLogRepository.DeleteJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                                                }
                                                            }

                                                        }
                                                        else if (Clients.AccountingService_Id == MasterLookup._Services.Sage && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                        {
                                                            if (journalEntries != null)
                                                            {
                                                                SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                                var deleteJournalRes = Task.Run(() => sageUtility.DeleteJournalEntry_Intacct(journalEntries)).Result;
                                                                var serializer = new XmlSerializer(typeof(SageResponse));
                                                                SageResponse createSageJournalResponse = new SageResponse();
                                                                if (!string.IsNullOrEmpty(deleteJournalRes))
                                                                {
                                                                    using (var reader = new StringReader(deleteJournalRes))
                                                                    {
                                                                        createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                    }
                                                                    if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                                    {
                                                                        if (PolicyDetailsTransaction.Connection != null)
                                                                        {
                                                                            PolicyDetailsTransaction.Rollback();
                                                                        }
                                                                        serviceResponse.Status = false;
                                                                        serviceResponse.Message = "Cannot sync invoice with Sage!";
                                                                        return serviceResponse;
                                                                    }

                                                                    var xmlDoc = XDocument.Parse(deleteJournalRes);

                                                                    var status = xmlDoc.Descendants("status")
                                                                                       .FirstOrDefault(x => x.Parent?.Name == "result")
                                                                                       ?.Value;

                                                                    if (status == "failure")
                                                                    {
                                                                        var errorMsg = xmlDoc.Descendants("description2")
                                                                                             .FirstOrDefault()?.Value;

                                                                        if (PolicyDetailsTransaction?.Connection != null)
                                                                            PolicyDetailsTransaction.Rollback();

                                                                        serviceResponse.Status = false;
                                                                        serviceResponse.Message = "Cannot sync invoice with Sage! ";

                                                                        //return serviceResponse;
                                                                    }
                                                                    else if (status == "success")
                                                                    {
                                                                        _journalEntryLogRepository.DeleteJournalEntry(
                                                                            MasterLookup._Sources.Invoice,
                                                                            invoice.Id);

                                                                        serviceResponse.Status = true;
                                                                        serviceResponse.Message = "Journal deleted successfully in Sage.";

                                                                        //return serviceResponse;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (PolicyDetailsTransaction?.Connection != null)
                                                                            PolicyDetailsTransaction.Rollback();

                                                                        serviceResponse.Status = false;
                                                                        serviceResponse.Message = "Unexpected response from Sage.";

                                                                        //return serviceResponse;
                                                                    }
                                                                }

                                                            }

                                                        }
                                                        else
                                                        {
                                                            if (Clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                                            {
                                                                if (journalEntries != null)
                                                                {
                                                                    QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                                    DeleteJournalResponse deleteExistingJournalResponse = new DeleteJournalResponse();
                                                                    deleteExistingJournalResponse = qBOUtility.DeleteJournalEntry(journalEntries);
                                                                    if (deleteExistingJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(deleteExistingJournalResponse?.JournalEntry?.Id))
                                                                    {
                                                                        if (PolicyDetailsTransaction.Connection != null)
                                                                        {
                                                                            PolicyDetailsTransaction.Rollback();
                                                                        }
                                                                        serviceResponse.Status = false;
                                                                        serviceResponse.Message = "Cannot sync invoice with Intuit!" + deleteExistingJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                        return serviceResponse;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (journalEntries != null)
                                                                {
                                                                    DeleteJournalResponse deleteExistingJournalResponse = new();
                                                                    SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                                    var deleteExistingJournalRes = Task.Run(() => sageUtility.DeleteJournalEntry_Intacct(journalEntries)).Result;

                                                                    var serializer = new XmlSerializer(typeof(SageResponse));
                                                                    SageResponse deleteSageJournalResponse = new SageResponse();
                                                                    using (var reader = new StringReader(deleteExistingJournalRes))
                                                                    {
                                                                        deleteSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                    }
                                                                    if (deleteSageJournalResponse == null || deleteSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                                    {
                                                                        if (PolicyDetailsTransaction.Connection != null)
                                                                        {
                                                                            PolicyDetailsTransaction.Rollback();
                                                                        }
                                                                        serviceResponse.Status = false;
                                                                        serviceResponse.Message = "Cannot sync invoice with Sage!";
                                                                        return serviceResponse;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        _invoiceRepository.InactiveInvoiceDetails(PolicyDetailsTransaction, invoice.Id, Clients.Client_Id);
                                                    }
                                                    // Mark all invoices as inactive for this policy
                                                    _invoiceRepository.InActiveInvoices(PolicyDetailsTransaction, policyDetails.Id, Clients.Client_Id);

                                                }
                                                if (!string.IsNullOrEmpty(accountingPost_Req.PaymentPlan) && string.IsNullOrEmpty(LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.IsSingleInvoiceCreated)))
                                                {
                                                    List<PolicyPaymentSchedule> scheduledInvoices = [];

                                                    Invoices invoiceDetail = _invoiceRepository.GetInvoices(policyDetails.Id) ?? new Invoices();
                                                    invoiceDetail.CreatedOn = DateTime.Now;
                                                    double policyAgtCommPercentage = 0;
                                                    double totalFeeTaxAmount = 0;
                                                    DateTime endorsementDate;
                                                    var EndorsementDate = policyDetails.TransactionType == MasterLookup._EITransactionTypes.END && DateTime.TryParse(accountingPost_Req.EndorsementEffectiveDate, out endorsementDate) ? endorsementDate : policyDetails.EffectiveDate;
                                                    scheduledInvoices = ScheduledInvoices(policyDetails, accountingPost_Req, invoiceDetail, BillingType_Id, AuthenticatedUser.Client_Id, EndorsementDate, policyDetails.TransactionType, policyDetails.OriginalSubmission_Id, policyDetails.PreviousSubmission_Id, policyDetails.PolicyNumber, out policyAgtCommPercentage, out totalFeeTaxAmount);
                                                    insertedInvoices = _installmentPlansRepository.InsertUpdateGeneratedInvoices(PolicyDetailsTransaction, scheduledInvoices, Clients.Client_Id);
                                                    NoOfInstallments = scheduledInvoices?.Count.ToString();
                                                    policyDetails.InvoicesList = new List<Invoices>();
                                                    foreach (var inv in scheduledInvoices)
                                                    {
                                                        Invoices invoices = _invoiceRepository.GetInvoices(policyDetails.Id, Convert.ToInt32(inv.InstallmentNo)) ?? new Invoices();
                                                        invoices.PolicyDetails_Id = policyDetails.Id;
                                                        invoices.InvoiceNo = string.IsNullOrEmpty(invoices.InvoiceNo) ? null : invoices.InvoiceNo;
                                                        invoices.InvoiceNote = accountingPost_Req.InvoiceNote;
                                                        double agentCommissionAmt = Math.Round(inv.Amount * (policyAgtCommPercentage / (100.0 - policyAgtCommPercentage)), 2);
                                                        if (inv.IsDownPayment)
                                                        {
                                                            inv.Amount = inv.Amount + totalFeeTaxAmount;
                                                        }
                                                        totalInvoiceAmount = totalInvoiceAmount + inv.Amount;
                                                        invoices.RemainingBalance = inv.Amount;
                                                        invoices.Amount = inv.Amount;
                                                        invoices.InvoiceEffectiveDate = policyDetails.EffectiveDate;
                                                        invoices.InstallmentNo = Convert.ToInt32(inv.InstallmentNo);
                                                        invoices.Schedule_Id = Convert.ToInt32(inv.PolicyPaymentSchedule_Id);
                                                        invoices.DueDate = inv.DueDate;
                                                        invoices.Client_Id = Clients.Client_Id;
                                                        //invoices.BillDate = inv.BillDate;
                                                        invoices = _invoiceRepository.InsertInstallmentInvoices(PolicyDetailsTransaction, invoices);
                                                        invoicesList.Add(invoices.Id);
                                                        if (inv.IsDownPayment || (policyDetails.TransactionType == MasterLookup._EITransactionTypes.END && inv.Sequence == 1))
                                                        {
                                                            invoices = _invoiceRepository.GetInvoices(policyDetails.Id, Convert.ToInt32(inv.InstallmentNo)) ?? new Invoices();
                                                            Invoice_Id = invoices.Id;
                                                            InvoiceNumber = invoices.InvoiceNo;
                                                            InvoiceDate = invoices?.BillDate.ToShortDateString();
                                                            PaymentDueOn = inv?.DueDate.ToShortDateString();
                                                        }
                                                        policyDetails.Invoices = invoices;
                                                        var PaymentApplicationsInvoiceList = _paymentsRepository.GetAllPaymentApplicationLineItemsByInvoice_Id(invoices.Id);
                                                        if (PaymentApplicationsInvoiceList?.Count() > 0)
                                                        {
                                                            if (PolicyDetailsTransaction.Connection != null)
                                                            {
                                                                PolicyDetailsTransaction.Rollback();
                                                            }
                                                            serviceResponse.Status = false;
                                                            serviceResponse.Message = $"Payment applications already exist for invoice '{invoices.InvoiceNo}' (Policy Number: '{policyDetails.PolicyNumber}').";
                                                            return serviceResponse;
                                                        }
                                                        _invoiceRepository.InactiveInvoiceDetails(PolicyDetailsTransaction, invoices.Id, Clients.Client_Id);

                                                        if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                                                        {
                                                            // invoices.Amount = inv.Amount + agentCommissionAmt;
                                                        }
                                                        bool isPackage = false;
                                                        //invoices.CreatedOn = inv.BillDate;
                                                        if (PolicyDetailsLineItemsList?.Where(x => x.Coverage == MasterLookup._CustomLOBs.Common)?.Count() > 0)
                                                        {
                                                            isPackage = true;
                                                            PolicyDetailsLineItemsList = PolicyDetailsLineItemsList?.Where(x => x.Coverage == MasterLookup._CustomLOBs.Common)?.ToList();
                                                        }
                                                        var GroupedByCoverage = PolicyDetailsLineItemsList.GroupBy(o => o.Coverage);
                                                        var commissionRules = _commissionRulesRepository.GetCommissionRules(MasterLookup._EntityTypes.Underwriter, policyDetails.UnderWriterDetail_Id, AuthenticatedUser.Client_Id);
                                                        if (GroupedByCoverage?.Count() > 0)
                                                        {
                                                            foreach (var item in GroupedByCoverage)
                                                            {
                                                                byte LOB_Id = !isPackage ? LookupUtility.GetAllLOBs().FirstOrDefault(x => x.Name?.Trim() == item.Key?.Trim())?.LOB_Id ?? 0 : LookupUtility.GetAllLOBs().FirstOrDefault(x => x.Name == MasterLookup._CustomLOBs.Package)?.LOB_Id ?? 0;
                                                                policyDetails.LOB_Id = LOB_Id;
                                                                if (LOB_Id == 0)
                                                                {
                                                                    if (PolicyDetailsTransaction?.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = $"The LOB {item.Key} is not Configured in Accounting. Please configure and retry!";
                                                                    return serviceResponse;
                                                                }
                                                                var PolicyDetailsLineItemsCoverageList = PolicyDetailsLineItemsList.Where(x => x.Coverage?.Trim() == item.Key?.Trim()).ToList();
                                                                if (PolicyDetailsLineItemsCoverageList?.Count() > 0)
                                                                {
                                                                    List<InvoiceDetails> invoiceDetailsList = GetInvoiceDetailsWithInstallmentPlan(BillingType_Id, invoices, policyDetails, PolicyDetailsLineItemsCoverageList, LOB_Id, policyAgtCommPercentage, commissionRules);
                                                                    if (invoiceDetailsList.Count() > 0)
                                                                    {
                                                                        foreach (var invoice in invoiceDetailsList)
                                                                        {
                                                                            if (invoice.EntityType_Id > 0 && (invoice.Entity_Id > 0 || (invoice.EntityType_Id == MasterLookup._EntityTypes.State && invoice.Entity_Id == 0)) && invoice.Amount != 0)
                                                                            {
                                                                                invoice.Client_Id = Clients.Client_Id;
                                                                                _invoiceRepository.InsertUpdateInvoiceDetails(PolicyDetailsTransaction, invoice);
                                                                                policyDetails.Invoices.InvoicesDetailsList.Add(invoice);
                                                                            }
                                                                        }
                                                                        policyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(policyDetails.Invoices.InvoicesDetailsList, policyDetails, true);
                                                                        policyDetails.InvoicesList.Add(policyDetails.Invoices);
                                                                    }


                                                                }
                                                            }
                                                        }
                                                        if (Clients.AccountingService_Id == MasterLookup._Services.Intuit && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                        {
                                                            PolicyDetails PolicyDetailsJournalEntry = policyDetails;
                                                            PolicyDetailsJournalEntry.Invoices = invoices;
                                                            JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoices.Id);
                                                            QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                            if (journalEntries != null)
                                                            {
                                                                updateInvoiceJournalResponse = qBOUtility.UpdateInvoiceJournalEntry(PolicyDetailsTransaction, PolicyDetailsJournalEntry, journalEntries);
                                                                if (updateInvoiceJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(updateInvoiceJournalResponse?.JournalEntry?.Id))
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Intuit!" + updateInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                    return serviceResponse;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                createInvoiceJournalResponse = qBOUtility.CreateInvoiceJournalEntry(PolicyDetailsTransaction, PolicyDetailsJournalEntry);
                                                                if (createInvoiceJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(createInvoiceJournalResponse?.JournalEntry?.Id))
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Intuit!" + createInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                    return serviceResponse;
                                                                }
                                                            }

                                                        }
                                                        else if (Clients.AccountingService_Id == MasterLookup._Services.Sage && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                        {
                                                            SageResponse createSageJournalResponse = new SageResponse();
                                                            PolicyDetails PolicyDetailsJournalEntry = policyDetails;
                                                            PolicyDetailsJournalEntry.Invoices = invoices;
                                                            JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoices.Id);
                                                            SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                            if (journalEntries != null)
                                                            {
                                                                var updateInvoiceJournalResp = Task.Run(() => sageUtility.UpdateInvoiceJournalEntry_Intacct(PolicyDetailsTransaction, PolicyDetailsJournalEntry, journalEntries)).Result;

                                                                var serializer = new XmlSerializer(typeof(SageResponse));
                                                                using (var reader = new StringReader(updateInvoiceJournalResp))
                                                                {
                                                                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                }
                                                                if (createSageJournalResponse == null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Sage!" + createSageJournalResponse?.Operation.Result?.Errormessage?.Error[0]?.Description2;
                                                                    return serviceResponse;
                                                                }

                                                            }
                                                            else
                                                            {
                                                                var createJournalRes = Task.Run(() =>
                                                                sageUtility.CreateInvoiceJournalEntry_Intacct(PolicyDetailsTransaction, PolicyDetailsJournalEntry)).Result;
                                                                var xmlDoc = XDocument.Parse(createJournalRes);
                                                                var description2 = xmlDoc.Descendants("description2").FirstOrDefault()?.Value;

                                                                var serializer = new XmlSerializer(typeof(SageResponse));
                                                                using (var reader = new StringReader(createJournalRes))
                                                                {
                                                                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                }
                                                                if (createSageJournalResponse == null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Intuit!" + description2;
                                                                    return serviceResponse;
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    List<InvoiceDetails> allInvoiceDetails = new List<InvoiceDetails>();


                                                    bool isPackage = false;
                                                    if (PolicyDetailsLineItemsList?.Where(x => x.Coverage == MasterLookup._CustomLOBs.Common)?.Count() > 0)
                                                    {
                                                        isPackage = true;
                                                        PolicyDetailsLineItemsList = PolicyDetailsLineItemsList?.Where(x => x.Coverage == MasterLookup._CustomLOBs.Common)?.ToList();
                                                    }
                                                    var GroupedByCoverage = PolicyDetailsLineItemsList.GroupBy(o => o.Coverage);
                                                    if (GroupedByCoverage?.Count() > 0)
                                                    {
                                                        var commissionRules = _commissionRulesRepository.GetCommissionRules(MasterLookup._EntityTypes.Underwriter, policyDetails.UnderWriterDetail_Id, AuthenticatedUser.Client_Id);
                                                        foreach (var item in GroupedByCoverage)
                                                        {
                                                            byte LOB_Id = !isPackage ? LookupUtility.GetAllLOBs().FirstOrDefault(x => x.Name?.Trim() == item.Key?.Trim())?.LOB_Id ?? 0 : LookupUtility.GetAllLOBs().FirstOrDefault(x => x.Name == MasterLookup._CustomLOBs.Package)?.LOB_Id ?? 0;
                                                            policyDetails.LOB_Id = LOB_Id;
                                                            if (LOB_Id == 0)
                                                            {
                                                                if (PolicyDetailsTransaction?.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }
                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = $"The LOB {item.Key} is not Configured in Accounting. Please configure and retry!";
                                                                return serviceResponse;
                                                            }
                                                            var PolicyDetailsLineItemsCoverageList = PolicyDetailsLineItemsList.Where(x => x.Coverage?.Trim() == item.Key?.Trim()).ToList();
                                                            if (PolicyDetailsLineItemsCoverageList?.Count() > 0)
                                                            {
                                                                List<InvoiceDetails> invoiceDetailsList = GetInvoiceDetails(BillingType_Id, policyDetails, PolicyDetailsLineItemsCoverageList, LOB_Id, commissionRules);
                                                                if (invoiceDetailsList.Count() > 0)
                                                                {
                                                                    allInvoiceDetails.AddRange(invoiceDetailsList);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    Invoices invoices = _invoiceRepository.GetInvoices(policyDetails.Id) ?? new Invoices();
                                                    invoices.PolicyDetails_Id = policyDetails.Id;
                                                    invoices.InvoiceNo = string.IsNullOrEmpty(invoices.InvoiceNo) ? null : invoices.InvoiceNo;
                                                    var settingKeyMap = new Dictionary<byte, string>
                                                    {
                                                        { MasterLookup._BillingTypes.Agency, MasterLookup._ClientSettings.BillingDueDateRuleAgencyBill },
                                                        { MasterLookup._BillingTypes.Insured, MasterLookup._ClientSettings.BillingDueDateRuleInsuredBill },
                                                        { MasterLookup._BillingTypes.Direct,  MasterLookup._ClientSettings.BillingDueDateRuleDirectBill }
                                                    };

                                                    if (settingKeyMap.TryGetValue(BillingType_Id, out string settingKey))
                                                    {
                                                        string ruleValue = LookupUtility.GetClientSettings(Clients.Client_Id, settingKey);

                                                        if (!string.IsNullOrEmpty(ruleValue))
                                                        {
                                                            invoices.Is_ClientSpecificDueDate = true;
                                                            invoices.ClientSpecificDueDateRule = ruleValue;
                                                        }
                                                    }
                                                    //invoices.DueDate = invoices.DueDate == default ? DateTime.Today.AddDays(30) : invoices.DueDate;
                                                    invoices.InvoiceNote = accountingPost_Req.InvoiceNote;
                                                    double policyPremium;
                                                    double policyAgtCommPercentage;
                                                    invoices.Amount = GetInvoicedAmount(allInvoiceDetails);
                                                    invoices.RemainingBalance = invoices.Amount;
                                                    invoices.InstallmentNo = 1;
                                                    invoices.InvoiceEffectiveDate = policyDetails.TransactionType == MasterLookup._EITransactionTypes.NEW ? policyDetails.EffectiveDate : null;
                                                    invoices.ClientSpecificInvoiceEffectiveDate = policyDetails.TransactionType == MasterLookup._EITransactionTypes.NEW ? policyDetails.EffectiveDate : policyDetails.PolicyDetailsLineItemsList?.FirstOrDefault()?.EffectiveDate ?? policyDetails.EffectiveDate;
                                                    invoices.Client_Id = AuthenticatedUser.Client_Id;
                                                    invoices = _invoiceRepository.InsertUpdateInvoices(PolicyDetailsTransaction, invoices);
                                                    Invoice_Id = invoices.Id;
                                                    InvoiceNumber = invoices.InvoiceNo;
                                                    NoOfInstallments = "1";
                                                    invoices = _invoiceRepository.GetInvoices(policyDetails.Id) ?? new Invoices();
                                                    InvoiceDate = invoices?.BillDate.ToShortDateString();
                                                    PaymentDueOn = invoices?.DueDate.ToShortDateString();
                                                    policyDetails.Invoices = invoices;
                                                    var PaymentApplicationsInvoiceList = _paymentsRepository.GetAllPaymentApplicationLineItemsByInvoice_Id(invoices.Id);
                                                    if (PaymentApplicationsInvoiceList?.Count() > 0)
                                                    {
                                                        if (PolicyDetailsTransaction.Connection != null)
                                                        {
                                                            PolicyDetailsTransaction.Rollback();
                                                        }
                                                        serviceResponse.Status = false;
                                                        serviceResponse.Message = $"Payment applications already exist for invoice '{invoices.InvoiceNo}' (Policy Number: '{policyDetails.PolicyNumber}').";
                                                        return serviceResponse;
                                                    }
                                                    _invoiceRepository.InactiveInvoiceDetails(PolicyDetailsTransaction, invoices.Id, Clients.Client_Id);
                                                    if (allInvoiceDetails?.Count() > 0)
                                                    {
                                                        foreach (var invoice in allInvoiceDetails)
                                                        {
                                                            if (invoice.EntityType_Id > 0 && (invoice.Entity_Id > 0 || (invoice.EntityType_Id == MasterLookup._EntityTypes.State && invoice.Entity_Id == 0)) && invoice.Amount != 0)
                                                            {
                                                                invoice.Invoice_Id = invoices.Id;
                                                                invoice.Client_Id = Clients.Client_Id;
                                                                _invoiceRepository.InsertUpdateInvoiceDetails(PolicyDetailsTransaction, invoice);
                                                                policyDetails.Invoices.InvoicesDetailsList.Add(invoice);
                                                            }
                                                        }
                                                    }
                                                    if (Clients.AccountingService_Id == MasterLookup._Services.Intuit && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                    {
                                                        PolicyDetails PolicyDetailsJournalEntry = policyDetails;
                                                        PolicyDetailsJournalEntry.Invoices = invoices;
                                                        JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoices.Id);
                                                        QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                        if (journalEntries != null)
                                                        {
                                                            updateInvoiceJournalResponse = qBOUtility.UpdateInvoiceJournalEntry(PolicyDetailsTransaction, PolicyDetailsJournalEntry, journalEntries);
                                                            if (updateInvoiceJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(updateInvoiceJournalResponse?.JournalEntry?.Id))
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }
                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Intuit!" + createInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                return serviceResponse;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            createInvoiceJournalResponse = qBOUtility.CreateInvoiceJournalEntry(PolicyDetailsTransaction, PolicyDetailsJournalEntry);
                                                            if (createInvoiceJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(createInvoiceJournalResponse?.JournalEntry?.Id))
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }
                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Intuit!" + createInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                return serviceResponse;
                                                            }
                                                        }

                                                    }
                                                    else if (Clients.AccountingService_Id == MasterLookup._Services.Sage && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                                    {
                                                        SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                        SageResponse createSageJournalResponse = new SageResponse();
                                                        PolicyDetails PolicyDetailsJournalEntry = policyDetails;
                                                        PolicyDetailsJournalEntry.Invoices = invoices;
                                                        JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoices.Id);
                                                        if (journalEntries != null)
                                                        {
                                                            var updateInvoiceJournalResp = Task.Run(() => sageUtility.UpdateInvoiceJournalEntry_Intacct(PolicyDetailsTransaction, PolicyDetailsJournalEntry, journalEntries)).Result;

                                                            var serializer = new XmlSerializer(typeof(SageResponse));
                                                            using (var reader = new StringReader(updateInvoiceJournalResp))
                                                            {
                                                                createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                            }
                                                            if (createSageJournalResponse == null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }
                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Intuit!" + createInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                return serviceResponse;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            string createJournalRes = null;

                                                            try
                                                            {
                                                                createJournalRes = Task.Run(() => sageUtility.CreateInvoiceJournalEntry_Intacct(PolicyDetailsTransaction, PolicyDetailsJournalEntry)).Result;
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Cannot sync invoice with Sage!";

                                                                return serviceResponse;
                                                            }

                                                            if (string.IsNullOrWhiteSpace(createJournalRes))
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Empty response from Sage.";

                                                                return serviceResponse;
                                                            }

                                                            try
                                                            {
                                                                var serializer = new XmlSerializer(typeof(SageResponse));

                                                                using (var reader = new StringReader(createJournalRes))
                                                                {
                                                                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;
                                                                serviceResponse.Message = "Invalid XML response from Sage: " + ex.Message;

                                                                return serviceResponse;
                                                            }

                                                            var recordNo = createSageJournalResponse?.Operation?.Result?.Data?.GlBatch?.RecordNo;

                                                            if (createSageJournalResponse == null || recordNo == null || recordNo == 0)
                                                            {
                                                                if (PolicyDetailsTransaction.Connection != null)
                                                                {
                                                                    PolicyDetailsTransaction.Rollback();
                                                                }

                                                                serviceResponse.Status = false;

                                                                var errorMessage = createSageJournalResponse?.Operation?.Result?.Errormessage?.Error?
                                                                    .FirstOrDefault();

                                                                serviceResponse.Message =
                                                                    "Cannot sync invoice with Sage! " +
                                                                    (errorMessage?.Description2 ?? errorMessage?.Description?.ToString() ?? "Unknown error");

                                                                return serviceResponse;
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoices.Id);
                                                        if (_AccountingType_Id == MasterLookup._Services.Intuit)
                                                        {
                                                            if (journalEntries != null)
                                                            {
                                                                QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                                DeleteJournalResponse deleteExistingJournalResponse = new DeleteJournalResponse();
                                                                deleteExistingJournalResponse = qBOUtility.DeleteJournalEntry(journalEntries);
                                                                if (deleteExistingJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(deleteExistingJournalResponse?.JournalEntry?.Id))
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Intuit!" + deleteExistingJournalResponse?.Fault?.Error?[0]?.Detail;
                                                                    return serviceResponse;
                                                                }
                                                            }
                                                        }
                                                        else if (_AccountingType_Id == MasterLookup._Services.Sage)
                                                        {
                                                            SageUtility sageUtility = new SageUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                                            if (journalEntries != null)
                                                            {
                                                                var deleteJournalRes = Task.Run(() => sageUtility.DeleteJournalEntry_Intacct(journalEntries)).Result;
                                                                var serializer = new XmlSerializer(typeof(SageResponse));
                                                                SageResponse createSageJournalResponse = new SageResponse();
                                                                using (var reader = new StringReader(deleteJournalRes))
                                                                {
                                                                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                }
                                                                if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                                {
                                                                    if (PolicyDetailsTransaction.Connection != null)
                                                                    {
                                                                        PolicyDetailsTransaction.Rollback();
                                                                    }
                                                                    serviceResponse.Status = false;
                                                                    serviceResponse.Message = "Cannot sync invoice with Sage!" + createSageJournalResponse?.Operation?.Result?.Errormessage?.Error[0]?.Description2;
                                                                    return serviceResponse;
                                                                }
                                                            }


                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (PolicyDetailsTransaction.Connection != null)
                                                {
                                                    PolicyDetailsTransaction.Rollback();
                                                }
                                                serviceResponse.Status = false;
                                                serviceResponse.Message = $"The specified bill type '{policyDetails.BillType}' is not configured in the accounting system.";
                                                return serviceResponse;
                                            }
                                        }
                                        //}
                                        #region EmptyPaymentPlan
                                        //else if (string.IsNullOrEmpty(accountingPost_Req.PaymentPlan)/* || PaymentPlan_Id == MasterLookup._PaymentPlans.AnnualFullPay*/)
                                        //{
                                        //    if (PolicyDetailsLineItemsList?.Count() > 0)
                                        //    {

                                        //        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                                        //        List<LU_LineItems_Clients> LineItemsClientsList = _lookupRepository.GetLineItems_ClientsByBillingType_Id(AuthenticatedUser.Client_Id, BillingType_Id);
                                        //        if (LineItemsClientsList == null)
                                        //        {
                                        //            if (PolicyDetailsTransaction.Connection != null)
                                        //            {
                                        //                PolicyDetailsTransaction.Rollback();
                                        //            }
                                        //            serviceResponse.Status = false;
                                        //            serviceResponse.Message = $"The specified bill type '{policyDetails.BillType}' has no line item configuration. Please contact the administrator.";
                                        //            return serviceResponse;
                                        //        }
                                        //        List<string> missingDescriptions = new List<string>();
                                        //        bool AreAllDescriptionsInLineTypes(
                                        //                List<LU_LineItems_Clients> firstList,
                                        //                List<PolicyDetailsLineItems> secondList)
                                        //        {
                                        //            var lineTypeNames = new HashSet<string>(firstList.Select(x => x.LineItemName));
                                        //            var distinctDescriptions = secondList.Where(z => z.LineType.ToLower().Trim() == "fee" || z.LineType.ToLower().Trim() == "tax")
                                        //                .Select(x => x.PolicyDetailDescription)
                                        //                .Distinct();
                                        //            missingDescriptions = distinctDescriptions
                                        //                .Where(desc => !lineTypeNames.Contains(desc))
                                        //                .ToList();
                                        //            return distinctDescriptions.All(desc => lineTypeNames.Contains(desc));
                                        //        }
                                        //        if (!AreAllDescriptionsInLineTypes(LineItemsClientsList, PolicyDetailsLineItemsList))
                                        //        {
                                        //            if (PolicyDetailsTransaction.Connection != null)
                                        //            {
                                        //                PolicyDetailsTransaction.Rollback();
                                        //            }
                                        //            serviceResponse.Status = false;
                                        //            serviceResponse.Message = $"The Line Items: {string.Join(", ", missingDescriptions)} are not configured in Accounting. Please contact system administrator.";
                                        //            return serviceResponse;
                                        //        }
                                        //        if (BillingType_Id > 0)
                                        //        {
                                        //            if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                        //            {
                                        //                if (PolicyDetailsTransaction.Connection != null)
                                        //                {
                                        //                    PolicyDetailsTransaction.Rollback();
                                        //                }
                                        //                serviceResponse.Status = false;
                                        //                serviceResponse.Message = $"The specified bill type '{policyDetails.BillType}' is not configured in the accounting system.";
                                        //                return serviceResponse;
                                        //            }
                                        //            else
                                        //            {
                                        //                Invoices invoices = _invoiceRepository.GetInvoices(policyDetails.Id) ?? new Invoices();
                                        //                invoices.PolicyDetails_Id = policyDetails.Id;
                                        //                invoices.InvoiceNo = string.IsNullOrEmpty(invoices.InvoiceNo) ? null : invoices.InvoiceNo;
                                        //                //invoices.DueDate = invoices.DueDate == default ? DateTime.Today.AddDays(30) : invoices.DueDate;
                                        //                invoices.InvoiceNote = accountingPost_Req.InvoiceNote;
                                        //                invoices.Amount = GetInvoiceAmount(PolicyDetailsLineItemsList, BillingType_Id);
                                        //                invoices.RemainingBalance = invoices.Amount;
                                        //                invoices.InstallmentNo = 1;
                                        //                invoices.InvoiceEffectiveDate = policyDetails.TransactionType == MasterLookup._EITransactionTypes.NEW ? policyDetails.EffectiveDate : policyDetails.PolicyDetailsLineItemsList?.FirstOrDefault()?.EffectiveDate ?? policyDetails.EffectiveDate;
                                        //                invoices = _invoiceRepository.InsertUpdateInvoices(PolicyDetailsTransaction, invoices);
                                        //                Invoice_Id = invoices.Id;
                                        //                InvoiceNumber = invoices.InvoiceNo;
                                        //                policyDetails.Invoices = invoices;
                                        //                var PaymentApplicationsList = _paymentsRepository.GetAllPaymentApplicationLineItemsByInvoice_Id(invoices.Id);
                                        //                if (PaymentApplicationsList?.Count() > 0)
                                        //                {
                                        //                    if (PolicyDetailsTransaction.Connection != null)
                                        //                    {
                                        //                        PolicyDetailsTransaction.Rollback();
                                        //                    }
                                        //                    serviceResponse.Status = false;
                                        //                    serviceResponse.Message = $"Payment applications already exist for invoice '{invoices.InvoiceNo}' (Policy Number: '{policyDetails.PolicyNumber}').";
                                        //                    return serviceResponse;
                                        //                }
                                        //                _invoiceRepository.InactiveInvoiceDetails(invoices.Id);
                                        //                var GroupedByCoverage = PolicyDetailsLineItemsList.GroupBy(o => o.Coverage);
                                        //                if (GroupedByCoverage?.Count() > 0)
                                        //                {
                                        //                    foreach (var item in GroupedByCoverage)
                                        //                    {
                                        //                        byte LOB_Id = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.Name == item.Key)?.LOB_Id ?? 0;
                                        //                        var PolicyDetailsLineItemsCoverageList = PolicyDetailsLineItemsList.Where(x => x.Coverage == item.Key).ToList();
                                        //                        if (PolicyDetailsLineItemsCoverageList?.Count() > 0)
                                        //                        {
                                        //                            List<InvoiceDetails> invoiceDetailsList = GetInvoiceDetails(BillingType_Id, invoices, policyDetails, PolicyDetailsLineItemsCoverageList, LOB_Id);
                                        //                            if (invoiceDetailsList.Count() > 0)
                                        //                            {
                                        //                                foreach (var invoice in invoiceDetailsList)
                                        //                                {
                                        //                                    if (invoice.EntityType_Id > 0 && invoice.Entity_Id > 0 && invoice.Amount != 0)
                                        //                                    {
                                        //                                        _invoiceRepository.InsertUpdateInvoiceDetails(PolicyDetailsTransaction, invoice);
                                        //                                        policyDetails.Invoices.InvoicesDetailsList.Add(invoice);
                                        //                                    }
                                        //                                }
                                        //                            }


                                        //                        }
                                        //                    }
                                        //                }
                                        //                if (Clients.AccountingService_Id == MasterLookup._Services.Intuit && Clients.AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                        //                {
                                        //                    PolicyDetails PolicyDetailsJournalEntry = policyDetails;
                                        //                    PolicyDetailsJournalEntry.Invoices = invoices;
                                        //                    JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoices.Id);
                                        //                    QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                        //                    if (journalEntries != null)
                                        //                    {
                                        //                        updateInvoiceJournalResponse = qBOUtility.UpdateInvoiceJournalEntry(PolicyDetailsTransaction, PolicyDetailsJournalEntry, journalEntries);
                                        //                        if (updateInvoiceJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(updateInvoiceJournalResponse?.JournalEntry?.Id))
                                        //                        {
                                        //                            if (PolicyDetailsTransaction.Connection != null)
                                        //                            {
                                        //                                PolicyDetailsTransaction.Rollback();
                                        //                            }
                                        //                            serviceResponse.Status = false;
                                        //                            serviceResponse.Message = "Cannot sync invoice with Intuit!" + createInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                        //                            return serviceResponse;
                                        //                        }
                                        //                    }
                                        //                    else
                                        //                    {
                                        //                        createInvoiceJournalResponse = qBOUtility.CreateInvoiceJournalEntry(PolicyDetailsTransaction, PolicyDetailsJournalEntry);
                                        //                        if (createInvoiceJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(createInvoiceJournalResponse?.JournalEntry?.Id))
                                        //                        {
                                        //                            if (PolicyDetailsTransaction.Connection != null)
                                        //                            {
                                        //                                PolicyDetailsTransaction.Rollback();
                                        //                            }
                                        //                            serviceResponse.Status = false;
                                        //                            serviceResponse.Message = "Cannot sync invoice with Intuit!" + createInvoiceJournalResponse?.Fault?.Error?[0]?.Detail;
                                        //                            return serviceResponse;
                                        //                        }
                                        //                    }

                                        //                }
                                        //            }

                                        //        }
                                        //        else
                                        //        {
                                        //            if (PolicyDetailsTransaction.Connection != null)
                                        //            {
                                        //                PolicyDetailsTransaction.Rollback();
                                        //            }
                                        //            serviceResponse.Status = false;
                                        //            serviceResponse.Message = $"The specified bill type '{policyDetails.BillType}' is not configured in the accounting system.";
                                        //            return serviceResponse;
                                        //        }
                                        //    }

                                        //}
                                        #endregion
                                        PolicyDetailsTransaction.Commit();
                                        if (!string.IsNullOrEmpty(accountingPost_Req.PaymentPlan) && string.IsNullOrEmpty(LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.IsSingleInvoiceCreated)))
                                        {
                                            if (accountingPost_Req.TransactionType == MasterLookup._EITransactionTypes.CAN)
                                            {
                                                var processNetPaymentCancellationInvoice = LookupUtility.GetClientSettings(Clients.Client_Id, accountingPost_Req.PaymentPlan);
                                                if (!string.IsNullOrEmpty(processNetPaymentCancellationInvoice))
                                                {
                                                    var payment = CreateCancellationPayment(policyDetails, Clients, AuthenticatedUser);
                                                    if (payment.Payment_Id > 0)
                                                    {
                                                        totalInvoiceAmount = totalInvoiceAmount * (-1);
                                                        var cancellationDate = policyDetails.TransactionType == MasterLookup._EITransactionTypes.NEW ? policyDetails.EffectiveDate : policyDetails.PolicyDetailsLineItemsList?.FirstOrDefault()?.EffectiveDate ?? policyDetails.EffectiveDate;
                                                        var processCancellation = ProcessCancellation(Clients.Client_Id, cancellationDate, payment, Convert.ToInt64(accountingPost_Req.OriginalSubmissionID), Clients, AuthenticatedUser, totalInvoiceAmount);
                                                    }
                                                }
                                            }
                                        }
                                        try
                                        {
                                            var pdfResults = new List<object>();

                                            policyDetails.Invoices = _invoiceRepository.GetInvoicesByInvoice_Id(Invoice_Id);
                                            policyDetails.Invoices.CreatedOn = insertedInvoices
                                            .FirstOrDefault(x => x.PolicyPaymentSchedule_Id == policyDetails.Invoices.Schedule_Id)
                                            ?.CreatedOn
                                            ?? policyDetails.Invoices.CreatedOn;
                                            policyDetails.Invoices.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(Invoice_Id);
                                            if (policyDetails.Invoices.InvoicesDetailsList?.Count() > 0)
                                            {
                                                policyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(policyDetails.Invoices.InvoicesDetailsList, policyDetails, true);
                                            }

                                            string maskedAccountNumber = "";
                                            string PostalCode = string.Empty;
                                            string entityCode = string.Empty;
                                            string encodedId = string.Empty;
                                            string entityRefId = string.Empty;
                                            string encodedCompany = string.Empty;
                                            long clientId = Clients.Client_Id;
                                            string companyCode = Clients.CompanyCode;
                                            ExternalAccountInformation externalAccountInformation = new ExternalAccountInformation();
                                            if (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                                            {
                                                entityCode = MasterLookup._AccountingCodes.Agent;
                                                entityRefId = agentDetails.AgentReference_Id;
                                                PostalCode = policyDetails.AgentDetails.ZipCode;
                                                externalAccountInformation.EntityType_Id = MasterLookup._EntityTypes.Agency;
                                                externalAccountInformation.Entity_Id = agentDetails.AgentDetail_Id;
                                            }
                                            else if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                            {
                                                entityCode = MasterLookup._AccountingCodes.Insured;
                                                entityRefId = insuredDetails.InsuredReference_Id;
                                                PostalCode = policyDetails.InsuredDetails.ZipCode;
                                                externalAccountInformation.EntityType_Id = MasterLookup._EntityTypes.Insured;
                                                externalAccountInformation.Entity_Id = insuredDetails.InsuredDetail_Id;
                                            }
                                            else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                                            {
                                                entityCode = MasterLookup._AccountingCodes.Mortgagee;
                                                entityRefId = mortgageeDetails?.MortgageeReference_Id;
                                                PostalCode = policyDetails.MortgageeDetails.ZipCode;
                                                externalAccountInformation.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                                                externalAccountInformation.Entity_Id = mortgageeDetails.MortgageeDetail_Id;
                                            }
                                            var externalAccountinfo = _externalAccountInformationRepository.GetExternalAccountInformation(externalAccountInformation.Entity_Id, externalAccountInformation.EntityType_Id, clientId);
                                            string AllowExternalAccountingID = LookupUtility.GetClientSettings(clientId, MasterLookup._ClientSettings.AllowExternalAccountingID);
                                            if (!string.IsNullOrWhiteSpace(AllowExternalAccountingID) && BillingType_Id == MasterLookup._BillingTypes.Agency)
                                            {
                                                externalAccountInformation = new ExternalAccountInformation()
                                                {
                                                    EntityType_Id = MasterLookup._EntityTypes.Agency,
                                                    Entity_Id = policyDetails.AgentDetail_Id,
                                                    AccountNumber = policyDetails.AgentDetails.EI_Accounting_Id,
                                                    Client_Id = policyDetails.Client_Id
                                                };
                                            }
                                            else
                                            {
                                                if (externalAccountinfo != null)
                                                {
                                                    externalAccountInformation = externalAccountinfo;
                                                    externalAccountInformation.ZipCode = PostalCode;
                                                    maskedAccountNumber = externalAccountinfo.AccountNumber;
                                                }
                                                else
                                                {
                                                    if (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed || BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                                                    {
                                                        string rawValue = $"{externalAccountInformation.Entity_Id}";
                                                        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawValue));

                                                        encodedId = base64
                                                            .Replace("=", "")
                                                            .Replace("+", "")
                                                            .Replace("/", "")
                                                            .ToUpper();

                                                        encodedId = encodedId.Substring(0, Math.Min(8, encodedId.Length));

                                                        string base64Company = Convert.ToBase64String(Encoding.UTF8.GetBytes(companyCode))
                                                            .Replace("=", "");

                                                        encodedCompany = base64Company
                                                            .Substring(0, Math.Min(6, base64Company.Length))
                                                            .ToUpper();

                                                        maskedAccountNumber = $"{encodedCompany}-{entityCode}-{encodedId}";


                                                        externalAccountInformation.Client_Id = Clients.Client_Id;
                                                        externalAccountInformation.AccountNumber = maskedAccountNumber;
                                                        externalAccountInformation.ZipCode = PostalCode;
                                                        _externalAccountInformationRepository.InsertUpdateExternalAccountInformation(externalAccountInformation);
                                                    }
                                                }
                                            }

                                            if (Clients.PaymentService_Id == MasterLookup._Services.ePay)
                                            {
                                                string paymentUrlTemplate = LookupUtility.GetClientSettings(clientId, MasterLookup._ClientSettings.InvoicePaymentURLTemplate);
                                                if (!string.IsNullOrWhiteSpace(paymentUrlTemplate))
                                                {
                                                    string finalPaymentUrl = CommonUtility.ApplyTemplateFromObject(paymentUrlTemplate, externalAccountInformation);
                                                    externalAccountInformation.PaymentURL = finalPaymentUrl;
                                                }
                                            }
                                            policyDetails.ExternalAccountInformation = externalAccountInformation;
                                            byte[] coverPayPaymentLogo = null;
                                            if (Clients?.PaymentService_Id == MasterLookup._Services.CorePay && (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed || BillingType_Id == MasterLookup._BillingTypes.Mortgagee))
                                            {
                                                var enableCoverPayPaymentLink = LookupUtility.GetClientSettings(clientId, MasterLookup._ClientSettings.EnableCoverPayPaymentLink);
                                                if (!string.IsNullOrEmpty(enableCoverPayPaymentLink))
                                                {
                                                    var enableCoverPayStripePaymentLink = LookupUtility.GetClientSettings(clientId, MasterLookup._ClientSettings.EnableCoverPayStripePaymentLink);
                                                    if (!string.IsNullOrEmpty(enableCoverPayStripePaymentLink))
                                                    {
                                                        var coverPayExternalClient_Id = LookupUtility.GetClientSettings(clientId, MasterLookup._ClientSettings.CoverPayExternalClient_Id);
                                                        var coverPayExternalPaymentLinkURL = LookupUtility.GetClientSettings(clientId, MasterLookup._ClientSettings.CoverPayExternalPaymentLinkURL);
                                                        if (!string.IsNullOrEmpty(coverPayExternalClient_Id) && !string.IsNullOrEmpty(coverPayExternalPaymentLinkURL))
                                                        {
                                                            string dynamicCoverPayURL = string.Format(coverPayExternalPaymentLinkURL, coverPayExternalClient_Id, maskedAccountNumber, PostalCode);
                                                            policyDetails.ExternalPaymentLink = dynamicCoverPayURL;
                                                            var coverPayimagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "CoverPay", "coverpay-payment-logo.png");
                                                            coverPayPaymentLogo = System.IO.File.ReadAllBytes(coverPayimagePath);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var externalPaymentDetails = coverPayService.GetExternalPaymentDetails(clientId, MasterLookup._Services.CorePay, policyDetails.Id);
                                                        if (string.IsNullOrEmpty(externalPaymentDetails?.PaymentLink))
                                                        {
                                                            var coverPayEntityType = string.Empty;
                                                            var coverPayEntityName = string.Empty;
                                                            var coverPayEntityEmail = string.Empty;
                                                            if (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                                                            {
                                                                coverPayEntityType = BillingType_Id == MasterLookup._BillingTypes.Agency ? "Agency" : "AgencyDetailed";
                                                                coverPayEntityName = policyDetails?.AgentDetails?.Name ?? string.Empty;
                                                                coverPayEntityEmail = policyDetails?.AgentDetails?.Email ?? string.Empty;
                                                            }
                                                            else if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                                            {
                                                                coverPayEntityType = "Insured";
                                                                coverPayEntityName = policyDetails?.InsuredDetails?.Name ?? string.Empty;
                                                                coverPayEntityEmail = policyDetails?.InsuredDetails?.Email ?? string.Empty;
                                                            }
                                                            else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                                                            {
                                                                coverPayEntityType = "Mortgagee";
                                                                coverPayEntityName = policyDetails?.MortgageeDetails?.Name ?? string.Empty;
                                                                coverPayEntityEmail = policyDetails?.MortgageeDetails?.Email ?? string.Empty;
                                                            }

                                                            var coverPayURL = coverPayService.GenerateCoverPayPaymentLink(clientId, policyDetails.Id, policyDetails?.PolicyNumber ?? string.Empty,
                                                            AuthenticatedUser.User_Id, coverPayEntityType, coverPayEntityName, coverPayEntityEmail,
                                                            policyDetails?.ExternalAccountInformation?.AccountNumber, policyDetails?.ExternalAccountInformation?.ZipCode);
                                                            if (!string.IsNullOrEmpty(coverPayURL))
                                                            {
                                                                policyDetails.ExternalPaymentLink = coverPayURL;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            policyDetails.ExternalPaymentLink = externalPaymentDetails?.PaymentLink;
                                                        }
                                                    }
                                                }
                                            }
                                            policyDetails.TotalAppliedAmount = Convert.ToDouble(_paymentsRepository.GetInvoiceAppliedPaymentDetails(policyDetails.Invoices.Id, Clients.Client_Id)?.TotalAppliedAmount);
                                            //policyDetails.InvoicesList = _invoiceRepository.GetInvoicesByPolicyDetail_Id(policyDetails.Id);
                                            var viewDataDictionary = new ViewDataDictionary
                                           (
                                           new EmptyModelMetadataProvider(),
                                                new ModelStateDictionary()
                                           )
                                            {
                                                Model = policyDetails,
                                            };
                                            var htmlContent = this.view.Render("Invoices/InvoicePDF", viewDataDictionary);
                                            byte[] CarrierLogo = null;
                                            var SettingsLogo = CommonUtility.GetEntitySettingValue<byte[]>((Clients?.Client_Id ?? 0), MasterLookup._EntityTypes.Carrier, policyDetails.CarrierDetail_Id, MasterLookup._Settings.InvoiceCarrierLogo);
                                            if (SettingsLogo?.Count() > 0)
                                            {
                                                CarrierLogo = SettingsLogo;
                                            }
                                            byte[] bytes = null;
                                            var hasDefaultTemplate = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.RTWHasDefaultTemplate);
                                            if (!string.IsNullOrEmpty(hasDefaultTemplate))
                                            {
                                                var Footer = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.InvoiceFooter);
                                                PDFRequest request = new PDFRequest
                                                {
                                                    BodyHTMLString = htmlContent,
                                                    HeaderHTMLString = "",
                                                    FooterHTMLString = Footer == null ? "" : Footer,
                                                    IsCoverPageExcluded = false
                                                };

                                                string serviceUrl = MasterLookup._PDFCoverterAPIURL;
                                                ServiceLogInfo logInfo = new ServiceLogInfo();
                                                ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(logInfo, serviceUrl, request, typeof(ServiceResponse), true);
                                                if (response.Status)
                                                {
                                                    bytes = Convert.FromBase64String(response.Data.ToString());
                                                }
                                            }
                                            else
                                            {
                                                bytes = _IPDFOptions.ExportToPdf(htmlContent, Clients.Logo, CarrierLogo, coverPayPaymentLogo, policyDetails.ExternalPaymentLink);
                                            }
                                            serviceResponse.Data = new { InvoiceNumber = InvoiceNumber, InvoiceName = "Invoice", InvoiceDate = InvoiceDate, PaymentDueOn = PaymentDueOn, NoOfInstallments = NoOfInstallments, PDFBytes = bytes, ExternalAccountingInformation = new { EntityCode = entityCode, EntityReferenceId = entityRefId, AccountNumber = maskedAccountNumber } };
                                            serviceResponse.Status = true;
                                            serviceResponse.Message = "Success";
                                            return serviceResponse;
                                        }
                                        catch (Exception ex)
                                        {
                                            FileLogger.LogError($"FinancialServicesController: GenerateInvoice: Client_Id: {Clients?.Client_Id}: SubmissionId: {accountingPost_Req?.SubmissionId}: QuoteId: {accountingPost_Req?.QuoteId} Exception: {ex} Message: {ex.Message}");
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Failed to generate the Invoice. Something went wrong while processing the request. Please try again, or contact support if the issue persists.";
                                            return serviceResponse;
                                        }
                                    }
                                }
                                else
                                {
                                    serviceResponse.Status = false;
                                    serviceResponse.Message = "SubmissionId or PolicyNumber or QuoteID cannot be empty.";
                                    return serviceResponse;
                                }
                            }
                            else
                            {
                                serviceResponse.Status = false;
                                serviceResponse.Message = "Request payload cannot be null.";
                                return serviceResponse;

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (PolicyDetailsTransaction.Connection != null)
                        {
                            PolicyDetailsTransaction.Rollback();
                        }
                        FileLogger.LogError($"FinancialServicesController: GenerateInvoice: Client_Id: {Clients?.Client_Id}: SubmissionId: {accountingPost_Req?.SubmissionId}: QuoteId: {accountingPost_Req?.QuoteId} Exception: {ex} Message: {ex.Message}");
                        serviceResponse.Status = false;
                        serviceResponse.Message = "Failed to generate the Invoice. Something went wrong while processing the request. Please try again, or contact support if the issue persists.";
                    }
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid API Credentials.";
                    return serviceResponse;
                }

            }
            else
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Username cannot be null or empty.";
                return serviceResponse;
            }

            return serviceResponse;
        }
        [HttpGet]
        public ServiceResponse GetTransactionDetails(long QuoteId)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            if (QuoteId == 0)
                return new ServiceResponse { Status = false, Message = "QuoteId cannot be null or empty." };
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                    return new ServiceResponse { Status = false, Message = "Username cannot be null or empty." };

                var authenticatedUser = _usersRepository.GetUsers(username);
                var client = _accountRepository.GetClients(authenticatedUser.Client_Id);
                if (client == null)
                    return new ServiceResponse { Status = false, Message = "Invalid API Credentials." };

                var transactionDetails = _policyDetailsRepository.GetTransactionDetails(QuoteId, client.Client_Id);
                if (transactionDetails == null)
                    return new ServiceResponse { Status = false, Message = "No records found." };

                return new ServiceResponse
                {
                    Status = true,
                    Message = "Success",
                    Data = transactionDetails
                };
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        [HttpGet]
        public ServiceResponse GetARAPTransactionDetails(long quoteId, string type)
        {
            if (quoteId == 0 && string.IsNullOrEmpty(type))
            {
                return new ServiceResponse
                {
                    Status = false,
                    Message = "QuoteId and Type cannot be null or empty."
                };
            }

            if (quoteId == 0)
            {
                return new ServiceResponse
                {
                    Status = false,
                    Message = "QuoteId cannot be null or empty."
                };
            }

            if (string.IsNullOrEmpty(type))
            {
                return new ServiceResponse
                {
                    Status = false,
                    Message = "Type cannot be null or empty."
                };
            }

            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                    return new ServiceResponse { Status = false, Message = "Username cannot be null or empty." };

                var authenticatedUser = _usersRepository.GetUsers(username);
                var client = _accountRepository.GetClients(authenticatedUser.Client_Id);

                if (client == null)
                    return new ServiceResponse { Status = false, Message = "Invalid API Credentials." };

                var transactions = _policyDetailsRepository
                    .GetAllTransactionDetailsByDirections(quoteId, client.Client_Id, type);

                return new ServiceResponse
                {
                    Status = true,
                    Message = "Success",
                    Data = transactions
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }

        [HttpGet]
        [Route("/api/v2/invoices")]
        public IActionResult GetAllOpenInvoices(string? accountNumber = null, string? postalCode = null, string? clientId = null, string? querySearch = null)
        {
            InvoicesRes invoicesRes = new InvoicesRes();
            string username = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var AuthenticatedUser = _usersRepository.GetUsers(User.Identity?.Name);
                var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);
                string allowOpenInvoicesAPI = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.AllowOpenInvoicesAPI);
                if (Clients != null && (Clients.PaymentService_Id == MasterLookup._Services.ePay || Clients.PaymentService_Id == MasterLookup._Services.CorePay || !string.IsNullOrEmpty(allowOpenInvoicesAPI)))
                {
                    try
                    {
                        IncomingRequests incomingRequests = new IncomingRequests();
                        incomingRequests.Service_Id = MasterLookup._Services.ePay;
                        incomingRequests.RequestBody = $"accountNumber={accountNumber}&postalCode={postalCode}&client_Id={clientId}&querySearch={querySearch}";
                        incomingRequests.CreatedBy = username;
                        incomingRequests = _incomingRequestsRepository.InsertIncomingRequests(incomingRequests);
                        if (string.IsNullOrEmpty(accountNumber) && string.IsNullOrEmpty(querySearch))
                        {
                            return BadRequest("Either AccountNumber or Query is required.");
                        }

                        long entity_Id = 0;
                        byte entityType_Id = 0;
                        bool IsAllowPartialPayment = true;
                        string ruleValue = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.ePayAllowPartialPayment);
                        if (ruleValue != null)
                        {
                            Boolean.TryParse(ruleValue, out IsAllowPartialPayment);
                        }
                        if (string.IsNullOrEmpty(querySearch))
                        {
                            ExternalAccountInformation externalAccountInformation = _externalAccountInformationRepository.GetExternalAccountInformationByAccountNumber(accountNumber, Clients.Client_Id);
                            if (externalAccountInformation != null)
                            {
                                if (externalAccountInformation.Entity_Id > 0 && externalAccountInformation.EntityType_Id > 0)
                                {
                                    EntityInfo entityInfo = _policyDetailsRepository.GetEntityInfo(Clients.Client_Id, externalAccountInformation.Entity_Id, externalAccountInformation.EntityType_Id);
                                    if (entityInfo != null)
                                    {
                                        invoicesRes.emailAddresses.Add(entityInfo.Email);
                                        invoicesRes.payerName = entityInfo.PayerName;
                                        entity_Id = externalAccountInformation.Entity_Id;
                                        entityType_Id = externalAccountInformation.EntityType_Id;
                                    }
                                }
                            }
                        }

                        List<InvoiceList> OpenInvoiceList = _policyDetailsRepository.GetAllOpenInvoices(Clients.Client_Id, entity_Id, entityType_Id, querySearch);
                        if (OpenInvoiceList?.Count > 0)
                        {
                            invoicesRes.invoiceAttributeMetadata.Add(new InvoiceAttributeMetadatum()
                            {
                                displayOrder = 1,
                                parameterName = "policyNumber",
                                displayName = "Policy Number"
                            });
                            invoicesRes.invoiceAttributeMetadata.Add(new InvoiceAttributeMetadatum()
                            {
                                displayOrder = 2,
                                parameterName = "effectiveDate",
                                displayName = "Effective Date"
                            });

                            foreach (var item in OpenInvoiceList)
                            {
                                Invoice invoice = new Invoice();
                                invoiceUnderwriting invoiceUnderwritings = new invoiceUnderwriting();
                                Address address = new();
                                Address mailingAddress = new();
                                Contact carrierContact = new();
                                Contact mgaContact = new();
                                Contact retailAgentContact = new();
                                invoice.id = $"{item.InvoiceNo}-{item.InvoiceId}";
                                invoice.name = item.InsuredName;
                                invoice.dueDate = item.DueDate;
                                invoice.amount = (decimal)item.RemainingBalance;
                                invoice.maximumAmount = (decimal)item.RemainingBalance;
                                invoice.allowPartialPayment = IsAllowPartialPayment;
                                invoice.divisionId = item?.CarrierReferenceId;
                                invoice.invoiceUnderwriting = new invoiceUnderwriting()
                                {
                                    billType = item.BillType,
                                    allowInsuredPayment = item.BillType == "Direct" || item.BillType == "Insured" ? true : false,
                                    policyNumber = item.PolicyNumber,
                                    agencyId = item.Agency_Id,
                                    agencyName = item.AgencyName,
                                    insuredName = item.InsuredName,
                                    insuredEmail = item.InsuredEmail,
                                    insuredPhone = item.InsuredPhone,
                                    effectiveDate = item.EffectiveDate,
                                    expirationDate = item.ExpirationDate,
                                    coverageCode = item.CoverageCode,
                                    coverageDescription = item.CoverageCodeDescription?.Trim(),
                                    flagFinanced = item.FlagFinanced,
                                    billingType = item.BillingType,
                                    minimumPremium = (decimal)item.MinimumPremium,
                                    invoiceTypeID = item.InvoiceTypeID,
                                    invoiceTypeDescription = null,
                                    totalFees = (decimal)item.TotalFees,
                                    totalTaxes = (decimal)item.TotalTaxes,
                                    customerType = item.CustomerType,
                                    phone = item.ClientPhone,
                                    email = item.ClientEmail,
                                    dbaName = item.DBAName,
                                    address = new Address
                                    {
                                        suite = item.InsuredPhysicalSuite,
                                        street = item.InsuredPhysicalStreet,
                                        city = item.InsuredPhysicalCity,
                                        state = item.InsuredPhysicalState,
                                        postalCode = item.InsuredPhysicalPostalCode,
                                        country = 0
                                    },
                                    mailingAddress = new Address
                                    {
                                        suite = item.InsuredMailingSuite,
                                        street = item.InsuredMailingStreet,
                                        city = item.InsuredMailingCity,
                                        state = item.InsuredMailingState,
                                        postalCode = item.InsuredMailingPostalCode,
                                        country = 0

                                    },
                                    carrierContact = new Contact
                                    {
                                        companyID = null,
                                        companyName = item.CarrierName,

                                        companyNAIC = item.CarrierNAIC ?? string.Empty,
                                        phone = null,

                                        suite = item.CarrierSuite,

                                        street = item.CarrierStreet,

                                        city = item.CarrierCity,

                                        state = item.CarrierSuite,

                                        postalCode = item.CarrierPostalCode,

                                        country = 0
                                    },
                                    mgaContact = new Contact
                                    {
                                        companyID = null,
                                        companyName = item.MGAContactName,

                                        companyNAIC = null,
                                        phone = item.MGAContactPhone,

                                        suite = null,

                                        street = item.MGAContactStreet,

                                        city = item.MGAContactCity,

                                        state = item.MGAContactState,

                                        postalCode = item.MGAContactPostalCode,

                                        country = 0
                                    },
                                    retailAgentContact = new Contact
                                    {
                                        companyID = item.Agency_Id,
                                        companyName = item.RetailAgentName,

                                        companyNAIC = null,
                                        phone = null,

                                        suite = item.RetailAgentSuite,

                                        street = item.RetailAgentStreet,

                                        city = item.RetailAgentCity,

                                        state = item.RetailAgentState,

                                        postalCode = item.RetailAgentPostalCode,

                                        country = 0

                                    }
                                };

                                invoice.attributeValues.Add(new AttributeValue()
                                {
                                    parameterName = "policyNumber",
                                    value = item.PolicyNumber
                                });
                                invoice.attributeValues.Add(new AttributeValue()
                                {
                                    parameterName = "effectiveDate",
                                    value = item.EffectiveDate.ToString(MasterLookup._ServerDateFormat)
                                });
                                if (!string.IsNullOrEmpty(item.External_AccountNumber))
                                {
                                    invoice.attributeValues.Add(new AttributeValue()
                                    {
                                        parameterName = "AccountNumber",
                                        value = item.External_AccountNumber
                                    });
                                }
                                if (!string.IsNullOrEmpty(item.External_PostalCode))
                                {
                                    invoice.attributeValues.Add(new AttributeValue()
                                    {
                                        parameterName = "PostalCode",
                                        value = item.External_PostalCode
                                    });
                                }
                                invoicesRes.invoices.Add(invoice);
                            }
                        }
                        //else
                        //{
                        //    return BadRequest("Invalid AccountNumber");
                        //}
                    }
                    catch (Exception ex)
                    {

                        ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                        exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                        exceptionLogInfo.ErrorStackTrace = ex.StackTrace;
                        exceptionLogInfo.ErrorMessage = ex.Message;
                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                    }
                }

            }

            return Ok(invoicesRes);
        }

        [HttpPost]
        [Route("/api/v2/CashReceipts")]
        public IActionResult PostCashReceipts([FromBody] JsonElement data)
        {
            string username = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var AuthenticatedUser = _usersRepository.GetUsers(User.Identity?.Name);
                var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);
                if (Clients != null && (Clients.PaymentService_Id == MasterLookup._Services.ePay || Clients.PaymentService_Id == MasterLookup._Services.CorePay))
                {
                    PostCashReceiptsReq postCashReceiptsReq = null;
                    PostCashReceiptsRes postCashReceiptsResponse = new PostCashReceiptsRes();
                    List<string> ClientNames = new List<string>();
                    try
                    {
                        string rawJson = data.GetRawText();
                        postCashReceiptsReq = JsonConvert.DeserializeObject<PostCashReceiptsReq>(rawJson);
                        if (postCashReceiptsReq != null)
                        {
                            IncomingRequests incomingRequests = new IncomingRequests();
                            incomingRequests.Service_Id = Clients.PaymentService_Id == MasterLookup._Services.ePay ? MasterLookup._Services.ePay : MasterLookup._Services.CorePay;
                            incomingRequests.RequestBody = JsonConvert.SerializeObject(postCashReceiptsReq);
                            incomingRequests.CreatedBy = AuthenticatedUser.Username;
                            incomingRequests = _incomingRequestsRepository.InsertIncomingRequests(incomingRequests);
                            if (postCashReceiptsReq.postInvoicesRequestModels?.Count() > 0)
                            {
                                PostCashReceipts_Batches Batches = new PostCashReceipts_Batches();
                                Batches.IncomingRequest_Id = incomingRequests.IncomingRequest_Id;
                                Batches.BatchReference_Id = postCashReceiptsReq.batchId.ToString();
                                Batches.Service_Id = Clients.PaymentService_Id == MasterLookup._Services.ePay ? MasterLookup._Services.ePay : MasterLookup._Services.CorePay;
                                Batches = _postCashReceiptsRepository.InsertPostCashReceipts_Batches(Batches);
                                foreach (var item in postCashReceiptsReq.postInvoicesRequestModels)
                                {
                                    PostCashReceipts_PaymentTransactions PaymentTransactions = new PostCashReceipts_PaymentTransactions();
                                    PaymentTransactions.Batch_Id = Batches.Batch_Id;
                                    PaymentTransactions.TransactionReference_Id = item.transactionId.ToString();
                                    PaymentTransactions.ProcessorReference_Id = item.processorId.ToString();
                                    PaymentTransactions.TransactionDate = !string.IsNullOrEmpty(item.checkDate) ? DateTime.Parse(item.checkDate).ToLocalTime() : DateTime.Now;
                                    string RawAccountnumberValue = item.PaymentAttributes.accountNumber;
                                    if (!string.IsNullOrEmpty(RawAccountnumberValue))
                                    {
                                        ExternalAccountInformation externalAccountInformation = _externalAccountInformationRepository.GetExternalAccountInformationByAccountNumber(RawAccountnumberValue, Clients.Client_Id);
                                        if (externalAccountInformation != null)
                                        {
                                            if (externalAccountInformation.Entity_Id > 0 && externalAccountInformation.EntityType_Id > 0)
                                            {
                                                PaymentTransactions.AccountNumber = RawAccountnumberValue;
                                                PaymentTransactions.TransactionAmount = item.paidInvoices?.Sum(x => x.paidAmount) ?? 0;
                                                PaymentTransactions = _postCashReceiptsRepository.InsertPostCashReceipts_PaymentTransactions(PaymentTransactions);
                                                Clients clients = _accountRepository.GetClients(externalAccountInformation.Client_Id);
                                                ClientNames.Add(clients.CompanyName);
                                                if (clients?.PaymentService_Id != MasterLookup._Services.ePay && clients?.PaymentService_Id != MasterLookup._Services.CorePay)
                                                {
                                                    continue;
                                                }
                                                SqlTransaction transaction = _baseRepository.GetNewTransaction();
                                                long Payment_Id = 0;
                                                try
                                                {
                                                    Payments payments = new Payments();
                                                    payments.EntityType_Id = externalAccountInformation.EntityType_Id;
                                                    payments.Entity_Id = externalAccountInformation.Entity_Id;
                                                    payments.PaymentDate = PaymentTransactions.TransactionDate;
                                                    payments.Amount = PaymentTransactions.TransactionAmount;
                                                    payments.RemainingAmount = PaymentTransactions.TransactionAmount;
                                                    var hasDefaultPaymentTypes = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.HasDefaultPaymentTypes);
                                                    if(string.IsNullOrEmpty(hasDefaultPaymentTypes))
                                                    {
                                                        payments.PaymentType_Id = MasterLookup._ePayPaymentTypeCodes.PaymentTypes.GetValueOrDefault(item.processorId.ToString());
                                                    }
                                                    else
                                                    {
                                                        payments.PaymentType_Id = MasterLookup._epayDefaultPaymentTypeCodes.PaymentTypes.GetValueOrDefault(item.processorId.ToString());
                                                    }
                                                    payments.ReferenceNumber = PaymentTransactions.TransactionReference_Id;
                                                    payments.PaymentStatus_Id = MasterLookup._PaymentStatus.Completed;
                                                    payments.Client_Id = externalAccountInformation.Client_Id;
                                                    payments.OperatingBank_Id = _clientServiceDefaultOperatingBankRepository.GetClientServiceDefaultOperatingBank(externalAccountInformation.Client_Id, MasterLookup._ServiceProviderTypes.Payments, clients.PaymentService_Id)?.OperatingBank_Id ?? 0;
                                                    payments.PaymentDirection_Id = MasterLookup._PaymentDirections.Incoming;
                                                    payments = _paymentsRepository.InsertUpdatePayments(transaction, payments);
                                                    if (payments.Payment_Id > 0)
                                                    {
                                                        Payment_Id = payments.Payment_Id;
                                                        CreateJournalRes createJournalResponse = new CreateJournalRes();
                                                        SageResponse createSageJournalResponse = new SageResponse();
                                                        if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                                                        {
                                                            QBOUtility qBOUtility = new QBOUtility(externalAccountInformation.Client_Id, AuthenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                                            createJournalResponse = qBOUtility.CreatePaymentJournalEntry(transaction, payments);
                                                            if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                                                            {
                                                                if (transaction.Connection != null)
                                                                {
                                                                    transaction.Rollback();
                                                                }
                                                                ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                                                                exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                                                                exceptionLogInfo.ErrorStackTrace = "FinancialServiceController/PostCashReceipts/CreateJournalEntry";
                                                                exceptionLogInfo.ErrorMessage = createJournalResponse?.Fault?.Error?[0]?.Message;
                                                                exceptionLogInfo.ErrorName = "Create Payment Journal Entry Error";
                                                                exceptionLogInfo.ErrorDescription = $"Error while creating a payment journal entry for transaction id:{PaymentTransactions.TransactionReference_Id}, AccountNumber: {RawAccountnumberValue}";
                                                                postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                                _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                                continue;
                                                            }
                                                        }
                                                        else if (clients?.AccountingService_Id == MasterLookup._Services.Sage)
                                                        {
                                                            SageUtility sageUtility = new SageUtility(externalAccountInformation.Client_Id, AuthenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                                            var createJournalRes = Task.Run(() => sageUtility.CreatePaymentJournalEntry_Intacct(transaction, payments)).Result;

                                                            var serializer = new XmlSerializer(typeof(SageResponse));
                                                            using (var reader = new StringReader(createJournalRes))
                                                            {
                                                                createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                            }
                                                            if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                            {
                                                                if (transaction.Connection != null)
                                                                {
                                                                    transaction.Rollback();
                                                                }


                                                                ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                                                                exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                                                                exceptionLogInfo.ErrorStackTrace = "FinancialServiceController/PostCashReceipts/CreateJournalEntry";

                                                                var xmlDoc = XDocument.Parse(createJournalRes);

                                                                var status = xmlDoc.Descendants("status")
                                                                                   .FirstOrDefault(x => x.Parent?.Name == "result")
                                                                                   ?.Value;

                                                                if (status == "failure")
                                                                {
                                                                    var errorMsg = xmlDoc.Descendants("description2")
                                                                                         .FirstOrDefault()?.Value;

                                                                    exceptionLogInfo.ErrorMessage = errorMsg;
                                                                }
                                                                exceptionLogInfo.ErrorName = "Create Payment Journal Entry Error";
                                                                exceptionLogInfo.ErrorDescription = $"Error while creating a payment journal entry for transaction id:{PaymentTransactions.TransactionReference_Id}, AccountNumber: {RawAccountnumberValue}";
                                                                postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                                _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                                continue;
                                                            }
                                                        }
                                                    }
                                                    if (item.paidInvoices?.Count() > 0)
                                                    {
                                                        double PaymentsRemainingAmount = payments.RemainingAmount;
                                                        List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
                                                        foreach (var paidInvoice in item.paidInvoices)
                                                        {
                                                            try
                                                            {
                                                                double RemainingBalance = 0;
                                                                bool Paid = false;
                                                                PostCashReceipts_PaidInvoices PaidInvoices = new PostCashReceipts_PaidInvoices();
                                                                PaidInvoices.PaymentTransaction_Id = PaymentTransactions.PaymentTransaction_Id;
                                                                string invoiceIdAndNo = paidInvoice.id;
                                                                long invoiceId = 0;
                                                                if (!string.IsNullOrWhiteSpace(invoiceIdAndNo))
                                                                {
                                                                    var parts = invoiceIdAndNo.Split('-');
                                                                    if (parts.Length > 1 && long.TryParse(parts[1], out long parsedValue))
                                                                    {
                                                                        invoiceId = parsedValue;
                                                                    }
                                                                }
                                                                PaidInvoices.Invoice_Id = invoiceId;
                                                                PaidInvoices.PaidAmount = paidInvoice.paidAmount;
                                                                PaidInvoices.Comment = paidInvoice.comment;
                                                                PaidInvoices.Division = paidInvoice.division;
                                                                PaidInvoices = _postCashReceiptsRepository.InsertPostCashReceipts_PaidInvoices(PaidInvoices);
                                                                if (payments?.Payment_Id > 0)
                                                                {

                                                                    PaymentApplications paymentApplications = new PaymentApplications();
                                                                    paymentApplications.Payment_Id = payments.Payment_Id;
                                                                    paymentApplications.Invoice_Id = PaidInvoices.Invoice_Id;
                                                                    paymentApplications.AppliedDate = DateTime.Now;
                                                                    paymentApplications.TotalAppliedAmount = PaidInvoices.PaidAmount;
                                                                    paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(transaction, paymentApplications);
                                                                    Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(PaidInvoices.Invoice_Id);
                                                                    RemainingBalance = invoices.RemainingBalance - paidInvoice.paidAmount;
                                                                    Paid = RemainingBalance == 0;
                                                                    _invoiceRepository.UpdateBalanceInInvoices(transaction, PaidInvoices.Invoice_Id, RemainingBalance, Paid);
                                                                    List<InvoiceDetails> invoiceDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(PaidInvoices.Invoice_Id, MasterLookup._Directions.AR);
                                                                    if (invoiceDetailsList?.Count() > 0)
                                                                    {
                                                                        invoiceDetailsList = invoiceDetailsList.Where(x => x.RemainingBalance != 0).ToList();
                                                                        if (Paid)
                                                                        {
                                                                            foreach (var invoiceDetail in invoiceDetailsList)
                                                                            {
                                                                                PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                                                                paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                                                                paymentApplicationLineItems.InvoiceLineItem_Id = invoiceDetail.Id;
                                                                                paymentApplicationLineItems.AppliedAmount = invoiceDetail.RemainingBalance;
                                                                                paymentApplicationLineItems.Is_SplitPayment = false;
                                                                                _paymentsRepository.InsertUpdatePaymentApplicationLineItems(transaction, paymentApplicationLineItems);
                                                                                _invoiceRepository.UpdateBalanceInInvoiceDetails(transaction, invoiceDetail.Id, 0, true);
                                                                                PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                                            }
                                                                            PaymentsRemainingAmount = PaymentsRemainingAmount - paidInvoice.paidAmount;
                                                                            _paymentsRepository.UpdatePaymentRemainingAmount(transaction, payments.Payment_Id, PaymentsRemainingAmount);
                                                                        }
                                                                        else
                                                                        {
                                                                            double AllocatedRemainingAmount = PaidInvoices.PaidAmount;
                                                                            var LineTypeList = new List<int>
                                                    {
                                                         MasterLookup._LineItemTypes.Tax,
                                                         MasterLookup._LineItemTypes.Fee,
                                                         MasterLookup._LineItemTypes.CarrierFee,
                                                         MasterLookup._LineItemTypes.Commission,
                                                         MasterLookup._LineItemTypes.Premium
                                                    };
                                                                            foreach (var Type in LineTypeList)
                                                                            {
                                                                                var LOBGroupedLineItems = invoiceDetailsList.Where(x => x.LineItemType_Id == Type).GroupBy(x => x.LOB_Id).ToList();
                                                                                if (LOBGroupedLineItems?.Count() > 0 && AllocatedRemainingAmount != 0)
                                                                                {
                                                                                    foreach (var lobGroup in LOBGroupedLineItems)
                                                                                    {
                                                                                        var LOB_Id = lobGroup.Key;

                                                                                        var LineItemListGroupPerLOB_Id = lobGroup.ToList();

                                                                                        foreach (var lineitem in LineItemListGroupPerLOB_Id)
                                                                                        {
                                                                                            if (AllocatedRemainingAmount != 0)
                                                                                            {
                                                                                                bool Is_HouseCommissionType =
                                                                                                    Type == MasterLookup._LineItemTypes.Commission &&
                                                                                                    LineItemListGroupPerLOB_Id.Any(li =>
                                                                                                    {
                                                                                                        var BillingType_LineItems_Directions = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == li.BillingType_LineItems_Directions_Id);

                                                                                                        var LineItems_Directions = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingType_LineItems_Directions?.LineItems_Directions_Id);

                                                                                                        var LineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItems_Directions?.LineItems_Clients_Id);

                                                                                                        var LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(z => z.LineItem_Id == LineItems_Clients?.LineItem_Id);

                                                                                                        return LineItems?.Name == MasterLookup._CustomLineitems.HouseCommission;
                                                                                                    });

                                                                                                if (Is_HouseCommissionType)
                                                                                                {
                                                                                                    double ActualPremium = 0;
                                                                                                    double CarrierPremium = 0;
                                                                                                    double HseCommissionAmount = 0;

                                                                                                    foreach (var invoiceDetails in invoiceDetailsList)
                                                                                                    {
                                                                                                        if (invoiceDetails.LOB_Id == lineitem.LOB_Id)
                                                                                                        {
                                                                                                            LU_BillingType_LineItems_Directions _BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                                                                                                            LU_BillingTypes _BillingTypes = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.BillingType_Id == _BillingLineItemsDirections?.BillingType_Id);
                                                                                                            LU_LineItems_Directions _LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == _BillingLineItemsDirections?.LineItems_Directions_Id);
                                                                                                            LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == _LineItemsDirections?.LineItems_Clients_Id);
                                                                                                            LU_LineItems _LineItems =
                                                                                                                LookupUtility.GetAllLineItems()
                                                                                                                .FirstOrDefault(c =>
                                                                                                                    c.LineItem_Id ==
                                                                                                                    lineItems_Clients?.LineItem_Id)!;

                                                                                                            if (_BillingLineItemsDirections?.BillingType_Id ==
                                                                                                                MasterLookup._BillingTypes.Agency)
                                                                                                            {
                                                                                                                if (_LineItems?.LineItemType_Id ==
                                                                                                                        MasterLookup._LineItemTypes.Premium &&
                                                                                                                    _LineItemsDirections?.Direction_Id ==
                                                                                                                        MasterLookup._Directions.AR)
                                                                                                                {
                                                                                                                    CarrierPremium = invoiceDetails.Amount;
                                                                                                                }
                                                                                                                else if (_LineItems?.LineItemType_Id ==
                                                                                                                             MasterLookup._LineItemTypes.Commission &&
                                                                                                                         _LineItemsDirections?.Direction_Id ==
                                                                                                                             MasterLookup._Directions.AR)
                                                                                                                {
                                                                                                                    HseCommissionAmount = invoiceDetails.Amount;
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }

                                                                                                    ActualPremium = CarrierPremium + HseCommissionAmount;

                                                                                                    double HseCommPercentage =
                                                                                                        ActualPremium != 0
                                                                                                            ? (HseCommissionAmount / ActualPremium) * 100
                                                                                                            : 0;


                                                                                                    double CalculatedHseCommAmount =
                                                                                                        Math.Round(
                                                                                                            ((double)AllocatedRemainingAmount *
                                                                                                             HseCommPercentage / 100),
                                                                                                            2);

                                                                                                    double ActualApplyAmount =
                                                                                                        Math.Round(
                                                                                                            Math.Sign(lineitem.RemainingBalance) *
                                                                                                            Math.Min(
                                                                                                                Math.Abs(CalculatedHseCommAmount),
                                                                                                                Math.Abs(lineitem.RemainingBalance)), 2);


                                                                                                    if (ActualApplyAmount != 0)
                                                                                                    {
                                                                                                        PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                                                                                        paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                                                                                        paymentApplicationLineItems.InvoiceLineItem_Id = lineitem.Id;
                                                                                                        paymentApplicationLineItems.AppliedAmount = ActualApplyAmount;
                                                                                                        paymentApplicationLineItems.Is_SplitPayment = false;
                                                                                                        paymentApplicationLineItems.SubType_Id = null;
                                                                                                        _paymentsRepository.InsertUpdatePaymentApplicationLineItems(transaction, paymentApplicationLineItems);
                                                                                                        RemainingBalance = Math.Round(lineitem.RemainingBalance - Convert.ToDouble(Math.Sign(lineitem.RemainingBalance) * Math.Abs(ActualApplyAmount)), 2);
                                                                                                        Paid = RemainingBalance == 0;

                                                                                                        _invoiceRepository.UpdateBalanceInInvoiceDetails(
                                                                                                            transaction,
                                                                                                            lineitem.Id,
                                                                                                            RemainingBalance,
                                                                                                            Paid);

                                                                                                        PaymentApplicationLineItemsList.Add(
                                                                                                            paymentApplicationLineItems);

                                                                                                        AllocatedRemainingAmount =
                                                                                                            Math.Round(
                                                                                                                AllocatedRemainingAmount -
                                                                                                                ActualApplyAmount,
                                                                                                                2);
                                                                                                    }
                                                                                                }
                                                                                                else
                                                                                                {

                                                                                                    double _ApplyAmount =
                                                                                                        Math.Round(
                                                                                                            Math.Sign(lineitem.RemainingBalance) *
                                                                                                            Math.Min(
                                                                                                                Math.Abs(AllocatedRemainingAmount),
                                                                                                                Math.Abs(lineitem.RemainingBalance)),
                                                                                                            2);

                                                                                                    if (_ApplyAmount != 0)
                                                                                                    {
                                                                                                        PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                                                                                        paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                                                                                        paymentApplicationLineItems.InvoiceLineItem_Id = lineitem.Id;
                                                                                                        paymentApplicationLineItems.AppliedAmount = _ApplyAmount;
                                                                                                        paymentApplicationLineItems.Is_SplitPayment = false;
                                                                                                        paymentApplicationLineItems.SubType_Id = null;
                                                                                                        _paymentsRepository.InsertUpdatePaymentApplicationLineItems(transaction, paymentApplicationLineItems);
                                                                                                        RemainingBalance = Math.Round(lineitem.RemainingBalance - Convert.ToDouble(Math.Sign(lineitem.RemainingBalance) * Math.Abs(_ApplyAmount)), 2);
                                                                                                        Paid = RemainingBalance == 0;
                                                                                                        _invoiceRepository.UpdateBalanceInInvoiceDetails(transaction, lineitem.Id, RemainingBalance, Paid);
                                                                                                        PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                                                                        AllocatedRemainingAmount = Math.Round(AllocatedRemainingAmount - _ApplyAmount, 2);
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            PaymentsRemainingAmount = PaymentsRemainingAmount - paidInvoice.paidAmount;
                                                                            _paymentsRepository.UpdatePaymentRemainingAmount(transaction, payments.Payment_Id, PaymentsRemainingAmount);
                                                                        }
                                                                    }

                                                                }


                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                                                                exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                                                                exceptionLogInfo.ErrorStackTrace = ex.StackTrace;
                                                                exceptionLogInfo.ErrorMessage = ex.Message;
                                                                exceptionLogInfo.ErrorName = "Payment Application Error";
                                                                exceptionLogInfo.ErrorDescription = $"Error while applying payment for transaction id: {PaymentTransactions.TransactionReference_Id}, invoice: {paidInvoice.id}, account number: {RawAccountnumberValue}";
                                                                postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                                _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                                if (transaction.Connection != null)
                                                                {
                                                                    transaction.Rollback();
                                                                }
                                                            }
                                                        }
                                                        if (PaymentApplicationLineItemsList.Count() > 0)
                                                        {
                                                            if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                                            {
                                                                JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntryWithTransaction(transaction, MasterLookup._Sources.Payment, payments.Payment_Id);
                                                                QBOUtility qBOUtility = new QBOUtility(externalAccountInformation.Client_Id, AuthenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                                                CreateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(transaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
                                                                if (!(updateJournalRes != null && updateJournalRes.JournalEntry?.Id != null))
                                                                {
                                                                    if (transaction.Connection != null)
                                                                    {
                                                                        transaction.Rollback();
                                                                    }
                                                                    qBOUtility.DeleteJournalEntry(journalEntries);
                                                                    ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                                                                    exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                                                                    exceptionLogInfo.ErrorStackTrace = "FinancialServiceController/PostCashReceipts/UpdateJournalEntry";
                                                                    exceptionLogInfo.ErrorMessage = updateJournalRes.Fault?.Error?[0]?.Message;
                                                                    exceptionLogInfo.ErrorName = "Update Payment Journal Entry Error";
                                                                    exceptionLogInfo.ErrorDescription = $"Error while updating payment journal entry for transaction id:{PaymentTransactions.TransactionReference_Id}, account number: {RawAccountnumberValue}";
                                                                    postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                                    _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                                }
                                                                else
                                                                {

                                                                }
                                                            }
                                                            else if (clients?.AccountingService_Id == MasterLookup._Services.Sage)
                                                            {
                                                                SageResponse createSageJournalResponse = new SageResponse();
                                                                SageUtility sageUtility = new SageUtility(externalAccountInformation.Client_Id, AuthenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                                                var createJournalRes = Task.Run(() => sageUtility.CreatePaymentJournalEntry_Intacct(transaction, payments)).Result;

                                                                var serializer = new XmlSerializer(typeof(SageResponse));
                                                                using (var reader = new StringReader(createJournalRes))
                                                                {
                                                                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                                }
                                                                if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                                {
                                                                    if (transaction.Connection != null)
                                                                    {
                                                                        transaction.Rollback();
                                                                    }
                                                                    ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                                                                    exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                                                                    exceptionLogInfo.ErrorStackTrace = "FinancialServiceController/PostCashReceipts/CreateJournalEntry";

                                                                    var xmlDoc = XDocument.Parse(createJournalRes);

                                                                    var status = xmlDoc.Descendants("status")
                                                                                       .FirstOrDefault(x => x.Parent?.Name == "result")
                                                                                       ?.Value;

                                                                    if (status == "failure")
                                                                    {
                                                                        var errorMsg = xmlDoc.Descendants("description2")
                                                                                             .FirstOrDefault()?.Value;

                                                                        exceptionLogInfo.ErrorMessage = errorMsg;
                                                                    }
                                                                    exceptionLogInfo.ErrorName = "Create Payment Journal Entry Error";
                                                                    exceptionLogInfo.ErrorDescription = $"Error while creating a payment journal entry for transaction id:{PaymentTransactions.TransactionReference_Id}, AccountNumber: {RawAccountnumberValue}";
                                                                    postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                                    _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                                    continue;
                                                                }
                                                            }
                                                        }
                                                        if (transaction.Connection != null)
                                                        {
                                                            transaction.Commit();
                                                        }
                                                    }
                                                    if (transaction.Connection != null)
                                                    {
                                                        transaction.Commit();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                                                    exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                                                    exceptionLogInfo.ErrorStackTrace = ex.StackTrace;
                                                    exceptionLogInfo.ErrorMessage = ex.Message;
                                                    exceptionLogInfo.ErrorName = "Payment processing Error";
                                                    exceptionLogInfo.ErrorDescription = $"Error while processing a payment for transaction id:{PaymentTransactions.TransactionReference_Id}, account number: {RawAccountnumberValue}";
                                                    postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                    _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                    JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntryWithTransaction(transaction, MasterLookup._Sources.Payment, Payment_Id);
                                                    if (journalEntries != null)
                                                    {
                                                        QBOUtility qBOUtility = new QBOUtility(externalAccountInformation.Client_Id, AuthenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                                        qBOUtility.DeleteJournalEntry(journalEntries);
                                                    }

                                                    if (transaction.Connection != null)
                                                    {
                                                        transaction.Rollback();
                                                    }
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogInfo exceptionLogInfo = new ExceptionLogInfo();
                        exceptionLogInfo.UserSessionId = AuthenticatedUser.User_Id;
                        exceptionLogInfo.ErrorStackTrace = ex.StackTrace;
                        exceptionLogInfo.ErrorMessage = ex.Message;
                        exceptionLogInfo.ErrorName = "Top - Level Error";
                        exceptionLogInfo.ErrorDescription = "An error occurred while processing the 'post cash receipts.'";
                        postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                    }
                    finally
                    {
                        bool hasErrors = postCashReceiptsResponse?.Errors?.Count() > 0;
                        if (hasErrors)
                        {
                            var distinctNames = ClientNames.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                            string finalResult = distinctNames.Count == 1 ? distinctNames[0] : string.Join(", ", distinctNames);
                            string Subject = $"[UAT] PostCashReceipts Errors - {finalResult}";
                            string toAddress = MasterLookup._EI_EWSDetails.Support_CCEmailAddress;
                            string bccAddress = MasterLookup._EI_EWSDetails.Support_BCCEmailAddresses;
                            Dictionary<string, string> CIDImages = new Dictionary<string, string>();
                            string cidImageFolderPath = Path.Combine(_env.WebRootPath, "images");
                            CIDImages.Add("errorBanner", "banner-error-report-v4.jpg");
                            List<InLineAttachmentItem> inLineAttachmentItems = new List<InLineAttachmentItem>();
                            foreach (var img in CIDImages)
                            {
                                string imagePath = Path.Combine(cidImageFolderPath, img.Value);
                                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                                inLineAttachmentItems.Add(new InLineAttachmentItem()
                                {
                                    IsInline = true,
                                    BinaryImage = imageBytes,
                                    Name = img.Key,
                                    ContentID = img.Key
                                });
                            }
                            var viewDataDictionary = new ViewDataDictionary
                                                    (
                                                    new EmptyModelMetadataProvider(),
                                                         new ModelStateDictionary()
                                                    )
                            {
                                Model = postCashReceiptsResponse,
                            };
                            var htmlContent = this.view.Render("PostCashReceipts/ErrorReport", viewDataDictionary);
                            EWS_EmailUtility _EmailUtility = new EWS_EmailUtility();
                            _EmailUtility.SendExceptionReport(Subject, htmlContent, finalResult, null, toAddress, bccAddress, inLineAttachmentItems);
                        }
                    }
                }
            }

            return Ok("Successfully processed the cash receipts.");
        }
        [HttpPost]
        public IActionResult CorePayPayments([FromBody] JsonElement data)
        {
            string username = User.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(username))
            {
                var AuthenticatedUser = _usersRepository.GetUsers(User.Identity?.Name);
                var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);
                var incomingRequests = new IncomingRequests
                {
                    Service_Id = MasterLookup._Services.ePay,
                    RequestBody = data.ToString(),
                    CreatedBy = AuthenticatedUser.Username,
                };
                incomingRequests = _incomingRequestsRepository.InsertIncomingRequests(incomingRequests);
                if (Clients != null && Clients.PaymentService_Id == MasterLookup._Services.CorePay)
                {
                    var postCashReceiptsResponse = new PostCashReceiptsRes();
                    try
                    {
                        string rawJson = data.GetRawText();
                        var corePayPaymentRequest = JsonConvert.DeserializeObject<CorePayPaymentRequest>(rawJson);
                        if (corePayPaymentRequest != null)
                        {
                            if (!string.Equals(corePayPaymentRequest?.Event?.Type, "Payment.Success", StringComparison.OrdinalIgnoreCase))
                            {
                                return Ok(new
                                {
                                    StatusCode = 200,
                                    Message = $"Event {corePayPaymentRequest?.Event?.Type} is skipped in EI Accounting."
                                });
                            }
                            var entityTypes = MasterLookup._EntityTypes;
                            byte entityTypeId = corePayPaymentRequest?.Entity?
                                                .Select(e => e?.Type)
                                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                                .Select(t =>
                                                {
                                                    var prop = entityTypes.GetType()
                                                        .GetProperty(t, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                                                    return prop != null ? (byte?)prop.GetValue(entityTypes) : null;
                                                })
                                                .FirstOrDefault(x => x.HasValue) ?? entityTypes.Others;

                            string referenceId = corePayPaymentRequest?.Entity?
                                                 .FirstOrDefault(e =>
                                                 {
                                                     if (string.IsNullOrWhiteSpace(e?.Type)) return false;

                                                     var prop = entityTypes.GetType()
                                                         .GetProperty(e.Type, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                                                     return prop != null;
                                                 })?.ReferenceId ?? string.Empty;
                            var policyReferenceId = corePayPaymentRequest?.Entity?.FirstOrDefault(x => string.Equals(x?.Type, "POLICY", StringComparison.OrdinalIgnoreCase))?.ReferenceId ?? string.Empty;
                            if (entityTypeId != entityTypes.Insured && entityTypeId != entityTypes.Agency && entityTypeId != entityTypes.Mortgagee)
                            {
                                return BadRequest(new
                                {
                                    StatusCode = 400,
                                    Message = "Entity Type is missing or not recognized."
                                });
                            }

                            if (string.IsNullOrWhiteSpace(referenceId))
                            {
                                return BadRequest(new
                                {
                                    StatusCode = 400,
                                    Message = "ReferenceId is missing for the provided entity."
                                });
                            }

                            var lineItems = corePayPaymentRequest?.TransactionInfo?.LineItems ?? [];
                            var missingInvoices = lineItems
                                                  .Where(li => string.IsNullOrWhiteSpace(li?.Invoice?.InvoiceNumber))
                                                  .Select(li => li?.Name)
                                                  .ToList() ?? [];

                            if (missingInvoices?.Any() == true)
                            {
                                return BadRequest(new
                                {
                                    StatusCode = 400,
                                    Message = $"Invoice Number missing for line items [{string.Join(", ", missingInvoices)}]."
                                });
                            }

                            // Now validate each invoice number against repository
                            var invalidInvoices = new List<string>();

                            foreach (var li in lineItems)
                            {
                                var invNumber = li?.Invoice?.InvoiceNumber;
                                if (!string.IsNullOrWhiteSpace(invNumber))
                                {
                                    var invoicesData = _invoiceRepository.GetInvoicesByInvoiceNumber(invNumber, Clients.Client_Id, policyReferenceId);
                                    if (invoicesData == null)
                                    {
                                        invalidInvoices.Add(invNumber);
                                    }
                                }
                            }

                            if (invalidInvoices?.Any() == true)
                            {
                                return BadRequest(new
                                {
                                    StatusCode = 400,
                                    Message = $"The following Invoice numbers do not exist in the system: {string.Join(", ", invalidInvoices)}."
                                });
                            }



                            string transactionReference_Id = corePayPaymentRequest?.TransactionInfo?.TransactionId ?? string.Empty;
                            SqlTransaction transaction = null;
                            transaction = _baseRepository.GetNewTransaction();
                            double PaymentsRemainingAmount = 0;
                            decimal totalPaid = corePayPaymentRequest?.TransactionInfo?.LineItems?.Sum(li => li?.TotalAmount) ?? 0m;
                            decimal installmentFees = corePayPaymentRequest?.TransactionInfo?.LineItems?.SelectMany(li => li?.Charges ?? [])?.Where(c => c.Name == "Installment Fee")?.Sum(c => c?.Amount) ?? 0m;
                            byte PaymentType_Id = LookupUtility.GetAllPaymentTypes()?.FirstOrDefault(x => x.Name == corePayPaymentRequest?.TransactionInfo?.Method)?.PaymentType_Id ?? 0;
                            //decimal finalAmount = Clients.ExcludeInstallmentFee ? (totalPaid - installmentFees) : totalPaid;
                            double PaymentAmount = Convert.ToDouble(totalPaid - installmentFees);
                            var payments = new Payments
                            {
                                EntityType_Id = entityTypeId,
                                Entity_Id = CommonUtility.GetEntityIdByName(entityTypeId, referenceId, AuthenticatedUser.Client_Id), // From the request we are identifying the EntiyType and with that sending to common method which will get the EntityId from name but what if Insured EntityType have same name?
                                PaymentDate = corePayPaymentRequest?.Event?.Time ?? DateTime.Now, // There is no Date which refer to Payment Date so for now we kept the Event Time as Payment Date
                                Amount = PaymentAmount,
                                RemainingAmount = PaymentAmount,
                                PaymentType_Id = PaymentType_Id > 0 ? PaymentType_Id : MasterLookup._PaymentTypes.CreditCard, // For now we kept this as CreditCard need this info from Request Payload
                                ReferenceNumber = transactionReference_Id,
                                PaymentStatus_Id = MasterLookup._PaymentStatus.Completed,
                                Client_Id = AuthenticatedUser.Client_Id,
                                OperatingBank_Id = _clientServiceDefaultOperatingBankRepository.GetClientServiceDefaultOperatingBank(AuthenticatedUser.Client_Id, MasterLookup._ServiceProviderTypes.Payments, Clients.PaymentService_Id)?.OperatingBank_Id ?? 0,
                                PaymentDirection_Id = MasterLookup._PaymentDirections.Incoming,
                            };
                            payments = _paymentsRepository.InsertUpdatePayments(transaction, payments);
                            PaymentsRemainingAmount = payments.RemainingAmount;
                            long Payment_Id = 0;
                            if (payments?.Payment_Id > 0)
                            {
                                // clients.AccountingService_Id = 0;// Need to remove this line for Testing to avoid Journal Entry we are setting it as 0
                                Payment_Id = payments.Payment_Id;
                                var createJournalResponse = new CreateJournalRes();
                                if (Clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                                {
                                    var qBOUtility = new QBOUtility(AuthenticatedUser.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                    createJournalResponse = qBOUtility.CreatePaymentJournalEntry(transaction, payments);
                                    if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                                    {
                                        if (transaction.Connection != null)
                                        {
                                            transaction.Rollback();
                                        }
                                        var exceptionLogInfo = new ExceptionLogInfo
                                        {
                                            UserSessionId = AuthenticatedUser.User_Id,
                                            ErrorStackTrace = "FinancialServiceController/CorePayPayments/CreateJournalEntry",
                                            ErrorMessage = createJournalResponse?.Fault?.Error?[0]?.Message ?? string.Empty,
                                            ErrorName = "Create Payment Journal Entry Error",
                                            ErrorDescription = $"Error while creating a payment journal entry for transaction id:{transactionReference_Id}",
                                        };

                                        postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                        return Ok(new
                                        {
                                            StatusCode = 500,
                                            Message = $"Error while creating a payment journal entry for transaction id:{transactionReference_Id} ErrorMessage: {exceptionLogInfo?.ErrorMessage}"
                                        });
                                    }
                                }
                                else if (Clients?.AccountingService_Id == MasterLookup._Services.Sage)
                                {
                                    SageResponse createSageJournalResponse = new SageResponse();
                                    SageUtility sageUtility = new SageUtility(AuthenticatedUser.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                    var createJournalRes = Task.Run(() => sageUtility.CreatePaymentJournalEntry_Intacct(transaction, payments)).Result;

                                    var serializer = new XmlSerializer(typeof(SageResponse));
                                    using (var reader = new StringReader(createJournalRes))
                                    {
                                        createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                    }
                                    if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                    {
                                        if (transaction.Connection != null)
                                        {
                                            transaction.Rollback();
                                        }


                                        var xmlDoc = XDocument.Parse(createJournalRes);

                                        var status = xmlDoc.Descendants("status")
                                                           .FirstOrDefault(x => x.Parent?.Name == "result")
                                                           ?.Value;
                                        string errorMsg = string.Empty;
                                        if (status == "failure")
                                        {
                                            errorMsg = xmlDoc.Descendants("description2")
                                                                 .FirstOrDefault()?.Value;

                                        }
                                        var exceptionLogInfo = new ExceptionLogInfo
                                        {
                                            UserSessionId = AuthenticatedUser.User_Id,
                                            ErrorStackTrace = "FinancialServiceController/CorePayPayments/CreateJournalEntry",
                                            ErrorMessage = errorMsg ?? string.Empty,
                                            ErrorName = "Create Payment Journal Entry Error",
                                            ErrorDescription = $"Error while creating a payment journal entry for transaction id:{transactionReference_Id}",
                                        };

                                        postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                        return Ok(new
                                        {
                                            StatusCode = 500,
                                            Message = $"Error while creating a payment journal entry for transaction id:{transactionReference_Id} ErrorMessage: {exceptionLogInfo?.ErrorMessage}"
                                        });
                                    }
                                }
                            }
                            if (corePayPaymentRequest?.TransactionInfo?.LineItems?.Count() > 0)
                            {
                                var groupedInvoices = corePayPaymentRequest.TransactionInfo.LineItems.Where(li => !string.IsNullOrEmpty(li?.Invoice?.InvoiceNumber)).GroupBy(li => li.Invoice.InvoiceNumber);
                                foreach (var invoiceGroup in groupedInvoices)
                                {
                                    string invoiceNumber = invoiceGroup.Key;
                                    var PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
                                    var invoicesData = _invoiceRepository.GetInvoicesByInvoiceNumber(invoiceNumber, Clients.Client_Id, policyReferenceId);

                                    if (invoicesData == null || (invoicesData != null && invoicesData?.RemainingBalance == 0))
                                        continue;
                                    long client_Id = 0;
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(invoiceNumber))
                                        {
                                            if (invoicesData != null)
                                            {
                                                client_Id = invoicesData.Client_Id;
                                                long invoice_Id = invoicesData.Id;
                                                decimal totalForThisInvoice = 0;
                                                totalForThisInvoice = invoiceGroup.Sum(li => li?.TotalAmount ?? 0m);
                                                decimal invoiceLevelinstallmentFees = 0;
                                                invoiceLevelinstallmentFees = invoiceGroup.SelectMany(li => li?.Charges ?? new List<CorePayPaymentChargeDetails>()).Where(c => c?.Name == "Installment Fee").Sum(c => c?.Amount ?? 0m);
                                                totalForThisInvoice -= invoiceLevelinstallmentFees;
                                                double paidAmount = Convert.ToDouble(totalForThisInvoice);
                                                double RemainingBalance = 0;
                                                bool Paid = false;



                                                if (payments?.Payment_Id > 0)
                                                {
                                                    var paymentApplications = new PaymentApplications
                                                    {
                                                        Payment_Id = payments.Payment_Id,
                                                        Invoice_Id = invoice_Id,
                                                        AppliedDate = DateTime.Now,
                                                        TotalAppliedAmount = paidAmount,
                                                    };

                                                    paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(transaction, paymentApplications);
                                                    var invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoice_Id);
                                                    RemainingBalance = invoices.RemainingBalance - paidAmount;
                                                    Paid = RemainingBalance == 0;
                                                    _invoiceRepository.UpdateBalanceInInvoices(transaction, invoice_Id, RemainingBalance, Paid);
                                                    List<InvoiceDetails> invoiceDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(invoice_Id, MasterLookup._Directions.AR);
                                                    if (invoiceDetailsList?.Count() > 0)
                                                    {
                                                        invoiceDetailsList = invoiceDetailsList.Where(x => x.RemainingBalance != 0).ToList();
                                                        if (Paid)
                                                        {
                                                            foreach (var invoiceDetail in invoiceDetailsList)
                                                            {
                                                                var paymentApplicationLineItems = new PaymentApplicationLineItems
                                                                {
                                                                    PaymentApplication_Id = paymentApplications.PaymentApplication_Id,
                                                                    InvoiceLineItem_Id = invoiceDetail.Id,
                                                                    AppliedAmount = invoiceDetail.RemainingBalance,
                                                                    Is_SplitPayment = false,
                                                                };

                                                                _paymentsRepository.InsertUpdatePaymentApplicationLineItems(transaction, paymentApplicationLineItems);
                                                                _invoiceRepository.UpdateBalanceInInvoiceDetails(transaction, invoiceDetail.Id, 0, true);
                                                                PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                            }
                                                            PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - paidAmount, 2);
                                                            _paymentsRepository.UpdatePaymentRemainingAmount(transaction, payments.Payment_Id, PaymentsRemainingAmount);
                                                        }
                                                        else
                                                        {
                                                            double AllocatedRemainingAmount = paidAmount;
                                                            var LineTypeList = new List<int>
                                                            {
                                                                 MasterLookup._LineItemTypes.Tax,
                                                                 MasterLookup._LineItemTypes.Fee,
                                                                 MasterLookup._LineItemTypes.CarrierFee,
                                                                 MasterLookup._LineItemTypes.Commission,
                                                                 MasterLookup._LineItemTypes.Premium
                                                            };
                                                            foreach (var Type in LineTypeList)
                                                            {
                                                                var LOBGroupedLineItems = invoiceDetailsList.Where(x => x.LineItemType_Id == Type && x.RemainingBalance != 0).GroupBy(x => x.LOB_Id).ToList();
                                                                if (LOBGroupedLineItems?.Count() > 0 && AllocatedRemainingAmount != 0)
                                                                {
                                                                    foreach (var lobGroup in LOBGroupedLineItems)
                                                                    {
                                                                        var LOB_Id = lobGroup.Key;
                                                                        var LineItemListGroupPerLOB_Id = lobGroup.ToList();
                                                                        double AllocatedAmountPerItem = Math.Round(AllocatedRemainingAmount / LineItemListGroupPerLOB_Id.Count(), 2);
                                                                        foreach (var lineitem in LineItemListGroupPerLOB_Id)
                                                                        {
                                                                            if (AllocatedRemainingAmount != 0)
                                                                            {
                                                                                bool Is_HouseCommissionType = Type == MasterLookup._LineItemTypes.Commission && LineItemListGroupPerLOB_Id.Any(li =>
                                                                                {
                                                                                    var BillingType_LineItems_Directions = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == li.BillingType_LineItems_Directions_Id);
                                                                                    var LineItems_Directions = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingType_LineItems_Directions?.LineItems_Directions_Id);
                                                                                    var LineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItems_Directions?.LineItems_Clients_Id);
                                                                                    var LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(z => z.LineItem_Id == LineItems_Clients?.LineItem_Id);
                                                                                    if (BillingType_LineItems_Directions?.BillingType_Id == MasterLookup._BillingTypes.Agency)
                                                                                    {
                                                                                        return LineItems?.Name == MasterLookup._CustomLineitems.HouseCommission;
                                                                                    }
                                                                                    else if (BillingType_LineItems_Directions?.BillingType_Id == MasterLookup._BillingTypes.Insured)
                                                                                    {
                                                                                        return LineItems?.Name == MasterLookup._CustomLineitems.TotalCommission;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        return false;
                                                                                    }
                                                                                });
                                                                                if (Is_HouseCommissionType)
                                                                                {
                                                                                    double ActualPremium = 0;
                                                                                    double CarrierPremium = 0;
                                                                                    double HseCommissionAmount = 0;
                                                                                    foreach (var invoiceDetails in invoiceDetailsList)
                                                                                    {
                                                                                        if (invoiceDetails.LOB_Id == lineitem.LOB_Id)
                                                                                        {
                                                                                            LU_BillingType_LineItems_Directions _BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                                                                                            LU_BillingTypes _BillingTypes = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.BillingType_Id == _BillingLineItemsDirections?.BillingType_Id);
                                                                                            LU_LineItems_Directions _LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == _BillingLineItemsDirections?.LineItems_Directions_Id);
                                                                                            LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == _LineItemsDirections?.LineItems_Clients_Id);
                                                                                            LU_LineItems _LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id)!;
                                                                                            if (_BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Agency)
                                                                                            {
                                                                                                if (_LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && _LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                                                                                {
                                                                                                    CarrierPremium = invoiceDetails.Amount;
                                                                                                }
                                                                                                else if (_LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && _LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                                                                                {
                                                                                                    HseCommissionAmount = invoiceDetails.Amount;
                                                                                                }
                                                                                            }
                                                                                            else if (_BillingLineItemsDirections?.BillingType_Id == MasterLookup._BillingTypes.Insured)
                                                                                            {
                                                                                                if (_LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Premium && _LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                                                                                {
                                                                                                    CarrierPremium = invoiceDetails.Amount;
                                                                                                }
                                                                                                else if (_LineItems?.LineItemType_Id == MasterLookup._LineItemTypes.Commission && _LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                                                                                                {
                                                                                                    HseCommissionAmount = invoiceDetails.Amount;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    ActualPremium = CarrierPremium + HseCommissionAmount;
                                                                                    double HseCommPercentage = ActualPremium != 0 ? (HseCommissionAmount / ActualPremium) * 100 : 0;
                                                                                    double CalculatedHseCommAmount = Math.Round(((double)AllocatedAmountPerItem * HseCommPercentage / 100), 2);
                                                                                    double ActualApplyAmount = Math.Round(Math.Sign(lineitem.RemainingBalance) * Math.Min(Math.Abs(CalculatedHseCommAmount), Math.Abs(lineitem.RemainingBalance)), 2);
                                                                                    if (ActualApplyAmount != 0)
                                                                                    {
                                                                                        var paymentApplicationLineItems = new PaymentApplicationLineItems
                                                                                        {
                                                                                            PaymentApplication_Id = paymentApplications.PaymentApplication_Id,
                                                                                            InvoiceLineItem_Id = lineitem.Id,
                                                                                            AppliedAmount = ActualApplyAmount,
                                                                                            Is_SplitPayment = false,
                                                                                            SubType_Id = null,
                                                                                        };

                                                                                        _paymentsRepository.InsertUpdatePaymentApplicationLineItems(transaction, paymentApplicationLineItems);
                                                                                        RemainingBalance = Math.Round(lineitem.RemainingBalance - Convert.ToDouble(Math.Sign(lineitem.RemainingBalance) * Math.Abs(ActualApplyAmount)), 2);
                                                                                        Paid = RemainingBalance == 0;
                                                                                        _invoiceRepository.UpdateBalanceInInvoiceDetails(transaction, lineitem.Id, RemainingBalance, Paid);
                                                                                        PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                                                        AllocatedRemainingAmount = Math.Round(AllocatedRemainingAmount - ActualApplyAmount, 2);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    double _ApplyAmount = Math.Round(Math.Sign(lineitem.RemainingBalance) * Math.Min(Math.Abs(AllocatedAmountPerItem), Math.Abs(lineitem.RemainingBalance)), 2);
                                                                                    if (_ApplyAmount != 0)
                                                                                    {
                                                                                        var paymentApplicationLineItems = new PaymentApplicationLineItems
                                                                                        {
                                                                                            PaymentApplication_Id = paymentApplications.PaymentApplication_Id,
                                                                                            InvoiceLineItem_Id = lineitem.Id,
                                                                                            AppliedAmount = _ApplyAmount,
                                                                                            Is_SplitPayment = false,
                                                                                            SubType_Id = null,
                                                                                        };

                                                                                        _paymentsRepository.InsertUpdatePaymentApplicationLineItems(transaction, paymentApplicationLineItems);
                                                                                        RemainingBalance = Math.Round(lineitem.RemainingBalance - Convert.ToDouble(Math.Sign(lineitem.RemainingBalance) * Math.Abs(_ApplyAmount)), 2);
                                                                                        Paid = RemainingBalance == 0;
                                                                                        _invoiceRepository.UpdateBalanceInInvoiceDetails(transaction, lineitem.Id, RemainingBalance, Paid);
                                                                                        PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                                                        //AllocatedRemainingAmount -= _ApplyAmount;
                                                                                        AllocatedRemainingAmount = Math.Round(AllocatedRemainingAmount - _ApplyAmount, 2);
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - paidAmount, 2);
                                                            _paymentsRepository.UpdatePaymentRemainingAmount(transaction, payments.Payment_Id, PaymentsRemainingAmount);
                                                        }
                                                    }
                                                }

                                                if (PaymentApplicationLineItemsList.Count() > 0)
                                                {
                                                    JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntryWithTransaction(transaction, MasterLookup._Sources.Payment, payments.Payment_Id);

                                                    if (Clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                                                    {
                                                        QBOUtility qBOUtility = new QBOUtility(client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                                        CreateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(transaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
                                                        if (!(updateJournalRes != null && updateJournalRes.JournalEntry?.Id != null))
                                                        {
                                                            if (transaction.Connection != null)
                                                            {
                                                                transaction.Rollback();
                                                            }
                                                            qBOUtility.DeleteJournalEntry(journalEntries);
                                                            var exceptionLogInfo = new ExceptionLogInfo
                                                            {
                                                                UserSessionId = AuthenticatedUser.User_Id,
                                                                ErrorStackTrace = "FinancialServiceController/CorePayPayments/UpdateJournalEntry",
                                                                ErrorMessage = updateJournalRes?.Fault?.Error?[0]?.Message ?? string.Empty,
                                                                ErrorName = "Update Payment Journal Entry Error",
                                                                ErrorDescription = $"Error while updating payment journal entry for transaction id:{transactionReference_Id}",
                                                            };

                                                            postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                            _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                        }
                                                    }
                                                    else if (Clients?.AccountingService_Id == MasterLookup._Services.Sage)
                                                    {
                                                        SageResponse createSageJournalResponse = new SageResponse();
                                                        SageUtility sageUtility = new SageUtility(client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                                        var createJournalRes = Task.Run(() => sageUtility.CreateJournalEntry_Intacct(transaction, payments, PaymentApplicationLineItemsList, journalEntries, true)).Result;

                                                        var serializer = new XmlSerializer(typeof(SageResponse));
                                                        using (var reader = new StringReader(createJournalRes))
                                                        {
                                                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                                        }
                                                        if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                                        {
                                                            if (transaction.Connection != null)
                                                            {
                                                                transaction.Rollback();
                                                            }


                                                            var xmlDoc = XDocument.Parse(createJournalRes);

                                                            var status = xmlDoc.Descendants("status")
                                                                               .FirstOrDefault(x => x.Parent?.Name == "result")
                                                                               ?.Value;
                                                            string errorMsg = string.Empty;
                                                            if (status == "failure")
                                                            {
                                                                errorMsg = xmlDoc.Descendants("description2")
                                                                                     .FirstOrDefault()?.Value;

                                                            }
                                                            var exceptionLogInfo = new ExceptionLogInfo
                                                            {
                                                                UserSessionId = AuthenticatedUser.User_Id,
                                                                ErrorStackTrace = "FinancialServiceController/CorePayPayments/CreateJournalEntry",
                                                                ErrorMessage = errorMsg ?? string.Empty,
                                                                ErrorName = "Create Payment Journal Entry Error",
                                                                ErrorDescription = $"Error while creating a payment journal entry for transaction id:{transactionReference_Id}, InvoiceNumber: {invoiceNumber}",
                                                            };

                                                            postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                                            _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        var exceptionLogInfo = new ExceptionLogInfo
                                        {
                                            UserSessionId = AuthenticatedUser.User_Id,
                                            ErrorStackTrace = ex.StackTrace ?? string.Empty,
                                            ErrorMessage = ex.Message ?? string.Empty,
                                            ErrorName = "Payment processing Error",
                                            ErrorDescription = $"Error while processing a payment for transaction id:{transactionReference_Id} InvoiceNumber: {invoiceNumber}",
                                        };

                                        postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                                        JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntryWithTransaction(transaction, MasterLookup._Sources.Payment, Payment_Id);
                                        if (journalEntries != null)
                                        {
                                            QBOUtility qBOUtility = new QBOUtility(client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                            qBOUtility.DeleteJournalEntry(journalEntries);
                                        }

                                        if (transaction.Connection != null)
                                        {
                                            transaction.Rollback();
                                        }
                                    }
                                }
                            }
                            if (transaction?.Connection != null)
                            {
                                transaction.Commit();
                            }
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                StatusCode = 400,
                                Message = "CorePay Payment data is missing or empty."
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        var exceptionLogInfo = new ExceptionLogInfo
                        {
                            UserSessionId = AuthenticatedUser.User_Id,
                            ErrorStackTrace = ex?.StackTrace ?? string.Empty,
                            ErrorMessage = ex?.Message ?? string.Empty,
                            ErrorName = "Top - Level Error",
                            ErrorDescription = "An error occurred while processing the CorePay cash receipts.'",
                        };
                        postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                    }
                    finally
                    {
                        bool hasErrors = postCashReceiptsResponse?.Errors?.Count() > 0;
                        if (hasErrors)
                        {
                            string finalResult = Clients.CompanyName;
                            string Subject = $"[UAT] CorePay Post Payment Errors - {finalResult}";
                            string toAddress = MasterLookup._EI_EWSDetails.Support_CCEmailAddress;
                            string bccAddress = MasterLookup._EI_EWSDetails.Support_BCCEmailAddresses;
                            Dictionary<string, string> CIDImages = new Dictionary<string, string>();
                            string cidImageFolderPath = Path.Combine(_env.WebRootPath, "images");
                            CIDImages.Add("errorBanner", "banner-error-report-v4.jpg");
                            List<InLineAttachmentItem> inLineAttachmentItems = new List<InLineAttachmentItem>();
                            foreach (var img in CIDImages)
                            {
                                string imagePath = Path.Combine(cidImageFolderPath, img.Value);
                                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                                inLineAttachmentItems.Add(new InLineAttachmentItem()
                                {
                                    IsInline = true,
                                    BinaryImage = imageBytes,
                                    Name = img.Key,
                                    ContentID = img.Key
                                });
                            }
                            var viewDataDictionary = new ViewDataDictionary
                                                    (
                                                    new EmptyModelMetadataProvider(),
                                                         new ModelStateDictionary()
                                                    )
                            {
                                Model = postCashReceiptsResponse,
                            };
                            var htmlContent = this.view.Render("PostCashReceipts/ErrorReport", viewDataDictionary);
                            EWS_EmailUtility _EmailUtility = new EWS_EmailUtility();
                            _EmailUtility.SendExceptionReport(Subject, htmlContent, finalResult, null, toAddress, bccAddress, inLineAttachmentItems);
                        }
                    }
                }
                else
                {
                    return Unauthorized(new
                    {
                        StatusCode = 401,
                        Message = "Client is not authorized for CorePay service."
                    });
                }
            }
            else
            {
                return Unauthorized(new
                {
                    StatusCode = 401,
                    Message = "Username not found in the provided token."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Successfully processed the CorePay Payments."
            });
            // return Ok("Successfully processed the CorePay Payments.");
        }
        [HttpPost]
        public IActionResult ApplyPaymentToSubmissionInvoices([FromBody] JsonElement data)
        {
            string username = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var AuthenticatedUser = _usersRepository.GetUsers(User.Identity?.Name);
                if (AuthenticatedUser == null)
                {
                    return Unauthorized(new
                    {
                        Status = false,
                        Message = "Authenticated user could not be found."
                    });
                }

                var incomingRequests = new IncomingRequests
                {
                    Service_Id = MasterLookup._Services.ApplySubmissionPayment,
                    RequestBody = data.ToString(),
                    CreatedBy = AuthenticatedUser.Username,
                };
                incomingRequests = _incomingRequestsRepository.InsertIncomingRequests(incomingRequests);

                var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);
                if (Clients == null)
                {
                    return Unauthorized(new
                    {
                        Status = false,
                        Message = "Client is not authorized for Applying Payment to Submission Invoices."
                    });
                }

                var postCashReceiptsResponse = new PostCashReceiptsRes();
                long submission_Id = 0;
                try
                {
                    string rawJson = data.GetRawText();
                    var processAccountingPaymentRequest = JsonConvert.DeserializeObject<ProcessAccountingPaymentRequest>(rawJson);
                    if (processAccountingPaymentRequest != null)
                    {
                        var errors = new List<string>();
                        submission_Id = processAccountingPaymentRequest?.Submission_Id ?? 0;
                        long quote_Id = processAccountingPaymentRequest?.Quote_Id ?? 0;
                        string policyNumber = processAccountingPaymentRequest?.PolicyNumber ?? string.Empty;
                        double totalAmount = processAccountingPaymentRequest?.TotalAmount ?? 0;
                        double totalPaidAmount = processAccountingPaymentRequest?.TotalPaidAmount ?? 0;

                        if (submission_Id <= 0)
                        {
                            errors.Add("Please send a valid Submission_Id.");
                        }
                        if (quote_Id <= 0)
                        {
                            errors.Add("Please send a valid Quote_Id.");
                        }
                        if (string.IsNullOrWhiteSpace(policyNumber))
                        {
                            errors.Add("Please send a valid PolicyNumber.");
                        }
                        if (totalAmount <= 0)
                        {
                            errors.Add("TotalAmount must be greater than zero.");
                        }
                        if (totalPaidAmount <= 0)
                        {
                            errors.Add("TotalPaidAmount must be greater than zero.");
                        }
                        if (totalPaidAmount > totalAmount)
                        {
                            errors.Add("TotalPaidAmount cannot be greater than TotalAmount.");
                        }

                        if (errors.Any())
                        {
                            return BadRequest(new
                            {
                                Status = false,
                                Message = "Validation failed.",
                                Data = errors
                            });
                        }

                        var policyDetails = _policyDetailsRepository.GetPolicyDetailsBySubmission_Id(submission_Id, Clients.Client_Id);
                        if (policyDetails == null || policyDetails.Id <= 0)
                        {
                            return BadRequest(new
                            {
                                Status = false,
                                Message = $"Submission with Submission_Id '{submission_Id}' does not exist in the system."
                            });
                        }

                        var policyInvoices = _invoiceRepository.GetAllInvoicesByPolicyDetail_Id(policyDetails.Id);
                        if (policyInvoices == null || !policyInvoices.Any())
                        {
                            return BadRequest(new
                            {
                                Status = false,
                                Message = $"No invoices found for Submission_Id '{submission_Id}'."
                            });
                        }

                        var appliedInvoiceDetails = new List<SelectedInvoiceDetailDTO>();
                        foreach (var invoice in policyInvoices)
                        {
                            var invoiceDetails = _invoiceRepository.GetInvoiceDetails(invoice.Id);
                            if (invoiceDetails == null || !invoiceDetails.Any())
                            {
                                continue;
                            }
                            foreach (var invoiceDetail in invoiceDetails)
                            {
                                var billingTypeLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetail.BillingType_LineItems_Directions_Id);
                                var lineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == billingTypeLineItemsDirections?.LineItems_Directions_Id);
                                if (lineItemsDirections?.Direction_Id == 1 && invoiceDetail?.RemainingBalance != 0)
                                {
                                    appliedInvoiceDetails.Add(new SelectedInvoiceDetailDTO
                                    {
                                        InvoiceDetail_Id = invoiceDetail.Id,
                                        Invoice_Id = invoice.Id,
                                        Amount = invoiceDetail.Amount,
                                        RemainingBalance = invoiceDetail.RemainingBalance,
                                        IsPartial = true,
                                    });
                                }
                            }
                        }

                        if (appliedInvoiceDetails == null || !appliedInvoiceDetails.Any())
                        {
                            return BadRequest(new
                            {
                                Status = false,
                                Message = $"Payment applicable invoice details not found for Submission_Id '{submission_Id}'."
                            });
                        }

                        var groupedAppliedInvoiceDetails = appliedInvoiceDetails
                                                           .GroupBy(x => x.Invoice_Id)
                                                           .Select(g => new SelectedInvoiceDetailDTO
                                                           {
                                                               Invoice_Id = g.Key,
                                                               InvoiceDetail_Id = 0,
                                                               InvoiceDetail_Ids = g.Select(x => x.InvoiceDetail_Id!).ToList(),
                                                               Amount = g.Sum(x => x.Amount),
                                                               RemainingBalance = g.Sum(x => x.RemainingBalance),
                                                               IsPartial = true
                                                           })
                                                           .OrderBy(x => x.Invoice_Id)
                                                           .ToList();

                        double paymentToApply = totalPaidAmount;
                        for (int i = 0; i < groupedAppliedInvoiceDetails.Count; i++)
                        {
                            var invoice = groupedAppliedInvoiceDetails[i];
                            if (paymentToApply <= 0)
                            {
                                invoice.Amount = 0;
                                invoice.IsPartial = false;
                                continue;
                            }

                            double invoiceRemainingBalance = invoice.RemainingBalance;
                            bool isLastInvoice = i == groupedAppliedInvoiceDetails.Count - 1;
                            if (paymentToApply < invoiceRemainingBalance)
                            {
                                // Partial payment
                                invoice.Amount = paymentToApply;
                                invoice.RemainingBalance = invoiceRemainingBalance;
                                invoice.IsPartial = true;
                                paymentToApply = 0;
                            }
                            else if (paymentToApply == invoiceRemainingBalance)
                            {
                                // Fully paid
                                invoice.Amount = invoiceRemainingBalance;
                                invoice.RemainingBalance = invoiceRemainingBalance;
                                invoice.IsPartial = false;
                                paymentToApply = 0;
                            }
                            else if (isLastInvoice)
                            {
                                // Overpayment on the last invoice
                                invoice.Amount = paymentToApply;
                                invoice.RemainingBalance = invoiceRemainingBalance;
                                invoice.IsPartial = false;
                                paymentToApply = 0;
                            }
                            else
                            {
                                // Fully pay current invoice and continue
                                invoice.Amount = invoiceRemainingBalance;
                                invoice.RemainingBalance = invoiceRemainingBalance;
                                invoice.IsPartial = false;
                                paymentToApply -= invoiceRemainingBalance;
                            }
                        }

                        var createProcessAccountingPayment = CreateProcessAccountingPayment(policyDetails, Clients, AuthenticatedUser, totalPaidAmount);
                        if (createProcessAccountingPayment == null || createProcessAccountingPayment?.Payment_Id <= 0)
                        {
                            return BadRequest(new
                            {
                                Status = false,
                                Message = $"Unable to create the payment process for Submission_Id '{submission_Id}'."
                            });
                        }

                        string postJson = JsonConvert.SerializeObject(groupedAppliedInvoiceDetails);
                        var paymentApplicationLineItemsList = _paymentsRepository.ApplyPayments(Clients.Client_Id, postJson, createProcessAccountingPayment.Payment_Id, MasterLookup._Directions.AR, false);
                        if (!paymentApplicationLineItemsList.Any())
                        {
                            return BadRequest(new
                            {
                                Status = false,
                                Message = $"No payment application details were generated for Submission_Id '{submission_Id}' and Payment_Id '{createProcessAccountingPayment.Payment_Id}'."
                            });
                        }
                        string paymentApplicationLineitemJson = JsonConvert.SerializeObject(paymentApplicationLineItemsList.Select(x => new { PaymentApplicationLineItem_Id = x.PaymentApplicationLineItem_Id }).ToList());
                        SqlTransaction PaymentsTransaction = _baseRepository.GetNewTransaction();
                        try
                        {
                            if (paymentApplicationLineItemsList.Count() > 0)
                            {
                                List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, createProcessAccountingPayment.Payment_Id);
                                JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();
                                if (Clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                {
                                    QBOUtility qBOUtility = new QBOUtility(AuthenticatedUser.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                    CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, createProcessAccountingPayment, paymentApplicationLineItemsList, journalEntry, true);
                                    if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                    {
                                        PaymentsTransaction.Commit();
                                        return Ok(new
                                        {
                                            Status = true,
                                            Message = $"Successfully applied Payment (Payment ID: {createProcessAccountingPayment.Payment_Id}).",
                                        });
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }

                                        string Result = _paymentsRepository.UnApplyPayments(Clients.Client_Id, MasterLookup._Directions.AR, paymentApplicationLineitemJson);
                                        return BadRequest(new
                                        {
                                            Status = false,
                                            Message = createJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {createJournalRes.Fault.Error[0].Detail}" : $"Unable to sync to Intuit! {Result}",
                                        });
                                    }
                                }
                                else if (Clients.AccountingService_Id == MasterLookup._Services.Sage)
                                {
                                    SageUtility sageUtility = new SageUtility(AuthenticatedUser.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, Clients.AccountingType_Id);
                                    var createJournalRes = Task.Run(() =>
                                    sageUtility.CreateJournalEntry_Intacct(PaymentsTransaction, createProcessAccountingPayment, paymentApplicationLineItemsList, journalEntry, true)).Result;
                                    var serializer = new XmlSerializer(typeof(SageResponse));
                                    SageResponse createSageJournalResponse = new SageResponse();
                                    using (var reader = new StringReader(createJournalRes))
                                    {
                                        createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                    }
                                    if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
                                    {
                                        PaymentsTransaction.Commit();
                                        return Ok(new
                                        {
                                            Status = true,
                                            Message = $"Successfully applied Payment (Payment ID: {createProcessAccountingPayment.Payment_Id}).",
                                        });
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        string Result = _paymentsRepository.UnApplyPayments(Clients.Client_Id, MasterLookup._Directions.AR, paymentApplicationLineitemJson);
                                        return BadRequest(new
                                        {
                                            Status = false,
                                            Message = $"Unable to sync to Sage. {Result}",
                                        });
                                    }
                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (PaymentsTransaction.Connection != null)
                            {
                                PaymentsTransaction.Rollback();
                            }

                            var exceptionLogInfo = new ExceptionLogInfo
                            {
                                UserSessionId = AuthenticatedUser.User_Id,
                                ErrorStackTrace = ex.StackTrace ?? string.Empty,
                                ErrorMessage = ex.Message ?? string.Empty,
                                ErrorName = "Payment processing Error",
                                ErrorDescription = $"Error while processing a payment for Submission Id:{submission_Id} PolicyNumber: {policyNumber}",
                            };

                            _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                            string Result = _paymentsRepository.UnApplyPayments(Clients.Client_Id, MasterLookup._Directions.AR, paymentApplicationLineitemJson);
                            return StatusCode(500, new
                            {
                                Status = false,
                                Message = $"Error while processing a payment for Submission Id:{submission_Id} PolicyNumber: {policyNumber}"
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            Status = false,
                            Message = "Submission Payment data is missing or empty."
                        });
                    }
                }
                catch (Exception ex)
                {
                    var exceptionLogInfo = new ExceptionLogInfo
                    {
                        UserSessionId = AuthenticatedUser.User_Id,
                        ErrorStackTrace = ex?.StackTrace ?? string.Empty,
                        ErrorMessage = ex?.Message ?? string.Empty,
                        ErrorName = "Top - Level Error",
                        ErrorDescription = "An error occurred while processing the Payments to Submission Invoices.'",
                    };
                    postCashReceiptsResponse.Errors = PreparePostCashReceiptErrorLog(exceptionLogInfo, postCashReceiptsResponse.Errors);
                    _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                    return StatusCode(500, new
                    {
                        Status = false,
                        Message = $"An error occurred while processing the payment for Submission_Id '{submission_Id}'."
                    });
                }
                finally
                {
                    bool hasErrors = postCashReceiptsResponse?.Errors?.Count() > 0;
                    hasErrors = false;
                    if (hasErrors)
                    {
                        string finalResult = Clients.CompanyName;
                        string Subject = $"[UAT] Submission Post Payment Errors - {finalResult}";
                        string toAddress = MasterLookup._EI_EWSDetails.Support_CCEmailAddress;
                        string bccAddress = MasterLookup._EI_EWSDetails.Support_BCCEmailAddresses;
                        Dictionary<string, string> CIDImages = new Dictionary<string, string>();
                        string cidImageFolderPath = Path.Combine(_env.WebRootPath, "images");
                        CIDImages.Add("errorBanner", "banner-error-report-v4.jpg");
                        List<InLineAttachmentItem> inLineAttachmentItems = new List<InLineAttachmentItem>();
                        foreach (var img in CIDImages)
                        {
                            string imagePath = Path.Combine(cidImageFolderPath, img.Value);
                            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                            inLineAttachmentItems.Add(new InLineAttachmentItem()
                            {
                                IsInline = true,
                                BinaryImage = imageBytes,
                                Name = img.Key,
                                ContentID = img.Key
                            });
                        }

                        var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                        {
                            Model = postCashReceiptsResponse,
                        };
                        var htmlContent = this.view.Render("PostCashReceipts/ErrorReport", viewDataDictionary);
                        EWS_EmailUtility _EmailUtility = new EWS_EmailUtility();
                        _EmailUtility.SendExceptionReport(Subject, htmlContent, finalResult, null, toAddress, bccAddress, inLineAttachmentItems);
                    }
                }
            }
            else
            {
                return Unauthorized(new
                {
                    Status = false,
                    Message = "Username not found in the provided token."
                });
            }

            return Ok(new
            {
                Status = 200,
                Message = "Successfully processed the Accounting Payments."
            });
        }
        [HttpPost]
        [Route("/api/v2/GetOutstandingInvoices")]
        public IActionResult GetOutstandingInvoices([FromBody] JsonElement data)
        {
            var coverPayOutstandingInvoicesResponse = new CoverPayOutstandingInvoicesResponse();
            string username = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var AuthenticatedUser = _usersRepository.GetUsers(User.Identity?.Name);
                var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);
                if (Clients != null && (Clients.PaymentService_Id == MasterLookup._Services.ePay || Clients.PaymentService_Id == MasterLookup._Services.CorePay))
                {
                    try
                    {
                        IncomingRequests incomingRequests = new IncomingRequests();
                        incomingRequests.Service_Id = MasterLookup._Services.CorePay;
                        incomingRequests.RequestBody = data.ToString();
                        incomingRequests.CreatedBy = username;
                        incomingRequests = _incomingRequestsRepository.InsertIncomingRequests(incomingRequests);

                        string rawJson = data.GetRawText();
                        var coverPayOutstandingInvoicesRequest = JsonConvert.DeserializeObject<CoverPayOutstandingInvoicesRequest>(rawJson);
                        if (coverPayOutstandingInvoicesRequest == null)
                        {
                            coverPayOutstandingInvoicesResponse.Status = "FAILED";
                            coverPayOutstandingInvoicesResponse.Messages = new List<string> { "Invalid Request." };
                            coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                            {
                                RequestId = Guid.NewGuid().ToString(),
                                SchemaVersion = "1.0",
                                Source = new CoverPaySource
                                {
                                    SourceSystemCode = "EIACCOUNTING",
                                    ReferenceId = Guid.NewGuid().ToString()
                                },
                                BillingAccounts = null,
                            };
                        }
                        var accountNumber = coverPayOutstandingInvoicesRequest?.Entities?.FirstOrDefault(x => string.Equals(x.Type, "ACCOUNTNUMBER", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
                        long entity_Id = 0;
                        byte entityType_Id = 0;
                        if (string.IsNullOrEmpty(accountNumber) && string.IsNullOrEmpty(coverPayOutstandingInvoicesRequest.Query))
                        {
                            coverPayOutstandingInvoicesResponse.Status = "FAILED";
                            coverPayOutstandingInvoicesResponse.Messages = new List<string> { "Either AccountNumber or Query is required." };
                            coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                            {
                                RequestId = Guid.NewGuid().ToString(),
                                SchemaVersion = "1.0",
                                Source = new CoverPaySource
                                {
                                    SourceSystemCode = "EIACCOUNTING",
                                    ReferenceId = Guid.NewGuid().ToString()
                                },
                                BillingAccounts = null,
                            };
                        }
                        if (!string.IsNullOrEmpty(accountNumber))
                        {
                            ExternalAccountInformation externalAccountInformation = _externalAccountInformationRepository.GetExternalAccountInformationByAccountNumber(accountNumber, Clients.Client_Id);
                            if (externalAccountInformation != null)
                            {
                                if (externalAccountInformation?.Entity_Id > 0 && externalAccountInformation?.EntityType_Id > 0)
                                {
                                    EntityInfo entityInfo = _policyDetailsRepository.GetEntityInfo(Clients.Client_Id, externalAccountInformation.Entity_Id, externalAccountInformation.EntityType_Id);
                                    if (entityInfo != null)
                                    {
                                        entity_Id = externalAccountInformation.Entity_Id;
                                        entityType_Id = externalAccountInformation.EntityType_Id;
                                    }
                                }
                            }
                        }

                        var outstandingPolicyDetails = coverPayService.GetOutstandingPolicyDetails(Clients.Client_Id, entity_Id, entityType_Id, coverPayOutstandingInvoicesRequest?.Query ?? string.Empty);
                        if (outstandingPolicyDetails?.Count > 0)
                        {
                            string policyDetails_Id_Json = JsonConvert.SerializeObject(outstandingPolicyDetails.Select(x => x.PolicyDetails_Id).ToList());
                            var outstandingInvoiceDetails = coverPayService.GetOutstandingInvoiceDetails(Clients.Client_Id, policyDetails_Id_Json);
                            if (outstandingInvoiceDetails?.InvoiceDetails?.Count > 0)
                            {
                                var billingAccounts = new List<CoverPayBillingAccount>();
                                foreach (var policy in outstandingPolicyDetails)
                                {
                                    // Get all invoices belonging to this policy
                                    var policyInvoices = outstandingInvoiceDetails.InvoiceDetails.Where(x => x.PolicyDetails_Id == policy.PolicyDetails_Id).ToList();
                                    if (!policyInvoices.Any())
                                    {
                                        continue;
                                    }

                                    var billingAccount = new CoverPayBillingAccount
                                    {
                                        ExternalReferenceId = policy.PolicyDetails_Id.ToString(),
                                        Currency = policy.Currenty,
                                        BillingType = policy.BillType,
                                        Payer = new CoverPayPayer
                                        {
                                            ExternalReferenceId = policy.Payer_ExternalReference_Id,
                                            Name = policy.Payer_Name,
                                            EmailAddress = policy.Payer_Email
                                        },

                                        Customer = new CoverPayCustomer
                                        {
                                            ExternalReferenceId = policy.Customer_ExternalReference_Id,
                                            FirstName = policy.Customer_FirstName,
                                            LastName = policy.Customer_LastName,
                                            Email = policy.Customer_Email,
                                            PhoneNumber = policy.Customer_Phone,
                                            Addresses = new List<CoverPayAddress>
                                            {
                                                new CoverPayAddress
                                                {
                                                    AddressType = policy.Customer_AddressType,
                                                    Address1 = policy.Customer_Adress1,
                                                    Address2 = policy.Customer_Adress2,
                                                    City = policy.Customer_City,
                                                    State = policy.Customer_State,
                                                    PostalCode = policy.Customer_PostalCode,
                                                    Country = policy.Customer_Country
                                                }
                                            }
                                        },

                                        Policy = new CoverPayPolicy
                                        {
                                            ExternalReferenceId = policy.Policy_ExternalReference_Id,
                                            PolicyNumber = policy.Policy_PolicyNumber,
                                            InsuredName = policy.Policy_InsuredName,
                                            PolicyType = policy.Policy_PolicyType,
                                            LineOfBusiness = new CoverPayLineOfBusiness { Code = policy?.Policy_LineOfBusiness_Code ?? string.Empty, Name = policy?.Policy_LineOfBusiness_Name ?? string.Empty},
                                            EffectiveDate = policy.Policy_EffectiveDate,
                                            ExpirationDate = policy.Policy_ExpirationDate
                                        },

                                        Contacts = new List<CoverPayContact>
                                        {
                                            new CoverPayContact
                                            {
                                                ContactType = policy.Contacts_Agency_ContactType,
                                                ExternalReferenceId = policy.Contacts_Agency_ExternalReference_Id,
                                                CompanyName = policy.Contacts_Agency_CompanyName,
                                                CompanyNAIC = policy.Contacts_Agency_NAIC,
                                                Addresses = new List<CoverPayAddress>
                                                {
                                                    new CoverPayAddress
                                                    {
                                                        Address1 = policy.Contacts_Agency_Addresses_Adress1,
                                                        Address2 = policy.Contacts_Agency_Addresses_Adress2,
                                                        City = policy.Contacts_Agency_Addresses_City,
                                                        State = policy.Contacts_Agency_Addresses_State,
                                                        PostalCode = policy.Contacts_Agency_Addresses_PostalCode,
                                                        Country = policy.Contacts_Agency_Addresses_Country
                                                    }
                                                }
                                            },
                                            new CoverPayContact
                                            {
                                                ContactType = policy.Contacts_Insured_ContactType,
                                                ExternalReferenceId = policy.Contacts_Insured_ExternalReference_Id,
                                                CompanyName = policy.Contacts_Insured_CompanyName,
                                                CompanyNAIC = policy.Contacts_Insured_NAIC,
                                                Addresses = new List<CoverPayAddress>
                                                {
                                                    new CoverPayAddress
                                                    {
                                                        Address1 = policy.Contacts_Insured_Addresses_Adress1,
                                                        Address2 = policy.Contacts_Insured_Addresses_Adress2,
                                                        City = policy.Contacts_Insured_Addresses_City,
                                                        State = policy.Contacts_Insured_Addresses_State,
                                                        PostalCode = policy.Contacts_Insured_Addresses_PostalCode,
                                                        Country = policy.Contacts_Insured_Addresses_Country
                                                    }
                                                }
                                            }
                                        },

                                        Invoices = new List<CoverPayInvoice>(),
                                        PaymentOptions = new CoverPayPaymentOptions
                                        {
                                            AllowPartialPayment = false,
                                            PaymentMethods = new List<CoverPayPaymentMethod>()
                                        },
                                        Entities = new List<CoverPayEntity>()
                                    };

                                    // Map invoices for this policy
                                    foreach (var invoice in policyInvoices)
                                    {
                                        var premiumDetails = outstandingInvoiceDetails.PremiumDetails.Where(x => x.PolicyDetails_Id == policy.PolicyDetails_Id && x.Invoice_Id == invoice.Invoice_Id).ToList();
                                        var taxFeeDetails = outstandingInvoiceDetails.TaxFeeDetails.Where(x => x.PolicyDetails_Id == policy.PolicyDetails_Id && x.Invoice_Id == invoice.Invoice_Id).ToList();
                                        var coverPayInvoice = new CoverPayInvoice
                                        {
                                            ExternalReferenceId = invoice.Invoice_Id.ToString(),
                                            InvoiceNumber = invoice.InvoiceNumber,
                                            Dates = new CoverPayInvoiceDates
                                            {
                                                InvoiceDate = invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                                                DueDate = invoice.DueDate.ToString("yyyy-MM-dd")
                                            },

                                            Amount = Convert.ToDouble(invoice.Amount),
                                            PaidAmount = Convert.ToDouble(invoice.PaidAmount),
                                            OutstandingAmount = Convert.ToDouble(invoice.OutstandingAmount),
                                            PaymentEligibility = new CoverPayPaymentEligibility
                                            {
                                                Payable = invoice.OutstandingAmount > 0,
                                                Reason = invoice.OutstandingAmount > 0 ? string.Empty : "Invoice has no outstanding amount"
                                            },

                                            PaymentSchedule = new List<CoverPayPaymentSchedule>()
                                        };

                                        // Create payment schedule for invoice
                                        var paymentSchedule = new CoverPayPaymentSchedule
                                        {
                                            ExternalReferenceId = invoice.Invoice_Id.ToString(),
                                            InstallmentNumber = invoice.InstallmentNumber,
                                            Type = invoice.Type,
                                            DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
                                            Amount = Convert.ToDouble(invoice.Amount),
                                            PaidAmount = Convert.ToDouble(invoice.PaidAmount),
                                            OutstandingAmount = Convert.ToDouble(invoice.OutstandingAmount),
                                            LineItems = new List<CoverPayLineItem>()
                                        };

                                        // Premium
                                        foreach (var premium in premiumDetails)
                                        {
                                            var premiumLineItem = new CoverPayLineItem
                                            {
                                                ExternalReferenceId = premium.Invoice_Id.ToString(),
                                                Type = premium.Type,
                                                Description = premium.Description,
                                                OutstandingAmount = Convert.ToDouble(premium.OutstandingAmount),
                                                TotalOutstandingAmount = Convert.ToDouble(invoice.OutstandingAmount),
                                                Charges = new List<CoverPayCharge>()
                                            };

                                            // Add Tax/Fee as charges under the premium
                                            var invoiceTaxFees = taxFeeDetails;
                                            foreach (var taxFee in invoiceTaxFees)
                                            {
                                                premiumLineItem.Charges.Add(new CoverPayCharge
                                                {
                                                    ExternalReferenceId = $"{taxFee.Invoice_Id}_{taxFee.Name}",
                                                    Type = taxFee.Type,
                                                    Name = taxFee.Name,
                                                    OutstandingAmount = Convert.ToDouble(taxFee.OutstandingAmount)
                                                });
                                            }
                                            paymentSchedule.LineItems.Add(premiumLineItem);
                                        }
                                        coverPayInvoice.PaymentSchedule.Add(paymentSchedule);
                                        billingAccount.Invoices.Add(coverPayInvoice);
                                    }
                                    var entityType = policy.BillType;
                                    if (!string.IsNullOrWhiteSpace(coverPayOutstandingInvoicesRequest.Query))
                                    {
                                        if (!string.IsNullOrWhiteSpace(entityType))
                                        {
                                            billingAccount.Entities.Add(new CoverPayEntity
                                            {
                                                Type = entityType,
                                                Value = policy?.Payer_Name ?? string.Empty
                                            });

                                            billingAccount.Entities.Add(new CoverPayEntity
                                            {
                                                Type = "AccountNumber",
                                                Value = policy?.External_AccountNumber ?? string.Empty
                                            });

                                            billingAccount.Entities.Add(new CoverPayEntity
                                            {
                                                Type = "PostalCode",
                                                Value = policy?.External_PostalCode ?? string.Empty
                                            });
                                        }
                                    }
                                    else
                                    {
                                        billingAccount.Entities = new List<CoverPayEntity>(coverPayOutstandingInvoicesRequest.Entities
                                                .Select(x => new CoverPayEntity
                                                {
                                                    Type = x.Type,
                                                    Value = x.Value
                                                })
                                        );

                                        if (!string.IsNullOrWhiteSpace(entityType))
                                        {
                                            billingAccount.Entities.Add(new CoverPayEntity
                                            {
                                                Type = entityType,
                                                Value = policy?.Payer_Name ?? string.Empty
                                            });
                                        }
                                    }

                                    billingAccounts.Add(billingAccount);
                                }
                                if (billingAccounts?.Count > 0)
                                {
                                    coverPayOutstandingInvoicesResponse.Status = "SUCCESS";
                                    coverPayOutstandingInvoicesResponse.Messages = new List<string>();
                                    coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                                    {
                                        RequestId = Guid.NewGuid().ToString(),
                                        SchemaVersion = "1.0",
                                        Source = new CoverPaySource
                                        {
                                            SourceSystemCode = "EIACCOUNTING",
                                            ReferenceId = Guid.NewGuid().ToString()
                                        },
                                        BillingAccounts = billingAccounts
                                    };
                                }
                                else
                                {
                                    FileLogger.LogError($"FinancialServicesController: GetOutstandingInvoices: Request: {data.ToString()} Client_Id: {Clients.Client_Id} Entity_Id: {entity_Id} EntityType_Id: {entityType_Id} Query: '{coverPayOutstandingInvoicesRequest?.Query ?? string.Empty}' PolicyDetails_Id_Json: {policyDetails_Id_Json} Message: No billing accounts found.");
                                    coverPayOutstandingInvoicesResponse.Status = "FAILED";
                                    coverPayOutstandingInvoicesResponse.Messages = new List<string> { "No billing accounts found." };
                                    coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                                    {
                                        RequestId = Guid.NewGuid().ToString(),
                                        SchemaVersion = "1.0",
                                        Source = new CoverPaySource
                                        {
                                            SourceSystemCode = "EIACCOUNTING",
                                            ReferenceId = Guid.NewGuid().ToString()
                                        },
                                        BillingAccounts = billingAccounts
                                    };
                                    return BadRequest(coverPayOutstandingInvoicesResponse);
                                }
                            }
                            else
                            {
                                FileLogger.LogError($"FinancialServicesController: GetOutstandingInvoices: Request: {data.ToString()} Client_Id: {Clients.Client_Id} Entity_Id: {entity_Id} EntityType_Id: {entityType_Id} Query: '{coverPayOutstandingInvoicesRequest?.Query ?? string.Empty}' PolicyDetails_Id_Json: {policyDetails_Id_Json} Message: Failed to Retrive the Outstanding Invoices.");
                                coverPayOutstandingInvoicesResponse.Status = "FAILED";
                                coverPayOutstandingInvoicesResponse.Messages = new List<string> {"Failed to Retrive the Outstanding Invoices."};
                                coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                                {
                                    RequestId = Guid.NewGuid().ToString(),
                                    SchemaVersion = "1.0",
                                    Source = new CoverPaySource
                                    {
                                        SourceSystemCode = "EIACCOUNTING",
                                        ReferenceId = Guid.NewGuid().ToString()
                                    },
                                    BillingAccounts = null
                                };
                                return BadRequest(coverPayOutstandingInvoicesResponse);
                            }
                        }
                        else
                        {
                            FileLogger.LogError($"FinancialServicesController: GetOutstandingInvoices: Request: {data.ToString()} Client_Id: {Clients.Client_Id} Entity_Id: {entity_Id} EntityType_Id: {entityType_Id} Query: '{coverPayOutstandingInvoicesRequest?.Query ?? string.Empty}' Message: Failed to Retrive the Outstanding Policies.");
                            coverPayOutstandingInvoicesResponse.Status = "FAILED";
                            coverPayOutstandingInvoicesResponse.Messages = new List<string> { "Failed to Retrive the Outstanding Policies." };
                            coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                            {
                                RequestId = Guid.NewGuid().ToString(),
                                SchemaVersion = "1.0",
                                Source = new CoverPaySource
                                {
                                    SourceSystemCode = "EIACCOUNTING",
                                    ReferenceId = Guid.NewGuid().ToString()
                                },
                                BillingAccounts = null
                            };
                            return BadRequest(coverPayOutstandingInvoicesResponse);
                        }
                    }
                    catch (Exception ex)
                    {

                        var exceptionLogInfo = new ExceptionLogInfo
                        {
                            UserSessionId = AuthenticatedUser.User_Id,
                            ErrorStackTrace = ex.StackTrace,
                            ErrorMessage = ex.Message
                        };

                        _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                        coverPayOutstandingInvoicesResponse.Status = "FAILED";
                        coverPayOutstandingInvoicesResponse.Messages = new List<string> { "Something went wrong while fetching the outstanding invoices. Please contact the administrator." };
                        coverPayOutstandingInvoicesResponse.Data = new CoverPayOutstandingInvoicesData
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            SchemaVersion = "1.0",
                            Source = new CoverPaySource
                            {
                                SourceSystemCode = "EIACCOUNTING",
                                ReferenceId = Guid.NewGuid().ToString()
                            },
                            BillingAccounts = null
                        };
                        FileLogger.LogError($"FinancialServicesController: GetOutstandingInvoices: Request: {data.ToString()} Exception: {ex} Message: {ex.Message}");
                        return StatusCode(500, coverPayOutstandingInvoicesResponse);
                    }
                }

            }

            return Ok(coverPayOutstandingInvoicesResponse);
        }
        [NonAction]
        public List<PostCashReceiptsErrors> PreparePostCashReceiptErrorLog(ExceptionLogInfo exceptionLogInfo, List<PostCashReceiptsErrors> postCashReceiptsErrorsList)
        {
            PostCashReceiptsErrors postCashReceiptsErrors = new PostCashReceiptsErrors();
            postCashReceiptsErrors.ErrorStackTrace = exceptionLogInfo.ErrorStackTrace;
            postCashReceiptsErrors.ErrorMessage = exceptionLogInfo.ErrorMessage;
            postCashReceiptsErrors.Name = exceptionLogInfo.ErrorName;
            postCashReceiptsErrors.Description = exceptionLogInfo.ErrorDescription;
            postCashReceiptsErrorsList.Add(postCashReceiptsErrors);
            return postCashReceiptsErrorsList;
        }
        [NonAction]
        public List<InvoiceDetails> GetInvoiceDetails(byte BillingType_Id, PolicyDetails policyDetails, List<PolicyDetailsLineItems> policyDetailsLineItems, byte Lob_Id, List<CommissionRules> commissionRules = null)
        {
            var PremiumLine = policyDetailsLineItems?.FirstOrDefault(x => x.LineType == "PREMIUM");
            var FeeLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "fee");
            var TaxLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "tax");
            double PremiumAmount = PremiumLine?.Premium ?? 0;
            double CommssionPercentage = PremiumLine?.HouseCommission ?? 0;
            double AgtCommPercentage = PremiumLine?.AgentCommission ?? 0;
            double AgtCommAmount = 0;
            double HseCommAmount = 0;

            AgtCommAmount = PremiumLine?.BrokerCommissionAmount ?? 0;
            HseCommAmount = Math.Round((PremiumLine?.CommissionAmount ?? 0) - AgtCommAmount, 2);
            var fullyEarnedPremiumLine = policyDetailsLineItems?.FirstOrDefault(x => x.PolicyDetailDescription == MasterLookup._CustomLineitems.FullyEarnedPremium);
            double fullyEarnedPremiumAmount = fullyEarnedPremiumLine?.Premium ?? 0;
            var fullyEarnedAgtCommAmount = fullyEarnedPremiumLine?.BrokerCommissionAmount ?? 0;
            var fullyHseCommAmount = Math.Round((fullyEarnedPremiumLine?.CommissionAmount ?? 0) - fullyEarnedAgtCommAmount, 2);
            double entityCommissionBaseAmount = 0;
            if (commissionRules?.Any() == true)
            {
                var entityCommissionRule = commissionRules.FirstOrDefault(x => (x.ApplyOnAllTransactions == true) || string.Equals(x.SubmissionType, policyDetails.TransactionType, StringComparison.OrdinalIgnoreCase));
                entityCommissionBaseAmount = entityCommissionRule?.CommissionBasisType_Id switch
                {
                    var x when x == MasterLookup._CommissionBasisTypes.HouseCommission => HseCommAmount,
                    _ => 0
                };
            }
            double TotalFees = FeeLines?.Sum(c => c.FeeTaxAmount) ?? 0;
            double TotalTaxes = TaxLines?.Sum(c => c.FeeTaxAmount) ?? 0;
            bool splitCommission = policyDetails?.ReferralUnderwriter_Id > 0;
            List<InvoiceDetails> invoiceDetailsList = new List<InvoiceDetails>();
            List<LU_LineItems_Clients> lineItems_ClientsList = _lookupRepository.GetLineItems_ClientsByBillingType_Id(policyDetails.Client_Id, BillingType_Id);
            if (lineItems_ClientsList != null)
            {
                lineItems_ClientsList = lineItems_ClientsList.Where(x => x.Is_Excluded == false).ToList();
                foreach (var item in lineItems_ClientsList.Where(x => x.Is_Excluded == false))
                {
                    InvoiceDetails invoiceDetails = new InvoiceDetails();
                    invoiceDetails.BillingType_LineItems_Directions_Id = item.BillingType_LineItems_Directions_Id;
                    invoiceDetails.Direction_Id = item.Direction_Id;
                    invoiceDetails.Paid = false;
                    invoiceDetails.LOB_Id = Lob_Id;
                    if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                    {

                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR && item.TreatAsPremium == false)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR && item.TreatAsPremium == true)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = fullyEarnedPremiumAmount - (fullyEarnedAgtCommAmount + fullyHseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP && item.TreatAsPremium == false)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP && item.TreatAsPremium == true)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = fullyEarnedPremiumAmount - (fullyEarnedAgtCommAmount + fullyHseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR && item.TreatAsPremium == false)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR && item.TreatAsPremium == true)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = fullyHseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN && item.TreatAsPremium == false)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN && item.TreatAsPremium == true)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = fullyHseCommAmount;
                        }
                        else if ((item.LineItemType_Id == MasterLookup._LineItemTypes.Fee
                            || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax) && item.Is_Commissionable == true
                            && item.Direction_Id == MasterLookup._Directions.C)
                        {
                            var RelatedCommissionableFee = policyDetailsLineItems?.FirstOrDefault(c => item.LineItemName.ToLower().Trim().Contains(c.PolicyDetailDescription.ToLower().Trim()) && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = Math.Round((RelatedCommissionableFee * AgtCommPercentage) / 100, 2);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.C && item.TreatAsPremium == false)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = AgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.C && item.TreatAsPremium == true)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = fullyEarnedAgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee
                            || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax
                            || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if ((item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                                || (item.Direction_Id == MasterLookup._Directions.AP
                                && item.EntityType_Id == MasterLookup._EntityTypes.SalesRep))
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.SalesRep)
                                {
                                    if (policyDetails.SalesRepDetails != null && policyDetails.SalesRepDetails.Name != "No Sales Rep")
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.SalesRepDetails.Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.SalesRep;
                                        invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                                    }
                                }
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            }
                            if (item.Is_Commissionable == true)
                            {
                                var RelatedCommissionableFee = policyDetailsLineItems?.FirstOrDefault(c => item.LineItemName.ToLower().Trim().Contains(c.PolicyDetailDescription.ToLower().Trim()) && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                                invoiceDetails.Amount = Math.Round(RelatedCommissionableFee - ((RelatedCommissionableFee * AgtCommPercentage) / 100), 2);
                            }
                            else
                            {
                                invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                            }
                        }
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Direct)
                    {
                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP && item.LineItemName == MasterLookup._CustomLineitems.AgencyBrokerCommission)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = AgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP && item.LineItemName == MasterLookup._CustomLineitems.UnderwriterCommission)
                        {
                            invoiceDetails.Entity_Id = policyDetails.UnderWriterDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Underwriter;
                            invoiceDetails.Amount = CommonUtility.GetEntityCommissionAmount(commissionRules, policyDetails.TransactionType, entityCommissionBaseAmount, splitCommission);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP &&
                            item.LineItemName == MasterLookup._CustomLineitems.ReferralUnderwriterCommission && splitCommission)
                        {
                            invoiceDetails.Entity_Id = policyDetails?.ReferralUnderwriter_Id ?? 0;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.ReferralUnderwriter;
                            invoiceDetails.Amount = CommonUtility.GetEntityCommissionAmount(commissionRules, policyDetails.TransactionType, entityCommissionBaseAmount, splitCommission);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = AgtCommAmount + HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            if (item.LineItemName == MasterLookup._CustomLineitems.MGACommission)
                            {

                                invoiceDetails.Amount = HseCommAmount;
                            }
                            else
                            {
                                invoiceDetails.Amount = CommssionPercentage;
                            }
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Paid = null;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount;
                            invoiceDetails.Paid = null;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if (item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            else
                            {
                                if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                }
                                else
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                        }
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                    {
                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee && policyDetails.MortgageeDetails != null && policyDetails.MortgageeDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = (long)policyDetails.MortgageeDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (policyDetails.MortgageeDetails != null && policyDetails.MortgageeDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = (long)policyDetails.MortgageeDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }
                            invoiceDetails.Amount = (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = AgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if (item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }

                            invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                        }
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                    {
                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (item.LineItem_Id == MasterLookup._LineItems.AgencyBrokerCommission)
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                                invoiceDetails.Amount = 0 - AgtCommAmount;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                                invoiceDetails.Amount = (AgtCommAmount + HseCommAmount);
                            }

                        }
                        //else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP)
                        //{
                        //    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                        //    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                        //    invoiceDetails.Amount = AgtCommAmount;
                        //}
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if (item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }

                            invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                        }
                    }
                    invoiceDetails.Amount = Math.Round(invoiceDetails.Amount, 2);
                    invoiceDetails.RemainingBalance = invoiceDetails.Amount;
                    invoiceDetailsList.Add(invoiceDetails);
                }
            }
            return invoiceDetailsList;
        }


        [NonAction]
        public List<InvoiceDetails> GetInvoiceDetailsWithInstallmentPlan(byte BillingType_Id, Invoices invoices, PolicyDetails policyDetails, List<PolicyDetailsLineItems> policyDetailsLineItems, byte Lob_Id, double policyAgtCommPercentage, List<CommissionRules> commissionRules = null)
        {
            double totalFeeTaxAmount = 0;
            double policyTotalAmount = GetInvoiceAmount(policyDetailsLineItems, BillingType_Id, out double originalPremium, out double originalAgentCommPct, out totalFeeTaxAmount);
            if (invoices.InstallmentNo == 0)
            {
                invoices.Amount = invoices.Amount - totalFeeTaxAmount;
            }
            double installmentRatio = policyTotalAmount == 0 ? 1 : Math.Round(invoices.Amount / policyTotalAmount, 10);

            var PremiumLine = policyDetailsLineItems?.FirstOrDefault(x => x.LineType == "PREMIUM");
            var FeeLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "fee");
            var TaxLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "tax");
            double OriginalPremiumAmount = PremiumLine?.Premium ?? 0;
            double PremiumAmount = Math.Round(OriginalPremiumAmount * installmentRatio, 2);

            double TotalCommissionAmount = PremiumLine?.CommissionAmount ?? 0;
            double AgtCommAmount = PremiumLine?.BrokerCommissionAmount ?? 0;
            double HseCommAmount = Math.Round((PremiumLine?.CommissionAmount ?? 0) - AgtCommAmount, 2);
            double CommssionPercentage = PremiumLine?.HouseCommission ?? 0;
            double AgtCommPercentage = PremiumLine?.AgentCommission ?? 0;

            AgtCommAmount = Math.Round(AgtCommAmount * installmentRatio, 2);
            HseCommAmount = Math.Round(HseCommAmount * installmentRatio, 2);

            double TotalFees = FeeLines?.Sum(c => c.FeeTaxAmount) ?? 0;
            double TotalTaxes = TaxLines?.Sum(c => c.FeeTaxAmount) ?? 0;
            double entityCommissionBaseAmount = 0;
            bool splitCommission = policyDetails?.ReferralUnderwriter_Id > 0;
            if (commissionRules?.Any() == true)
            {
                var entityCommissionRule = commissionRules.FirstOrDefault(x => (x.ApplyOnAllTransactions == true) || string.Equals(x.SubmissionType, policyDetails.TransactionType, StringComparison.OrdinalIgnoreCase));
                entityCommissionBaseAmount = entityCommissionRule?.CommissionBasisType_Id switch
                {
                    var x when x == MasterLookup._CommissionBasisTypes.HouseCommission => HseCommAmount,
                    _ => 0
                };
            }
            List<InvoiceDetails> invoiceDetailsList = new List<InvoiceDetails>();
            List<LU_LineItems_Clients> lineItems_ClientsList = _lookupRepository.GetLineItems_ClientsByBillingType_Id(policyDetails.Client_Id, BillingType_Id);
            if (lineItems_ClientsList != null)
            {
                lineItems_ClientsList = lineItems_ClientsList.Where(x => x.Is_Excluded == false).ToList();
                foreach (var item in lineItems_ClientsList.Where(x => x.Is_Excluded == false))
                {
                    InvoiceDetails invoiceDetails = new InvoiceDetails();
                    invoiceDetails.Invoice_Id = invoices.Id;
                    invoiceDetails.BillingType_LineItems_Directions_Id = item.BillingType_LineItems_Directions_Id;
                    invoiceDetails.Direction_Id = item.Direction_Id;
                    invoiceDetails.Paid = false;
                    invoiceDetails.LOB_Id = Lob_Id;
                    if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                    {

                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if ((item.LineItemType_Id == MasterLookup._LineItemTypes.Fee
                            || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax) && item.Is_Commissionable == true
                            && item.Direction_Id == MasterLookup._Directions.C)
                        {
                            var RelatedCommissionableFee = policyDetailsLineItems?.FirstOrDefault(c => item.LineItemName.ToLower().Trim().Contains(c.PolicyDetailDescription.ToLower().Trim()) && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = Math.Round((RelatedCommissionableFee * AgtCommPercentage) / 100, 2);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.C)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = AgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee
                            || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax
                            || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if ((item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                                || (item.Direction_Id == MasterLookup._Directions.AP
                                && item.EntityType_Id == MasterLookup._EntityTypes.SalesRep))
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;

                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.SalesRep)
                                {
                                    if (policyDetails.SalesRepDetails != null && policyDetails.SalesRepDetails.Name != "No Sales Rep")
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.SalesRepDetails.Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.SalesRep;
                                        invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                                    }
                                }
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            }
                            if (invoices.InstallmentNo == 0)
                            {
                                if (item.Is_Commissionable == true)
                                {
                                    var RelatedCommissionableFee = policyDetailsLineItems?.FirstOrDefault(c => item.LineItemName.ToLower().Trim().Contains(c.PolicyDetailDescription.ToLower().Trim()) && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                                    invoiceDetails.Amount = Math.Round(RelatedCommissionableFee - ((RelatedCommissionableFee * AgtCommPercentage) / 100), 2);
                                }
                                else
                                {
                                    invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Direct)
                    {
                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP && item.LineItemName == MasterLookup._CustomLineitems.AgencyBrokerCommission)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = AgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP && item.LineItemName == MasterLookup._CustomLineitems.UnderwriterCommission)
                        {
                            invoiceDetails.Entity_Id = policyDetails.UnderWriterDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Underwriter;
                            invoiceDetails.Amount = CommonUtility.GetEntityCommissionAmount(commissionRules, policyDetails.TransactionType, entityCommissionBaseAmount, splitCommission);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP &&
                            item.LineItemName == MasterLookup._CustomLineitems.ReferralUnderwriterCommission && splitCommission)
                        {
                            invoiceDetails.Entity_Id = policyDetails?.ReferralUnderwriter_Id ?? 0;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.ReferralUnderwriter;
                            invoiceDetails.Amount = CommonUtility.GetEntityCommissionAmount(commissionRules, policyDetails.TransactionType, entityCommissionBaseAmount, splitCommission);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = AgtCommAmount + HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            if (item.LineItemName == MasterLookup._CustomLineitems.MGACommission)
                            {

                                invoiceDetails.Amount = HseCommAmount;
                            }
                            else
                            {
                                invoiceDetails.Amount = CommssionPercentage;
                            }
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Paid = null;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount;
                            invoiceDetails.Paid = null;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if (item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            else
                            {
                                if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                }
                                else
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                        }
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                    {
                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (policyDetails.MortgageeDetails != null && policyDetails.MortgageeDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = (long)policyDetails.MortgageeDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (policyDetails.MortgageeDetails != null && policyDetails.MortgageeDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = (long)policyDetails.MortgageeDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }
                            invoiceDetails.Amount = (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                            invoiceDetails.Amount = AgtCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if (item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }
                            if (invoices.InstallmentNo == 0)
                            {
                                invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                    {
                        if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Premium && item.Direction_Id == MasterLookup._Directions.AP)
                        {
                            if (policyDetails.WholesalerDetail_Id > 0)
                            {
                                invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                            }
                            invoiceDetails.Amount = PremiumAmount - (AgtCommAmount + HseCommAmount);
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (item.LineItem_Id == MasterLookup._LineItems.AgencyBrokerCommission)
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                                invoiceDetails.Amount = 0 - AgtCommAmount;
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                                invoiceDetails.Amount = (AgtCommAmount + HseCommAmount);
                            }

                        }
                        //else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.AP)
                        //{
                        //    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                        //    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Agency;
                        //    invoiceDetails.Amount = AgtCommAmount;
                        //}
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Commission && item.Direction_Id == MasterLookup._Directions.IN)
                        {
                            invoiceDetails.Entity_Id = policyDetails.Client_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Client;
                            invoiceDetails.Amount = HseCommAmount;
                        }
                        else if (item.LineItemType_Id == MasterLookup._LineItemTypes.Fee || item.LineItemType_Id == MasterLookup._LineItemTypes.Tax || item.LineItemType_Id == MasterLookup._LineItemTypes.CarrierFee)
                        {
                            if (item.Direction_Id == MasterLookup._Directions.AP && item.Is_Passthrough)
                            {
                                invoiceDetails.EntityType_Id = item.EntityType_Id;
                                if (item.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.AgentDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Insured)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.Client)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.Client_Id;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.FinanceDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                                {
                                    if (policyDetails.WholesalerDetail_Id > 0)
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                                    }
                                    else
                                    {
                                        invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                    }
                                }
                                else if (item.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                                {
                                    invoiceDetails.Entity_Id = policyDetails.CarrierDetail_Id;
                                    invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                                }
                            }
                            else
                            {
                                invoiceDetails.Entity_Id = policyDetails.InsuredDetail_Id;
                                invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Insured;
                            }

                            invoiceDetails.Amount = policyDetailsLineItems?.FirstOrDefault(c => c.PolicyDetailDescription.ToLower().Trim() == item.LineItemName.ToLower().Trim() && c.FeeTaxAmount != 0)?.FeeTaxAmount ?? 0;
                        }
                    }
                    invoiceDetails.Amount = Math.Round(invoiceDetails.Amount, 2);
                    invoiceDetails.RemainingBalance = invoiceDetails.Amount;
                    invoiceDetailsList.Add(invoiceDetails);
                }
            }
            return invoiceDetailsList;
        }

        [NonAction]
        public double GetInvoiceAmount(List<PolicyDetailsLineItems> policyDetailsLineItems, byte BillingType_Id, out double PolicyPremiumAmount, out double PolicyAgtCommPercentage, out double totalFeeTaxAmount)
        {
            double CalculatedPremium = 0;
            double TotalAmount = 0;
            PolicyPremiumAmount = 0;
            PolicyAgtCommPercentage = 0;
            totalFeeTaxAmount = 0;
            var GroupedByCoverage = policyDetailsLineItems.GroupBy(o => o.Coverage);
            if (GroupedByCoverage?.Count() > 0)
            {
                foreach (var item in GroupedByCoverage)
                {
                    var PremiumLine = policyDetailsLineItems?.FirstOrDefault(x => x.LineType == "PREMIUM" && x.Coverage == item.Key);
                    var FeeLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "fee" && c.Coverage == item.Key);
                    var TaxLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "tax" && c.Coverage == item.Key);
                    double PremiumAmount = PremiumLine?.Premium ?? 0;
                    PolicyPremiumAmount = PremiumLine?.Premium ?? 0;
                    double HseCommPercentage = PremiumLine?.HouseCommission ?? 0;
                    double AgtCommPercentage = PremiumLine?.AgentCommission ?? 0;
                    PolicyAgtCommPercentage = AgtCommPercentage;
                    double HseCommAmount = PremiumLine?.CommissionAmount ?? 0;
                    double AgtCommAmount = PremiumLine?.BrokerCommissionAmount ?? 0;
                    double EndAdditionalFees = FeeLines?.Where(x => x.Is_Commissionable == true).Sum(c => c.FeeTaxAmount) ?? 0;

                    double commissionableFees =
                        policyDetailsLineItems?
                            .Where(c =>
                                c.LineType != null &&
                                c.LineType.Equals("fee", StringComparison.OrdinalIgnoreCase) &&
                                c.Is_Commissionable)
                            .Sum(x => (double?)x.FeeTaxAmount) ?? 0;


                    double nonCommissionableFees =
                        policyDetailsLineItems?
                            .Where(c =>
                                c.LineType != null &&
                                c.LineType.Equals("fee", StringComparison.OrdinalIgnoreCase) &&
                                !c.Is_Commissionable)
                            .Sum(x => (double?)x.FeeTaxAmount) ?? 0;


                    double commissionRate = Convert.ToDouble(AgtCommPercentage) / 100;
                    double commissionAmount = Math.Round(commissionableFees * commissionRate, 2);
                    double netCommissionableFees = commissionableFees - commissionAmount;
                    double TotalFees = netCommissionableFees + nonCommissionableFees;

                    //double TotalFees = FeeLines?.Sum(c => c.FeeTaxAmount) ?? 0;
                    double TotalTaxes = TaxLines?.Sum(c => c.FeeTaxAmount) ?? 0;
                    totalFeeTaxAmount += (TotalFees + TotalTaxes);
                    if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                    {
                        CalculatedPremium = PremiumAmount - AgtCommAmount;
                        TotalAmount = TotalAmount + CalculatedPremium;
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                    {
                        CalculatedPremium = PremiumAmount;
                        TotalAmount = TotalAmount + CalculatedPremium;
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Direct)
                    {
                        CalculatedPremium = (HseCommAmount - AgtCommAmount) + AgtCommAmount;
                        TotalAmount = TotalAmount + CalculatedPremium;
                    }
                }
            }
            TotalAmount = Math.Round(TotalAmount, MasterLookup._GeneralSettings.MathRoundPrecision);
            return TotalAmount;
        }
        [NonAction]
        public double GetInvoicedAmount(List<InvoiceDetails> invoiceDetails)
        {
            double InvoicedAmount = 0;
            if (invoiceDetails != null && invoiceDetails.Count > 0)
            {
                InvoicedAmount = Convert.ToDouble(invoiceDetails.Where(
                    x => x.Direction_Id == MasterLookup._Directions.AR &&
                    x.EntityType_Id > 0 && (x.Entity_Id > 0 || (x.EntityType_Id == MasterLookup._EntityTypes.State && x.Entity_Id == 0)) && x.Amount != 0).Sum(x => Convert.ToDecimal(x.Amount)));
            }
            return InvoicedAmount;
        }
        [NonAction]
        public double GetTotalPolicyAmounts(PolicyDetails policyDetails, List<PolicyDetailsLineItems> policyDetailsLineItems)
        {
            double Premium = 0;
            double Fees = 0;
            double Tax = 0;
            var GroupedByCoverage = policyDetailsLineItems?.GroupBy(o => o.Coverage);
            if (GroupedByCoverage?.Count() > 0)
            {
                foreach (var item in GroupedByCoverage)
                {
                    var PremiumLine = policyDetailsLineItems?.FirstOrDefault(x => x.LineType == "PREMIUM" && x.Coverage == item.Key);
                    var FeeLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "fee" && c.Coverage == item.Key);
                    var TaxLines = policyDetailsLineItems?.Where(c => c.LineType.ToLower() == "tax" && c.Coverage == item.Key);
                    Premium = Premium + (PremiumLine?.Premium ?? 0);
                    Fees = Fees + (FeeLines?.Sum(x => x.FeeTaxAmount) ?? 0);
                    Tax = Tax + (TaxLines?.Sum(x => x.FeeTaxAmount) ?? 0);
                }
            }
            return (Premium + Fees + Tax);
        }

        [NonAction]
        public ServiceResponse GetCommissionsServiceResponse(PolicyDetails policyDetails, List<PolicyDetailsLineItems> policyDetailsLineItems)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            serviceResponse.Status = true;
            double TotalCommissionAmount = 0;
            double TotalAgencyCommissionAmount = 0;
            byte BillType = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
            var GroupedByCoverage = policyDetailsLineItems?.GroupBy(o => o.Coverage);
            if (GroupedByCoverage?.Count() > 0)
            {
                foreach (var item in GroupedByCoverage)
                {
                    var PremiumLine = policyDetailsLineItems?.FirstOrDefault(x => x.LineType == "PREMIUM" && x.Coverage == item.Key);
                    TotalCommissionAmount += PremiumLine?.CommissionAmount ?? 0;
                    TotalAgencyCommissionAmount += PremiumLine?.BrokerCommissionAmount ?? 0;

                    if (PremiumLine != null)
                    {
                        if (Math.Abs(PremiumLine.BrokerCommissionAmount) > Math.Abs(PremiumLine.CommissionAmount))
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message += $"Total Commission cannot be less than Agency Commission (LOB: {PremiumLine.Coverage}).  Please correct it and try again. ";
                        }
                    }

                }
            }
            if (TotalCommissionAmount == 0 && TotalAgencyCommissionAmount == 0 && BillType == MasterLookup._BillingTypes.Direct)
            {
                var configuredLineItems = _lookupRepository.GetLineItems_ClientsByBillingType_Id(policyDetails.Client_Id, BillType);

                var configNames = new HashSet<string>(
                    configuredLineItems?.Where(x => x.Is_Excluded == false && x.Direction_Id == MasterLookup._Directions.AR)?.Select(c => c.LineItemName?.Trim()) ?? Enumerable.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);

                var feeTaxItems = policyDetailsLineItems?
                    .Where(x =>
                        (x.LineType?.ToUpper() == "FEE" || x.LineType?.ToUpper() == "TAX") &&
                        x.FeeTaxAmount > 0 && configNames?.Contains(x.PolicyDetailDescription?.Trim()) == true)
                    .ToList() ?? new List<PolicyDetailsLineItems>();

                if (TotalCommissionAmount == 0 && TotalAgencyCommissionAmount == 0)
                {
                    if (!(feeTaxItems?.Any() == true))
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Message += "No Commissions.";
                    }
                }
            }
            return serviceResponse;
        }

        [HttpGet]
        public ServiceResponse GetPaymentPlans(/*long Client_Id, */long CarrierId)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                string username = User.Identity?.Name;
                List<LU_PaymentPlans> paymentPlans = new List<LU_PaymentPlans>();

                if (!string.IsNullOrEmpty(username))
                {
                    var AuthenticatedUser = _usersRepository.GetUsers(username);
                    var Clients = _accountRepository.GetClients(AuthenticatedUser.Client_Id);

                    if (CarrierId > 0 /*&& Client_Id > 0*/)
                    {
                        CarrierDetails carrierDetails = _installmentPlansRepository.GetAllCarriers().FirstOrDefault(x => x.Client_Id == Clients.Client_Id && x.CarrierReference_Id == CarrierId.ToString());
                        if (carrierDetails != null)
                        {
                            paymentPlans = LookupUtility.GetAllPaymentPlans()
                                .Where(x => x.Client_Id == Clients.Client_Id && x.Carrier_Id == carrierDetails.CarrierDetail_Id)
                                .ToList();
                        }
                    }
                }
                serviceResponse.Status = true;
                serviceResponse.Data = paymentPlans;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
                return serviceResponse;
            }

        }
        [NonAction]
        public List<PolicyPaymentSchedule> ScheduledInvoices(PolicyDetails policyDetails, AccountingPost_Req accountingPost_Req, Invoices invoices, byte BillingType_Id, long Client_Id, DateTime EndorsementDate, string TransactionType, long OriginalSubmission_Id, long PreviousSubmission_Id, string policyNumber, out double policyAgtCommPercentage, out double totalFeeTaxAmount)
        {// Fetch payment plan
            policyAgtCommPercentage = 0;
            totalFeeTaxAmount = 0;
            var paymentPlan = _installmentPlansRepository.GetPaymentPlanBy_PaymentCode(
    policyDetails.PaymentPlan,
    Client_Id,
    policyDetails.CarrierDetail_Id, EndorsementDate, TransactionType, OriginalSubmission_Id, PreviousSubmission_Id, policyNumber);

            if (paymentPlan == null /*|| string.IsNullOrEmpty(policyDetails.PaymentPlan)*/)
                return new List<PolicyPaymentSchedule>();

            if (!paymentPlan.Status)
                throw new InvalidOperationException("This plan is disabled for new policies.");

            bool isEndorsement = TransactionType == MasterLookup._EITransactionTypes.END;
            var installments = new List<PolicyPaymentSchedule>();
            int sequence = 1;
            byte? installmentNo = 1;
            double policyPremium;
            //double policyAgtCommPercentage;

            // Calculate billable premium and commission %
            double billableTotal = GetInvoiceAmount(
                policyDetails.PolicyDetailsLineItemsList,
                BillingType_Id,
                out policyPremium,
                out policyAgtCommPercentage, out totalFeeTaxAmount);


            double invoiceTotal = billableTotal;

            double downPaymentAmount = 0;
            if (billableTotal > 0)
            {
                downPaymentAmount = Math.Round(Math.Max(invoiceTotal * (paymentPlan.DownPaymentPercent / 100.0), paymentPlan.DownPaymentMinimum), 2);
            }
            else
            {
                downPaymentAmount = Math.Round(Math.Min(invoiceTotal * (paymentPlan.DownPaymentPercent / 100.0), paymentPlan.DownPaymentMinimum), 2);
            }

            DateTime anchorDate;
            if (paymentPlan.PaymentAnchorType == null)
            {
                anchorDate = DateTime.Today;
            }
            else if (paymentPlan.PaymentAnchorType == "Effective Date")
            {
                if (policyDetails.TransactionType == "END")
                {
                    anchorDate = policyDetails?.EndorsementEffectiveDate ?? DateTime.Today;
                }
                else
                {
                    anchorDate = policyDetails.EffectiveDate;
                }
            }
            else
            {
                anchorDate = invoices.CreatedOn;
            }

            // Ensure anchorDate is valid
            if (anchorDate == DateTime.MinValue || anchorDate == DateTime.MaxValue)
                throw new InvalidOperationException("Anchor date is not valid.");

            var invoiceDueDateChangeEnabled = LookupUtility.GetClientSettings(Client_Id, MasterLookup._ClientSettings.InvoiceDueDateChangeEnabled);
            if (string.Equals(invoiceDueDateChangeEnabled, "Y", StringComparison.OrdinalIgnoreCase))
            {
                string invoiceInstallmentDueDateEffectiveFromSettingKey = string.Empty;
                string settingKey = string.Empty;
                if (BillingType_Id == MasterLookup._BillingTypes.Insured && TransactionType != MasterLookup._EITransactionTypes.END)
                {
                    settingKey = $"{MasterLookup._ClientSettings.InvoiceInstallmentDueDateTermDays}_Insured";
                    invoiceInstallmentDueDateEffectiveFromSettingKey = $"{MasterLookup._ClientSettings.InvoiceInstallmentDueDateEffectiveFrom}_Insured";
                }
                else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee && TransactionType != MasterLookup._EITransactionTypes.END)
                {
                    settingKey = $"{MasterLookup._ClientSettings.InvoiceInstallmentDueDateTermDays}_Mortgagee";
                    invoiceInstallmentDueDateEffectiveFromSettingKey = $"{MasterLookup._ClientSettings.InvoiceInstallmentDueDateEffectiveFrom}_Mortgagee";
                }

                var invoiceInstallmentDueDateEffectiveFrom = LookupUtility.GetClientSettings(Client_Id, invoiceInstallmentDueDateEffectiveFromSettingKey);
                if (!string.IsNullOrEmpty(invoiceInstallmentDueDateEffectiveFrom))
                {
                    var invoiceInstallmentDueDateEffectiveFromDate = Convert.ToDateTime(invoiceInstallmentDueDateEffectiveFrom).Date;
                    if (policyDetails.EffectiveDate.Date >= invoiceInstallmentDueDateEffectiveFromDate)
                    {
                        if (!string.IsNullOrEmpty(settingKey))
                        {
                            var invoiceInstallmentDueDateTermDays = LookupUtility.GetClientSettings(Client_Id, settingKey);
                            if (!string.IsNullOrEmpty(invoiceInstallmentDueDateTermDays) && int.TryParse(invoiceInstallmentDueDateTermDays, out int configuredDays))
                            {
                                paymentPlan.DownPaymentTermDays = configuredDays;
                                anchorDate = invoices.CreatedOn.Date > policyDetails?.EffectiveDate.Date ? invoices.CreatedOn : policyDetails.EffectiveDate;
                            }
                        }
                    }
                }
            }

            var useMaxAnchorDate = LookupUtility.GetClientSettings(Client_Id, MasterLookup._ClientSettings.InvoiceInstallmentDueDateUseMaxAnchorDate);
            if (!string.IsNullOrEmpty(useMaxAnchorDate))
            {
                var invoiceDate = invoices.CreatedOn.Date;
                DateTime anchorEffectiveDate = policyDetails?.EffectiveDate.Date ?? DateTime.MinValue.Date;
                if (TransactionType == MasterLookup._EITransactionTypes.END)
                {
                    anchorEffectiveDate = policyDetails?.EndorsementEffectiveDate?.Date ?? DateTime.MinValue.Date;
                }
                anchorDate = invoiceDate > anchorEffectiveDate ? invoices.CreatedOn.Date : anchorEffectiveDate;
            }

            DateTime downPaymentDue = SafeAddDays(anchorDate, paymentPlan.DownPaymentTermDays);
            DateTime downPaymentBillDate = DateTime.Today;

            if (!isEndorsement)
            {
                installments.Add(new PolicyPaymentSchedule
                {
                    InstallmentNo = null,
                    IsDownPayment = true,
                    Sequence = sequence++,
                    Amount = downPaymentAmount,
                    DueDate = downPaymentDue,
                    BillDate = downPaymentBillDate,
                    InvoiceCreatedDate = DateTime.Now,
                    PaymentPlan_Id = paymentPlan.PaymentPlan_Id,
                    PolicyDetails_Id = policyDetails.Id
                });

                if (paymentPlan.NumberOfPayments == 0)
                    return installments; // fully paid at downpayment
            }

            if (isEndorsement && paymentPlan?.NumberOfPayments == 0)
            {
                installments.Add(new PolicyPaymentSchedule
                {
                    InstallmentNo = null,
                    IsDownPayment = true,
                    Sequence = sequence++,
                    Amount = downPaymentAmount,
                    DueDate = downPaymentDue,
                    BillDate = downPaymentBillDate,
                    InvoiceCreatedDate = DateTime.Now,
                    PaymentPlan_Id = paymentPlan.PaymentPlan_Id,
                    PolicyDetails_Id = policyDetails.Id
                });

                if (paymentPlan?.NumberOfPayments == 0)
                    return installments; // fully paid at downpayment
            }

            int numInstallments = paymentPlan.NumberOfPayments;

            double remainingAmount = isEndorsement ? Math.Round(invoiceTotal, 2) : Math.Round(invoiceTotal - downPaymentAmount, 2);
            double baseInstallmentAmount = Math.Round(remainingAmount / numInstallments, 2);

            double sumSoFar = 0;

            // First Installment Due Date


            //       DateTime firstInstallmentDue = paymentPlan.FirstPaymentOffsetDays != 0
            //? downPaymentDue.AddDays(paymentPlan.FirstPaymentOffsetDays)
            //: CalculateNextDueDate(
            //    (Enum_InstallmentBasisTypes)paymentPlan.InstallmentBasisType_Id,
            //    downPaymentDue,
            //    paymentPlan.EffectiveDate,
            //    invoices.CreatedOn,
            //    (Enum_IntervalUnitTypes)paymentPlan.IntervalUnitType_Id,
            //    paymentPlan.IntervalValue,
            //    paymentPlan.FixedDayOfMonth, true);

            //       DateTime due = firstInstallmentDue;
            //       for (int i = 0; i < numInstallments; i++)
            //       {

            //           double net = (i == numInstallments - 1)
            //               ? Math.Round(remainingNet - sumSoFar, 2)
            //               : baseInstallmentNet;

            //           sumSoFar += net;

            //           DateTime billDate = paymentPlan.IsSetBillDateEqualDue
            //               ? due
            //               : due.AddDays(-paymentPlan.LeadTime);

            //           installments.Add(new PolicyPaymentSchedule
            //           {
            //               Sequence = sequence++,
            //               InstallmentNo = installmentNo++,
            //               Amount = net,
            //               DueDate = due,
            //               BillDate = billDate,
            //               PaymentPlan_Id = paymentPlan.PaymentPlan_Id,
            //               PolicyDetails_Id = policyDetails.Id
            //           });


            //           due = CalculateNextDueDate(
            //               (Enum_InstallmentBasisTypes)paymentPlan.InstallmentBasisType_Id,
            //               due,
            //               paymentPlan.EffectiveDate,
            //               invoices.CreatedOn,
            //               (Enum_IntervalUnitTypes)paymentPlan.IntervalUnitType_Id,
            //               paymentPlan.IntervalValue,
            //               paymentPlan.FixedDayOfMonth, false);
            //       }

            DateTime firstInstallmentBase = isEndorsement ? anchorDate : downPaymentDue; // installments always start from anchor
            DateTime firstInstallmentDue;

            if (paymentPlan.FirstPaymentOffsetDays.HasValue)
            {
                // User has explicitly configured an override
                firstInstallmentDue = firstInstallmentBase.AddDays(paymentPlan.FirstPaymentOffsetDays.Value);
            }
            else
            {
                // No override configured - first installment is one interval after the anchor date
                firstInstallmentDue = CalculateNextDueDate(
                    (Enum_InstallmentBasisTypes)paymentPlan.InstallmentBasisType_Id,
                    firstInstallmentBase,
                    paymentPlan.EffectiveDate,
                    invoices.CreatedOn,
                    (Enum_IntervalUnitTypes)paymentPlan.IntervalUnitType_Id,
                    paymentPlan.IntervalValue,
                    paymentPlan.FixedDayOfMonth,
                    true);
            }

            //invoiceCreatedDate = CalculateInvoiceDate(
            //    (Enum_InstallmentBasisTypes)paymentPlan.InstallmentBasisType_Id,
            //    firstInstallmentBase, 
            //    paymentPlan.EffectiveDate,
            //    invoices.CreatedOn,
            //    (Enum_IntervalUnitTypes)paymentPlan.IntervalUnitType_Id,
            //    paymentPlan.IntervalValue);



            DateTime due;
            if (isEndorsement)
            {
                due = paymentPlan.FirstRemainingInstallmentDueDate;
            }
            else
            {
                due = firstInstallmentDue;
            }
            for (int i = 0; i < numInstallments; i++)
            {
                double installmentAmount = (i == numInstallments - 1) ? Math.Round(remainingAmount - sumSoFar, 2) : baseInstallmentAmount;

                sumSoFar += installmentAmount;

                DateTime billDate = paymentPlan.IsSetBillDateEqualDue
                    ? due
                    : due.AddDays(-paymentPlan.LeadTime);

                installments.Add(new PolicyPaymentSchedule
                {
                    Sequence = sequence++,
                    InstallmentNo = installmentNo++,
                    Amount = installmentAmount,
                    DueDate = due,
                    BillDate = billDate,
                    //InvoiceCreatedDate = invoiceCreatedDate,
                    PaymentPlan_Id = paymentPlan.PaymentPlan_Id,
                    PolicyDetails_Id = policyDetails.Id
                });

                due = CalculateNextDueDate(
                    (Enum_InstallmentBasisTypes)paymentPlan.InstallmentBasisType_Id,
                    due,  // next installment starts from previous installment due
                    paymentPlan.EffectiveDate,
                    invoices.CreatedOn,
                    (Enum_IntervalUnitTypes)paymentPlan.IntervalUnitType_Id,
                    paymentPlan.IntervalValue,
                    paymentPlan.FixedDayOfMonth,
                    false);


                //invoiceCreatedDate = CalculateInvoiceDate(
                //    (Enum_InstallmentBasisTypes)paymentPlan.InstallmentBasisType_Id,
                //    invoiceCreatedDate,
                //    paymentPlan.EffectiveDate,
                //    invoices.CreatedOn,
                //    (Enum_IntervalUnitTypes)paymentPlan.IntervalUnitType_Id,
                //    paymentPlan.IntervalValue);
            }

            return installments;


        }
        private static DateTime SafeAddDays(DateTime date, int days)
        {
            if (days > 0 && date > DateTime.MaxValue.AddDays(-days))
                return DateTime.MaxValue;
            if (days < 0 && date < DateTime.MinValue.AddDays(-days))
                return DateTime.MinValue;

            return date.AddDays(days);
        }

        private static DateTime CalculateNextDueDate(
    Enum_InstallmentBasisTypes basis,
    DateTime previousDue,
    DateTime effectiveDate,
    DateTime invoiceDate,
    Enum_IntervalUnitTypes unit,
    int intervalValue,
    int? fixedDayOfMonth,
    bool isFirstInstallment = false)
        {
            return basis switch
            {
                Enum_InstallmentBasisTypes.EffectiveDate or Enum_InstallmentBasisTypes.DateBilled
                    => unit == Enum_IntervalUnitTypes.Days
                        ? previousDue.AddDays(intervalValue)
                        : previousDue.AddMonths(intervalValue),

                Enum_InstallmentBasisTypes.DayOfMonth
                    => CalculateDayOfMonthDue(previousDue, intervalValue, fixedDayOfMonth, isFirstInstallment),

                _ => throw new ArgumentOutOfRangeException(nameof(basis), $"Unsupported basis: {basis}")
            };
        }
        private static DateTime CalculateInvoiceDate(
    Enum_InstallmentBasisTypes basis,
    DateTime previousDue,
    DateTime effectiveDate,
    DateTime invoiceDate,
    Enum_IntervalUnitTypes unit,
    int intervalValue)
        {
            return basis switch
            {
                Enum_InstallmentBasisTypes.EffectiveDate or Enum_InstallmentBasisTypes.DateBilled
                    => unit == Enum_IntervalUnitTypes.Days
                        ? previousDue.AddDays(intervalValue)
                        : previousDue.AddMonths(intervalValue),

                _ => throw new ArgumentOutOfRangeException(nameof(basis), $"Unsupported basis: {basis}")
            };
        }


        private static DateTime CalculateDayOfMonthDue(DateTime previousDue, int monthsToAdd, int? fixedDayOfMonth, bool isFirstInstallment = false)
        {
            if (!fixedDayOfMonth.HasValue)
                throw new InvalidOperationException("FixedDayOfMonth is required when Basis = DayOfMonth.");

            DateTime targetDate;

            if (isFirstInstallment && fixedDayOfMonth.HasValue)
            {

                //var nextMonth = previousDue.AddDays((int)fixedDayOfMonth);
                //int day = Math.Min(fixedDayOfMonth.Value, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                //targetDate = new DateTime(nextMonth.Year, nextMonth.Month, day);

                //var nextMonth = previousDue.AddMonths(1); // Add 1 calendar month
                //int day = Math.Min(fixedDayOfMonth.Value, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                //targetDate = new DateTime(nextMonth.Year, nextMonth.Month, day);
                int day = Math.Min(fixedDayOfMonth.Value,
                       DateTime.DaysInMonth(previousDue.Year, previousDue.Month));

                targetDate = new DateTime(previousDue.Year, previousDue.Month, day);

                if (targetDate <= previousDue)
                {
                    var nextMonth = previousDue.AddMonths(1);
                    day = Math.Min(fixedDayOfMonth.Value,
                                   DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                    targetDate = new DateTime(nextMonth.Year, nextMonth.Month, day);
                }
            }
            else
            {

                var next = previousDue.AddMonths(monthsToAdd);
                int day = Math.Min(fixedDayOfMonth.Value, DateTime.DaysInMonth(next.Year, next.Month));
                targetDate = new DateTime(next.Year, next.Month, day);
            }

            return targetDate;
        }
        [NonAction]
        public bool IsNonMonetary(List<PolicyDetailsLineItems> items)
        {
            return items == null || !items.Any() ||
          items.All(x =>
              (x.LineType == "PREMIUM" && x.Premium == 0) ||
              ((x.LineType == "FEE" || x.LineType == "TAX") && x.FeeTaxAmount == 0)
          );
        }
        private Payments CreateCancellationPayment(PolicyDetails policyDetails, Clients clients, Users users)
        {
            SqlTransaction transaction = null;
            Payments payment = new Payments();
            try
            {
                transaction = _baseRepository.GetNewTransaction();
                payment.Direction_Id = MasterLookup._Directions.AR;
                payment.Amount = 0;
                payment.RemainingAmount = 0;
                payment.ReferenceNumber = $"Cancellation Installment Reconciliation for Policy Number: {policyDetails.PolicyNumber}";
                payment.OperatingBank_Id = _clientServiceDefaultOperatingBankRepository.GetClientServiceDefaultOperatingBank(users.Client_Id, MasterLookup._ServiceProviderTypes.Payments, clients.PaymentService_Id)?.OperatingBank_Id ?? 0;
                payment.PaymentDate = DateTime.Now;
                payment.PaymentType_Id = MasterLookup._PaymentTypes.CreditCard;
                payment.PaymentStatus_Id = MasterLookup._PaymentStatus.Completed;
                payment.Client_Id = clients.Client_Id;
                payment.PaymentDirection_Id = MasterLookup._PaymentDirections.Incoming;
                byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;


                if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                {
                    payment.EntityType_Id = MasterLookup._EntityTypes.Agency;
                    payment.Entity_Id = policyDetails.AgentDetail_Id;
                }
                else if (BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                {
                    payment.EntityType_Id = MasterLookup._EntityTypes.Insured;
                    payment.Entity_Id = policyDetails.InsuredDetail_Id;
                }
                else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                {
                    payment.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                    payment.Entity_Id = (long)policyDetails.MortgageeDetail_Id;
                }

                payment = _paymentsRepository.InsertUpdatePayments(transaction, payment);
                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction.Connection != null)
                {
                    transaction.Rollback();
                }
            }
            return payment;
        }
        private ServiceResponse ProcessCancellation(long Client_Id, DateTime CancellationDate, Payments payments, long OriginalSubmission_Id, Clients clients, Users authenticatedUser, double cancellationAmount)
        {
            var serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            var PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();

            try
            {
                PaymentsTransaction = _baseRepository.GetNewTransaction();
                if (CancellationDate == DateTime.MinValue)
                {
                    CancellationDate = new DateTime(2026, 10, 26);
                }
                var payload = _installmentPlansRepository
                    .GetPendingCancellationInvoiceDetails(Client_Id, CancellationDate, OriginalSubmission_Id, cancellationAmount)
                    .GroupBy(x => x.Invoice_Id)
                    .Select(g => new SelectedInvoiceDetailDTO
                    {
                        Invoice_Id = g.Key,
                        InvoiceDetail_Id = 0,
                        Amount = g.Sum(x => x.Amount),
                        RemainingBalance = g.Sum(x => x.RemainingBalance),
                        IsPartial = true,
                        InvoiceDetail_Ids = g.Select(x => x.InvoiceDetail_Id).ToList()
                    })
                    .ToList();

                if (!payload.Any())
                {
                    serviceResponse.Status = false;
                    serviceResponse.Data = null;
                    serviceResponse.Message = "No pending cancellation invoice details found.";
                    return serviceResponse;
                }

                string postJson = JsonConvert.SerializeObject(payload);
                var paymentApplicationLineItemsList = _paymentsRepository.ApplyPayments(clients.Client_Id, postJson, payments.Payment_Id, MasterLookup._Directions.AR, false);
                if (paymentApplicationLineItemsList.Count() > 0)
                {
                    string paymentApplicationLineitemJson = JsonConvert.SerializeObject(paymentApplicationLineItemsList.Select(x => new { PaymentApplicationLineItem_Id = x.PaymentApplicationLineItem_Id }).ToList());
                    List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                    JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();
                    if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                    {
                        QBOUtility qBOUtility = new QBOUtility(clients.Client_Id, authenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntry, true);
                        if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                        {
                            serviceResponse.Status = true;
                            serviceResponse.Message = "Applied payment successfully.";
                            PaymentsTransaction.Commit();
                            return serviceResponse;
                        }
                        else
                        {
                            if (PaymentsTransaction.Connection != null)
                            {
                                PaymentsTransaction.Rollback();
                            }

                            string Result = _paymentsRepository.UnApplyPayments(clients.Client_Id, payments.Direction_Id, paymentApplicationLineitemJson);
                            serviceResponse.Status = false;
                            serviceResponse.Message = createJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {createJournalRes.Fault.Error[0].Detail}" : $"Unable to sync to Intuit! {Result}";
                            return serviceResponse;
                        }
                    }
                    else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                    {
                        SageUtility sageUtility = new SageUtility(Client_Id, authenticatedUser.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        var createJournalRes = Task.Run(() =>
                        sageUtility.CreateJournalEntry_Intacct(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntry, true)).Result;
                        var serializer = new XmlSerializer(typeof(SageResponse));
                        SageResponse createSageJournalResponse = new SageResponse();
                        using (var reader = new StringReader(createJournalRes))
                        {
                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                        }
                        if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
                        {
                            serviceResponse.Status = true;
                            serviceResponse.Message = $"Successfully applied Payment (Payment ID: {payments.Payment_Id}).";
                            PaymentsTransaction.Commit();
                            return serviceResponse;
                        }
                        else
                        {
                            if (PaymentsTransaction.Connection != null)
                            {
                                PaymentsTransaction.Rollback();
                            }
                            string Result = _paymentsRepository.UnApplyPayments(clients.Client_Id, payments.Direction_Id, paymentApplicationLineitemJson);
                            serviceResponse.Status = false;
                            serviceResponse.Message = $"Unable to sync to Sage. {Result}";
                            return serviceResponse;
                        }
                    }

                    PaymentsTransaction.Commit();
                    serviceResponse.Status = true;
                    serviceResponse.Data = payload;
                    serviceResponse.Message = "Cancellation Amount Applied successfully.";
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Data = payload;
                    serviceResponse.Message = "Failed to Apply the Cancellation Amount.";
                }
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = "An error occurred while preparing the invoice payload.";
                serviceResponse.Data = null;
            }

            return serviceResponse;
        }
        private Payments CreateProcessAccountingPayment(PolicyDetails policyDetails, Clients clients, Users users, double totalAmount)
        {
            SqlTransaction transaction = null;
            Payments payment = new Payments();
            try
            {
                transaction = _baseRepository.GetNewTransaction();
                payment.Direction_Id = MasterLookup._Directions.AR;
                payment.Amount = totalAmount;
                payment.RemainingAmount = totalAmount;
                payment.ReferenceNumber = $"Process Accounting Payment for Policy Number: {policyDetails.PolicyNumber}";
                payment.OperatingBank_Id = _clientServiceDefaultOperatingBankRepository.GetClientServiceDefaultOperatingBank(users.Client_Id, MasterLookup._ServiceProviderTypes.Payments, clients.PaymentService_Id)?.OperatingBank_Id ?? 0;
                payment.PaymentDate = DateTime.Now;
                payment.PaymentType_Id = MasterLookup._PaymentTypes.CreditCard;
                payment.PaymentStatus_Id = MasterLookup._PaymentStatus.Completed;
                payment.Client_Id = clients.Client_Id;
                payment.PaymentDirection_Id = MasterLookup._PaymentDirections.Incoming;
                byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;


                if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                {
                    payment.EntityType_Id = MasterLookup._EntityTypes.Agency;
                    payment.Entity_Id = policyDetails.AgentDetail_Id;
                }
                else if (BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                {
                    payment.EntityType_Id = MasterLookup._EntityTypes.Insured;
                    payment.Entity_Id = policyDetails.InsuredDetail_Id;
                }
                else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                {
                    payment.EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                    payment.Entity_Id = (long)policyDetails.MortgageeDetail_Id;
                }
                else if (BillingType_Id == MasterLookup._BillingTypes.Direct)
                {
                    if (policyDetails.WholesalerDetail_Id != null && policyDetails.WholesalerDetail_Id != 0)
                    {
                        payment.Entity_Id = policyDetails.WholesalerDetail_Id ?? 0;
                        payment.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                    }
                    else
                    {
                        payment.Entity_Id = policyDetails.CarrierDetail_Id;
                        payment.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                    }
                }

                payment = _paymentsRepository.InsertUpdatePayments(transaction, payment);
                if (payment?.Payment_Id > 0)
                {
                    var createJournalResponse = new CreateJournalRes();
                    if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                    {
                        var qBOUtility = new QBOUtility(users.Client_Id, users.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        createJournalResponse = qBOUtility.CreatePaymentJournalEntry(transaction, payment);
                        if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                        {
                            if (transaction?.Connection != null)
                            {
                                transaction.Rollback();
                            }
                            var exceptionLogInfo = new ExceptionLogInfo
                            {
                                UserSessionId = users.User_Id,
                                ErrorStackTrace = "FinancialServiceController/CorePayPayments/CreateJournalEntry",
                                ErrorMessage = createJournalResponse?.Fault?.Error?[0]?.Message ?? string.Empty,
                                ErrorName = "Create Payment Journal Entry Error",
                                ErrorDescription = $"Error while creating a payment journal entry for Submission_Id:{policyDetails.Submission_Id}",
                            };
                            _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                            return null;
                        }
                    }
                    else if (clients?.AccountingService_Id == MasterLookup._Services.Sage)
                    {
                        SageResponse createSageJournalResponse = new SageResponse();
                        SageUtility sageUtility = new SageUtility(users.Client_Id, users.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        var createJournalRes = Task.Run(() => sageUtility.CreatePaymentJournalEntry_Intacct(transaction, payment)).Result;

                        var serializer = new XmlSerializer(typeof(SageResponse));
                        using (var reader = new StringReader(createJournalRes))
                        {
                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                        }
                        if (createSageJournalResponse == null || createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                        {
                            if (transaction?.Connection != null)
                            {
                                transaction.Rollback();
                            }
                            var xmlDoc = XDocument.Parse(createJournalRes);
                            var status = xmlDoc.Descendants("status").FirstOrDefault(x => x.Parent?.Name == "result")?.Value;
                            string errorMsg = string.Empty;
                            if (status == "failure")
                            {
                                errorMsg = xmlDoc.Descendants("description2").FirstOrDefault()?.Value;
                            }
                            var exceptionLogInfo = new ExceptionLogInfo
                            {
                                UserSessionId = users.User_Id,
                                ErrorStackTrace = "FinancialServiceController/CorePayPayments/CreateJournalEntry",
                                ErrorMessage = errorMsg ?? string.Empty,
                                ErrorName = "Create Payment Journal Entry Error",
                                ErrorDescription = $"Error while creating a payment journal entry for Submission_Id:{policyDetails.Submission_Id}",
                            };

                            _exceptionLogInfoRepository.InsertExceptionLogInfo(exceptionLogInfo);
                            return null;
                        }
                    }
                }
                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction?.Connection != null)
                {
                    transaction.Rollback();
                }
            }
            return payment;
        }
    }

}
