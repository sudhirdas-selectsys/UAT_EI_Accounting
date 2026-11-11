using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Accounting.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Accounting.Web.Controllers
{
    [SessionSecurityFilterAttribute]
    public class BaseController : Controller
    {
        public bool Is_AjaxRequest { get; set; }
        public UserSession userSession { get; set; }
        public long PolicyDetails_Id { get; set; }
        public int InstallmentNo { get; set; }
        public PolicyDetails _PolicyDetails { get; set; }
        public ServiceResponse serviceResponse = new ServiceResponse();

        public List<PolicyDetails> _PolicyDetailList { get; set; }
        public List<Payments> _PaymentsList { get; set; }
        public List<TransactionHistory> _TransactionHistoryList { get; set; }
        public Clients clients { get; set; }
        internal readonly IPolicyDetailsRepository _policyDetailsRepository = new PolicyDetailsRepository();
        internal readonly IInvoiceRepository _invoiceRepository = new InvoiceRepository();
        internal readonly IBaseRepository _baseRepository = new BaseRepository();
        internal readonly IClientsRepository _accountRepository = new ClientsRepository();
        internal readonly IUsersRepository _usersRepository = new UsersRepository();
        internal readonly IPaymentsRepository _paymentsRepository = new PaymentsRepository();
        internal readonly ILookupRepository _lookupRepository = new LookupRepository();
        internal readonly IChartofAccountsRepository _chartofAccountsRepository = new ChartofAccountsRepository();
        internal readonly ITransactionHistoryRepository _transactionhistoryRepository = new TransactionHistoryRepository();
        internal readonly IJournalEntryLogRepository _journalEntryLogRepository = new JournalEntryLogRepository();
        internal readonly ILOBCarrierGroupingRepository _lobcarriergroupingRepository = new LOBCarrierGroupingRepository();
        internal readonly ILOBGroupingDetailsRepository _lobgroupingDetailsRepository = new LOBGroupingDetailsRepository();
        internal readonly ICarrierGroupingDetailsRepository _carriergroupingDetailsRepository = new CarrierGroupingDetailsRepository();
        internal readonly ILOBMappingRepository _lobMappingRepository = new LOBMappingRepository();
        internal readonly IInstallmentPlansRepository _installmentPlansRepository = new InstallmentPlansRepository();
        internal readonly IClientServiceDefaultOperatingBankRepository _clientServiceDefaultOperatingBankRepository = new ClientServiceDefaultOperatingBankRepository();
        internal readonly IExternalAccountInformationRepository _externalAccountInformationRepository = new ExternalAccountInformationRepository();
		internal readonly IMakePaymentsRepository _makepaymentRepository = new MakePaymentsRepository();
        internal readonly IExternalVendorEntityReferenceRepository _externalVendorEntityReferenceRepository = new ExternalVendorEntityReferenceRepository();
        internal readonly IPayments_BillsMappingRepository _payments_BillsMappingRepository = new Payments_BillsMappingRepository();
        internal readonly IExternalBillReferenceRepository _externalBillReferenceRepository = new ExternalBillReferenceRepository();
        internal readonly IPayments_BillsMappingRepository _paymentsBillsMappingRepository = new Payments_BillsMappingRepository();
        internal readonly ICommissionRulesRepository _commissionRulesRepository= new CommissionRulesRepository();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllers = context.RouteData.Values["controller"]?.ToString();
            var actions = context.RouteData.Values["action"]?.ToString();

            //Allow SSO endpoints without session
            if (controllers == "SSO")
            {
                base.OnActionExecuting(context);
                return;
            }
            Is_AjaxRequest = IsAjaxRequest(context.HttpContext.Request);
            ViewBag.LoadPartialPanel = true;
            ViewBag.Is_AjaxRequest = Is_AjaxRequest;
            userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            _PolicyDetails = new PolicyDetails();
            if (userSession is null)
            {
                context.HttpContext.Session.Clear();
                context.Result = new ContentResult
                {
                    Content = "<script>window.top.location.href='/Login/Index';</script>",
                    ContentType = "text/html"
                };
                return;
            }
            try
            {
                if (userSession.ConfigurationClient_Id > 0)
                {
                    clients = _accountRepository.GetClients(userSession.ConfigurationClient_Id);
                }
                else
                {
                    clients = _accountRepository.GetClients(userSession.Client_Id);
                }

                if (context != null && !Is_AjaxRequest)
                {
                    ViewBag.LoadPartialPanel = false;
                }
                if ((context.RouteData != null && context.RouteData.Values["PolicyDetails_Id"] != null) || (context.HttpContext.Request.Method == "POST" && context.ActionArguments != null))
                {
                    ViewBag.Quote_Id = context.RouteData.Values["PolicyDetails_Id"];
                    if (context.RouteData.Values["PolicyDetails_Id"] != null)
                    {
                        PolicyDetails_Id = Convert.ToInt64(context.RouteData.Values["PolicyDetails_Id"]);
                    }
                    else
                    {
                        if (context.ActionArguments.Count > 0)
                        {
                            try
                            {
                                PolicyDetails_Id = ((PolicyDetails)(context.ActionArguments.ToList()[0].Value)).PolicyDetails_Id;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    if (PolicyDetails_Id > 0)
                    {
                        _PolicyDetails = _policyDetailsRepository.GetPolicyDetails(PolicyDetails_Id);
                        if (_PolicyDetails != null)
                        {
                            _PolicyDetails.AgentDetails = _policyDetailsRepository.GetAgentDetails(_PolicyDetails.AgentDetail_Id);
                            _PolicyDetails.InsuredDetails = _policyDetailsRepository.GetInsuredDetails(_PolicyDetails.InsuredDetail_Id);
                            _PolicyDetails.InterestDetails = _policyDetailsRepository.GetInterestDetails(_PolicyDetails.InterestDetail_Id ?? 0);
                            _PolicyDetails.CarrierDetails = _policyDetailsRepository.GetCarrierDetails(_PolicyDetails.CarrierDetail_Id);
                            _PolicyDetails.FinanceDetails = _policyDetailsRepository.GetFinanceDetails(_PolicyDetails.FinanceDetail_Id ?? 0);
                            _PolicyDetails.UnderWriterDetails = _policyDetailsRepository.GetUnderWriterDetails(_PolicyDetails.UnderWriterDetail_Id);
                            if (!string.IsNullOrEmpty(_PolicyDetails.PaymentPlan))
                            {
                                _PolicyDetails.InstallmentNo = InstallmentNo;
                            }
                            _PolicyDetails.Invoices = _invoiceRepository.GetInvoices(PolicyDetails_Id, _PolicyDetails.InstallmentNo);
                            _PolicyDetails.InvoicesList = _invoiceRepository.GetInvoicesByPolicyDetail_Id(PolicyDetails_Id);
                            if (_PolicyDetails.InvoicesList?.Count > 0)
                            {
                                foreach (var invoice in _PolicyDetails.InvoicesList)
                                {
                                    invoice.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(invoice.Id);
                                    if (invoice.InvoicesDetailsList?.Count() > 0)
                                    {
                                        invoice.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(invoice.InvoicesDetailsList, _PolicyDetails);
                                    }
                                }
                            }
                            if (context.Controller is Controller controller)
                            {
                                var controlType = controller.GetType();
                                var actionName = ControllerContext.RouteData.Values["action"].ToString().ToLower();
                                if (controlType == typeof(HomeController))
                                {
                                    if (context.HttpContext.Request.Method == "GET")
                                    {
                                        if (actionName == "dashboardgrid")
                                        {
                                            int PageSize = Convert.ToInt32(context.HttpContext.Request.Query["PageSize"].ToString());
                                            var PageIndex = Convert.ToInt32(context.HttpContext.Request.Query["PageIndex"].ToString());
                                            long TotalRecords = 0;
                                            _PolicyDetailList = _policyDetailsRepository.GetAllPolicyDetails(_PolicyDetails, userSession.Client_Id, PageSize, PageIndex, ref TotalRecords);
                                            if (_PolicyDetailList?.Count > 0)
                                            {
                                                foreach (var policy in _PolicyDetailList)
                                                {
                                                    policy.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(policy.Id);
                                                }
                                            }
                                            ViewBag.TotalRecords = TotalRecords;
                                            ViewBag.PageIndex = PageIndex;
                                            ViewBag.PageSize = PageSize;
                                        }
                                    }
                                }
                                else if (controlType == typeof(PolicyDetailsController))
                                {
                                    if (context.HttpContext.Request.Method == "GET")
                                    {
                                        if (actionName == "policydetailstransfer")
                                        {
                                            _PolicyDetails.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(PolicyDetails_Id);
                                        }
                                    }
                                }
                                else if (controlType == typeof(InvoicesController))
                                {
                                    if (context.HttpContext.Request.Method == "GET")
                                    {
                                        if (actionName.Equals("generateinvoice", StringComparison.OrdinalIgnoreCase) ||
                                            actionName.Equals("downloadinvoice", StringComparison.OrdinalIgnoreCase))
                                        {
                                            _PolicyDetails.Clients = _accountRepository.GetClients(_PolicyDetails.Client_Id);
                                            _PolicyDetails.WholesalerDetails = _policyDetailsRepository.GetWholesalerDetails(_PolicyDetails.WholesalerDetail_Id ?? 0);
                                            _PolicyDetails.Invoices = _invoiceRepository.GetInvoices(_PolicyDetails.Id, _PolicyDetails.InstallmentNo);
                                            _PolicyDetails.Invoices.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(_PolicyDetails.Invoices.Id);
                                            if (_PolicyDetails.Invoices.InvoicesDetailsList?.Count > 0)
                                            {
                                                _PolicyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(_PolicyDetails.Invoices.InvoicesDetailsList, _PolicyDetails, true);
                                            }
                                            if (_PolicyDetails?.Clients?.PaymentService_Id == MasterLookup._Services.ePay)
                                            {
                                                byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == _PolicyDetails.BillType)?.BillingType_Id ?? 0;
                                                long Entity_Id = 0;
                                                byte EntityType_Id = 0;
                                                if (BillingType_Id == MasterLookup._BillingTypes.Agency)
                                                {
                                                    Entity_Id = _PolicyDetails.AgentDetail_Id;
                                                    EntityType_Id = MasterLookup._EntityTypes.Agency;
                                                }
                                                else if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                                {
                                                    Entity_Id = _PolicyDetails.InsuredDetail_Id;
                                                    EntityType_Id = MasterLookup._EntityTypes.Insured;
                                                }
                                                _PolicyDetails.ExternalAccountInformation = _externalAccountInformationRepository.GetExternalAccountInformation(Entity_Id, EntityType_Id, _PolicyDetails.Client_Id);
                                            }
                                        }
                                        //else if (actionName == "downloadinvoice")
                                        //{
                                        //    _PolicyDetails.Clients = _accountRepository.GetClients(_PolicyDetails.Client_Id);
                                        //    _PolicyDetails.WholesalerDetails = _policyDetailsRepository.GetWholesalerDetails(_PolicyDetails.WholesalerDetail_Id ?? 0);
                                        //    _PolicyDetails.Invoices = _invoiceRepository.GetInvoices(_PolicyDetails.Id);
                                        //    _PolicyDetails.Invoices.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(_PolicyDetails.Invoices.Id);
                                        //    if (_PolicyDetails.Invoices.InvoicesDetailsList?.Count > 0)
                                        //    {
                                        //        _PolicyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(_PolicyDetails.Invoices.InvoicesDetailsList, _PolicyDetails, true);
                                        //    }
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (context.Controller is Controller controller)
                    {
                        var controlType = controller.GetType();
                        var actionName = ControllerContext.RouteData.Values["action"].ToString().ToLower();
                        if (controlType == typeof(HomeController))
                        {
                            if (context.HttpContext.Request.Method == "GET")
                            {
                                if (actionName == "dashboardgrid")
                                {
                                    int PageSize = Convert.ToInt32(context.HttpContext.Request.Query["PageSize"].ToString());
                                    var PageIndex = Convert.ToInt32(context.HttpContext.Request.Query["PageIndex"].ToString());
                                    long TotalRecords = 0;
                                    _PolicyDetailList = _policyDetailsRepository.GetAllPolicyDetails(_PolicyDetails, userSession.Client_Id, PageSize, PageIndex, ref TotalRecords);
                                    if (_PolicyDetailList?.Count > 0)
                                    {
                                        foreach (var policy in _PolicyDetailList)
                                        {
                                            policy.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(policy.Id);
                                        }
                                    }
                                    ViewBag.TotalRecords = TotalRecords;
                                    ViewBag.PageIndex = PageIndex;
                                    ViewBag.PageSize = PageSize;
                                    ViewBag.Client_Id = userSession.Client_Id;
                                }
                            }
                        }
                        else if (controlType == typeof(PaymentsController))
                        {
                            if (context.HttpContext.Request.Method == "GET")
                            {
                                if (actionName == "paymentsgrid")
                                {
                                    int PageSize = Convert.ToInt32(context.HttpContext.Request.Query["PageSize"].ToString());
                                    var PageIndex = Convert.ToInt32(context.HttpContext.Request.Query["PageIndex"].ToString());
                                    long TotalRecords = 0;
                                    _PaymentsList = _paymentsRepository.GetAllPayments(userSession.Client_Id, PageSize, PageIndex, ref TotalRecords);
                                    ViewBag.TotalRecords = TotalRecords;
                                    ViewBag.PageIndex = PageIndex;
                                    ViewBag.PageSize = PageSize;
                                }
                            }
                        }
                        else if (controlType == typeof(TransactionHistoryController))
                        {
                            if (context.HttpContext.Request.Method == "GET")
                            {
                                if (actionName == "transactionhistorygrid")
                                {
                                    int PageSize = Convert.ToInt32(context.HttpContext.Request.Query["PageSize"].ToString());
                                    var PageIndex = Convert.ToInt32(context.HttpContext.Request.Query["PageIndex"].ToString());
                                    long TotalRecords = 0;
                                    TransactionHistorySearch search = new TransactionHistorySearch();
                                    search.Client_Id = userSession.Client_Id;
                                    _TransactionHistoryList = _transactionhistoryRepository.GetAllTransactionHistory(search, PageSize, PageIndex, ref TotalRecords);
                                    ViewBag.TotalRecords = TotalRecords;
                                    ViewBag.PageIndex = PageIndex;
                                    ViewBag.PageSize = PageSize;
                                }
                            }
                        }
                        else if (controlType == typeof(MakePaymentsController))
                        {
                            if (context.HttpContext.Request.Method == "GET")
                            {
                                if (actionName == "makepaymentsgrid")
                                {
                                    int PageSize = Convert.ToInt32(context.HttpContext.Request.Query["PageSize"].ToString());
                                    var PageIndex = Convert.ToInt32(context.HttpContext.Request.Query["PageIndex"].ToString());
                                    long TotalRecords = 0;
                                    _PaymentsList = _paymentsRepository.GetAllPayments(userSession.Client_Id, PageSize, PageIndex, ref TotalRecords);
                                    ViewBag.TotalRecords = TotalRecords;
                                    ViewBag.PageIndex = PageIndex;
                                    ViewBag.PageSize = PageSize;
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex) { }
        }
        public bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }
    }
}
