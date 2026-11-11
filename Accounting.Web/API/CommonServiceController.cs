using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.External.QBO.Vendor;
using Accounting.Entities.External.Sage.Journal;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.EI_Connect;
using Accounting.Utilities.External.QBO;
using Accounting.Utilities.External.Sage;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Abstract;
using Accounting.Web.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Accounting.Web.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonServiceController : ControllerBase
    {
        private readonly IPDFOptions _IPDFOptions;
        private readonly ViewRender view;
        IPolicyDetailsRepository _policyDetailsRepository = new PolicyDetailsRepository();
        IInvoiceRepository _invoiceRepository = new InvoiceRepository();
        IClientsRepository _accountRepository = new ClientsRepository();
        IExternalAccountInformationRepository _externalAccountInformationRepository = new ExternalAccountInformationRepository();
        internal readonly IJournalEntryLogRepository _journalEntryLogRepository = new JournalEntryLogRepository();
        internal readonly ILookupRepository _lookupRepository = new LookupRepository();
        internal readonly IUsersRepository _usersRepository = new UsersRepository();
        internal readonly IBaseRepository _baseRepository = new BaseRepository();
        internal readonly IPaymentsRepository _paymentsRepository = new PaymentsRepository();
        public PolicyDetails _PolicyDetails = new PolicyDetails();
        private readonly IReportService _IReportService;
        private readonly ReportsUtility reportsUtility;
        private readonly ICoverPayService coverPayService;
        private readonly IMSOfficeFileOptions _IMSOfficeFileOptions;
        private readonly IZip _IZip;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        internal readonly IExternalVendorEntityReferenceRepository _externalVendorEntityReferenceRepository = new ExternalVendorEntityReferenceRepository();
        internal readonly IExternalBillReferenceRepository _externalBillReferenceRepository = new ExternalBillReferenceRepository();
        internal readonly IPayments_BillsMappingRepository _payments_BillsMappingRepository = new Payments_BillsMappingRepository();

        public CommonServiceController(ViewRender view, IPDFOptions iPDFOptions, IReportService iReportService, IUserContextRepository userContextRepository, ICoverPayService coverPayService,
        IMSOfficeFileOptions iMSOfficeFileOptions, IZip iZip, IWebHostEnvironment iWebHostEnvironment)
        {
            this.view = view;
            _IPDFOptions = iPDFOptions;
            _IReportService = iReportService;
            reportsUtility = new ReportsUtility(userContextRepository);
            this.coverPayService = coverPayService;
            this._IMSOfficeFileOptions = iMSOfficeFileOptions;
            this._IZip = iZip;
            this._IWebHostEnvironment = iWebHostEnvironment;
        }

        [HttpGet("GenerateInvoicePDF")]
        public async Task<ActionResult<ServiceResponse>> GenerateInvoicePDF(long PolicyDetails_Id, int InstallmentNo)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Authorization token missing.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid token.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                _PolicyDetails = _policyDetailsRepository.GetPolicyDetails(PolicyDetails_Id);
                if (_PolicyDetails != null)
                {
                    _PolicyDetails.AgentDetails = _policyDetailsRepository.GetAgentDetails(_PolicyDetails.AgentDetail_Id);
                    _PolicyDetails.InsuredDetails = _policyDetailsRepository.GetInsuredDetails(_PolicyDetails.InsuredDetail_Id);
                    _PolicyDetails.InterestDetails = _policyDetailsRepository.GetInterestDetails(_PolicyDetails.InterestDetail_Id ?? 0);
                    _PolicyDetails.CarrierDetails = _policyDetailsRepository.GetCarrierDetails(_PolicyDetails.CarrierDetail_Id);
                    _PolicyDetails.FinanceDetails = _policyDetailsRepository.GetFinanceDetails(_PolicyDetails.FinanceDetail_Id ?? 0);
                    _PolicyDetails.UnderWriterDetails = _policyDetailsRepository.GetUnderWriterDetails(_PolicyDetails.UnderWriterDetail_Id);
                    _PolicyDetails.MortgageeDetails = _policyDetailsRepository.GetMortgageeDetails(Convert.ToInt64(_PolicyDetails?.MortgageeDetail_Id));
                    //if (!string.IsNullOrEmpty(_PolicyDetails.PaymentPlan))
                    //{
                    _PolicyDetails.InstallmentNo = InstallmentNo;
                    //}
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
                    _PolicyDetails.TotalAppliedAmount = Convert.ToDouble(_paymentsRepository.GetInvoiceAppliedPaymentDetails(_PolicyDetails.Invoices.Id, _PolicyDetails.Clients.Client_Id)?.TotalAppliedAmount);
                    _PolicyDetails.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(_PolicyDetails?.Invoices?.PolicyDetails_Id ?? 0);
                    _PolicyDetails.Clients = _accountRepository.GetClients(_PolicyDetails.Client_Id);
                    _PolicyDetails.WholesalerDetails = _policyDetailsRepository.GetWholesalerDetails(_PolicyDetails.WholesalerDetail_Id ?? 0);
                    _PolicyDetails.Invoices = _invoiceRepository.GetInvoices(_PolicyDetails.Id, _PolicyDetails.InstallmentNo);
                    _PolicyDetails.Invoices.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(_PolicyDetails.Invoices.Id);
                    if (_PolicyDetails.Invoices.InvoicesDetailsList?.Count > 0)
                    {
                        _PolicyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(_PolicyDetails.Invoices.InvoicesDetailsList, _PolicyDetails, true);
                    }
                    if (_PolicyDetails?.Clients?.PaymentService_Id == MasterLookup._Services.ePay || _PolicyDetails?.Clients?.PaymentService_Id == MasterLookup._Services.CorePay)
                    {
                        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == _PolicyDetails.BillType)?.BillingType_Id ?? 0;
                        long Entity_Id = 0;
                        byte EntityType_Id = 0;
                        string PostalCode = string.Empty;
                        if (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                        {
                            Entity_Id = _PolicyDetails.AgentDetail_Id;
                            EntityType_Id = MasterLookup._EntityTypes.Agency;
                            PostalCode = _PolicyDetails?.AgentDetails?.ZipCode;
                        }
                        else if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                        {
                            Entity_Id = _PolicyDetails.InsuredDetail_Id;
                            EntityType_Id = MasterLookup._EntityTypes.Insured;
                            PostalCode = _PolicyDetails?.InsuredDetails?.ZipCode;
                        }
                        else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                        {
                            Entity_Id = _PolicyDetails?.MortgageeDetail_Id ?? 0;
                            EntityType_Id = MasterLookup._EntityTypes.Mortgagee;
                            PostalCode = _PolicyDetails?.MortgageeDetails?.ZipCode;
                        }
                        _PolicyDetails.ExternalAccountInformation = _externalAccountInformationRepository.GetExternalAccountInformation(Entity_Id, EntityType_Id, _PolicyDetails.Client_Id);
                        if (_PolicyDetails.ExternalAccountInformation != null)
                        {
                            _PolicyDetails.ExternalAccountInformation.ZipCode = PostalCode;
                            string paymentUrlTemplate = LookupUtility.GetClientSettings(_PolicyDetails.Clients.Client_Id, MasterLookup._ClientSettings.InvoicePaymentURLTemplate);
                            string finalPaymentUrl = CommonUtility.ApplyTemplateFromObject(paymentUrlTemplate, _PolicyDetails.ExternalAccountInformation);
                            _PolicyDetails.ExternalAccountInformation.PaymentURL = finalPaymentUrl;

                            if (_PolicyDetails?.Clients?.PaymentService_Id == MasterLookup._Services.CorePay && (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.Insured || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed || BillingType_Id == MasterLookup._BillingTypes.Mortgagee))
                            {
                                var enableCoverPayPaymentLink = LookupUtility.GetClientSettings(_PolicyDetails.Clients.Client_Id, MasterLookup._ClientSettings.EnableCoverPayPaymentLink);
                                if (!string.IsNullOrEmpty(enableCoverPayPaymentLink))
                                {
                                    var enableCoverPayStripePaymentLink = LookupUtility.GetClientSettings(_PolicyDetails.Clients.Client_Id, MasterLookup._ClientSettings.EnableCoverPayStripePaymentLink);
                                    if (!string.IsNullOrEmpty(enableCoverPayStripePaymentLink))
                                    {
                                        var coverPayExternalClient_Id = LookupUtility.GetClientSettings(_PolicyDetails.Clients.Client_Id, MasterLookup._ClientSettings.CoverPayExternalClient_Id);
                                        var coverPayExternalPaymentLinkURL = LookupUtility.GetClientSettings(_PolicyDetails.Clients.Client_Id, MasterLookup._ClientSettings.CoverPayExternalPaymentLinkURL);
                                        if (!string.IsNullOrEmpty(coverPayExternalClient_Id) && !string.IsNullOrEmpty(coverPayExternalPaymentLinkURL))
                                        {
                                            string dynamicCoverPayURL = string.Format(coverPayExternalPaymentLinkURL, coverPayExternalClient_Id, _PolicyDetails?.ExternalAccountInformation?.AccountNumber, _PolicyDetails?.ExternalAccountInformation?.ZipCode);
                                            _PolicyDetails.ExternalPaymentLink = dynamicCoverPayURL;
                                        }
                                    }
                                    else
                                    {
                                        var externalPaymentDetails = coverPayService.GetExternalPaymentDetails(_PolicyDetails.Clients.Client_Id, MasterLookup._Services.CorePay, PolicyDetails_Id);
                                        if (string.IsNullOrEmpty(externalPaymentDetails?.PaymentLink))
                                        {
                                            var coverPayEntityType = string.Empty;
                                            var coverPayEntityName = string.Empty;
                                            var coverPayEntityEmail = string.Empty;
                                            if (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                                            {
                                                coverPayEntityType = BillingType_Id == MasterLookup._BillingTypes.Agency ? "Agency" : "AgencyDetailed";
                                                coverPayEntityName = _PolicyDetails?.AgentDetails?.Name ?? string.Empty;
                                                coverPayEntityEmail = _PolicyDetails?.AgentDetails?.Email ?? string.Empty;
                                            }
                                            else if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                                            {
                                                coverPayEntityType = "Insured";
                                                coverPayEntityName = _PolicyDetails?.InsuredDetails?.Name ?? string.Empty;
                                                coverPayEntityEmail = _PolicyDetails?.InsuredDetails?.Email ?? string.Empty;
                                            }
                                            else if (BillingType_Id == MasterLookup._BillingTypes.Mortgagee)
                                            {
                                                coverPayEntityType = "Mortgagee";
                                                coverPayEntityName = _PolicyDetails?.MortgageeDetails?.Name ?? string.Empty;
                                                coverPayEntityEmail = _PolicyDetails?.MortgageeDetails?.Email ?? string.Empty;
                                            }

                                            var coverPayURL = coverPayService.GenerateCoverPayPaymentLink(_PolicyDetails.Clients.Client_Id, PolicyDetails_Id, _PolicyDetails?.PolicyNumber ?? string.Empty, PolicyDetails_Id, coverPayEntityType, coverPayEntityName, coverPayEntityEmail, _PolicyDetails?.ExternalAccountInformation?.AccountNumber, _PolicyDetails?.ExternalAccountInformation?.ZipCode);
                                            if (!string.IsNullOrEmpty(coverPayURL))
                                            {
                                                _PolicyDetails.ExternalPaymentLink = coverPayURL;
                                            }
                                        }
                                        else
                                        {
                                            _PolicyDetails.ExternalPaymentLink = externalPaymentDetails?.PaymentLink;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    DocumentFileInfo documentFileInfo = Download(_PolicyDetails);
                    if (documentFileInfo?.Bytes.Length > 0 && !string.IsNullOrEmpty(documentFileInfo?.Base64String))
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Data = documentFileInfo;
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "There was an error while processing your request.";
                    }
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = $"Policy Details not found for the PolicyId: {PolicyDetails_Id}.";
                }

                if (serviceResponse.Status)
                {
                    return Ok(serviceResponse);
                }
                else
                {
                    return BadRequest(serviceResponse);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
                return this.StatusCode((int)HttpStatusCode.InternalServerError, serviceResponse);
            }
        }

        [HttpPost("InActivePoliciesBySubmissionIds")]
        public async Task<ActionResult<ServiceResponse>> InActivePoliciesBySubmissionIds([Required] long client_Id, [Required] string username, [Required] List<long> submissionIds)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Authorization token missing.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid token.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                var Clients = _accountRepository.GetClients(client_Id);
                var AuthenticatedUser = _usersRepository.GetUsers(username);
                string guid = Guid.NewGuid().ToString();
                var results = new List<object>();
                if (submissionIds?.Count > 0)
                {
                    FileLogger.LogInformation($"CommonServiceController: InActivePoliciesBySubmissionIds: Guid: {guid} Client_Id: {Clients?.Client_Id}: UserName: {username} SubmissionIds: {JsonConvert.SerializeObject(submissionIds)}");
                    foreach (var submissionId in submissionIds)
                    {
                        FileLogger.LogInformation($"CommonServiceController: InActivePoliciesBySubmissionIds: Guid: {guid} Client_Id: {Clients?.Client_Id}: UserName: {username} SubmissionId: {submissionId}");
                        SqlTransaction PolicyDetailsTransaction = null;
                        try
                        {
                            PolicyDetailsTransaction = _baseRepository.GetNewTransaction();
                            if (PolicyDetailsTransaction == null)
                            {
                                results.Add(new
                                {
                                    SubmissionId = submissionId,
                                    Success = false,
                                    Message = "Unable to initiate a Transaction for the process."
                                });
                            }
                            else
                            {
                                var policyDetails = _policyDetailsRepository.GetPolicyDetailsBySubmission_Id(Convert.ToInt64(submissionId), client_Id) ?? new PolicyDetails();
                                if (policyDetails?.Id > 0)
                                {
                                    var PaymentApplicationsList = _paymentsRepository.GetAllPaymentApplicationLineItemsByPolicyDetail_Id(policyDetails.Id);
                                    if (PaymentApplicationsList?.Count() > 0)
                                    {
                                        results.Add(new
                                        {
                                            SubmissionId = submissionId,
                                            Success = false,
                                            Message = $"Payment applications already exist for Invoice number(s): {string.Join(", ", PaymentApplicationsList.Select(x => x.InvoiceNo).Distinct())} (Policy Number '{policyDetails.PolicyNumber}')."
                                        });
                                        continue;
                                    }

                                    byte _AccountingType_Id = 0;
                                    byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                                    var _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(Clients.Client_Id, BillingType_Id);
                                    if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                                    {
                                        _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                                    }
                                    else
                                    {
                                        _AccountingType_Id = Clients.AccountingType_Id;
                                    }


                                    var PolicyInvoices = _invoiceRepository.GetAllInvoicesByPolicyDetail_Id(policyDetails.Id);
                                    foreach (var invoice in PolicyInvoices)
                                    {
                                        var journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                        if (Clients.AccountingService_Id == MasterLookup._Services.Intuit && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                        {
                                            QBOUtility qBOUtility = new QBOUtility(Clients.Client_Id, AuthenticatedUser.User_Id, Clients.AccountingService_Id, _AccountingType_Id);
                                            if (journalEntries != null)
                                            {
                                                var deleteJournalResponse = qBOUtility.DeleteJournalEntry(journalEntries);
                                                if (deleteJournalResponse?.JournalEntry?.Id == null || string.IsNullOrEmpty(deleteJournalResponse?.JournalEntry?.Id))
                                                {
                                                    results.Add(new
                                                    {
                                                        Success = false,
                                                        SubmissionId = submissionId,
                                                        Message = "Cannot sync invoice with Intuit!" + deleteJournalResponse?.Fault?.Error?[0]?.Detail
                                                    });
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
                                                        results.Add(new
                                                        {
                                                            Success = false,
                                                            SubmissionId = submissionId,
                                                            Message = "Cannot sync invoice with Sage!"
                                                        });
                                                    }

                                                    var xmlDoc = XDocument.Parse(deleteJournalRes);
                                                    var status = xmlDoc.Descendants("status").FirstOrDefault(x => x.Parent?.Name == "result")?.Value;
                                                    if (status == "failure")
                                                    {
                                                        results.Add(new
                                                        {
                                                            Success = false,
                                                            SubmissionId = submissionId,
                                                            Message = "Cannot sync invoice with Sage!.."
                                                        });
                                                    }
                                                    else if (status == "success")
                                                    {
                                                        _journalEntryLogRepository.DeleteJournalEntry(MasterLookup._Sources.Invoice, invoice.Id);
                                                        results.Add(new
                                                        {
                                                            Success = true,
                                                            SubmissionId = submissionId,
                                                            Message = "Journal deleted successfully in Sage."
                                                        });
                                                    }
                                                    else
                                                    {
                                                        results.Add(new
                                                        {
                                                            Success = false,
                                                            SubmissionId = submissionId,
                                                            Message = "Unexpected response from Sage."
                                                        });
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
                                                        results.Add(new
                                                        {
                                                            Success = false,
                                                            SubmissionId = submissionId,
                                                            Message = "Cannot sync invoice with Intuit!" + deleteExistingJournalResponse?.Fault?.Error?[0]?.Detail
                                                        });
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
                                                        results.Add(new
                                                        {
                                                            Success = false,
                                                            SubmissionId = submissionId,
                                                            Message = "Cannot sync invoice with Sage!"
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                        _invoiceRepository.InactiveInvoiceDetails(PolicyDetailsTransaction, invoice.Id, Clients.Client_Id);
                                    }

                                    if (policyDetails?.Id > 0)
                                    {
                                        _invoiceRepository.InActiveInvoices(PolicyDetailsTransaction, policyDetails.Id, Clients.Client_Id);
                                        _policyDetailsRepository.InactivePolicyDetailsLineItems(PolicyDetailsTransaction, policyDetails.Id, Clients.Client_Id);
                                        _policyDetailsRepository.InactivePolicyDetails(PolicyDetailsTransaction, policyDetails.Id, Clients.Client_Id);
                                        results.Add(new
                                        {
                                            SubmissionId = submissionId,
                                            Success = true,
                                            Message = "Policy and invoices inactivated successfully."
                                        });

                                        if (PolicyDetailsTransaction.Connection != null)
                                        {
                                            PolicyDetailsTransaction.Commit();
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (PolicyDetailsTransaction.Connection != null)
                            {
                                PolicyDetailsTransaction.Rollback();
                            }
                            results.Add(new
                            {
                                SubmissionId = submissionId,
                                Success = false,
                                Message = ex.Message
                            });
                        }
                    }
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "SubmissionIds are Required.";
                    FileLogger.LogInformation($"CommonServiceController: InActivePoliciesBySubmissionIds: Guid: {guid} Client_Id: {Clients?.Client_Id}: UserName: {username} ServiceResponse: {JsonConvert.SerializeObject(serviceResponse)}");
                    return BadRequest(serviceResponse);
                }

                serviceResponse.Status = true;
                serviceResponse.Message = "InActivePoliciesBySubmissionIds executed successfully.";
                serviceResponse.Data = results;
                FileLogger.LogInformation($"CommonServiceController: InActivePoliciesBySubmissionIds: Guid: {guid} Client_Id: {Clients?.Client_Id}: UserName: {username} ServiceResponse: {JsonConvert.SerializeObject(serviceResponse)}");
                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
                return this.StatusCode((int)HttpStatusCode.InternalServerError, serviceResponse);
            }
        }

        [HttpPost("DownloadReport")]
        public IActionResult DownloadReport([FromQuery] long Report_Id,[FromQuery] long Client_Id,[FromQuery] byte FileType_Id, [FromQuery] bool IsAgentId_Include, [FromBody] List<JsonElement> InputSQLInputParameters)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Authorization token missing.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid token.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                var SQLInputParameters = InputSQLInputParameters.Select(x => x.Deserialize<LU_SQLInputParameters>()).ToList();
                int? totalRecords;
                int PageSize = MasterLookup._DefaultPageSize;
                int PageIndex = MasterLookup._DefaultPageNumber;
                int? TotalRecords = 0;
                if (SQLInputParameters?.Count > 0)
                {
                    SQLInputParameters.Add(new LU_SQLInputParameters { ParameterName = "Client_Id", InputValue = Convert.ToString(Client_Id) });
                }
                //long Client_Id = userSession.Client_Id;
                if (ReportsUtility.ValidReportWithoutTemplates.Contains(Report_Id))
                {
                    SQLInputParameters.AddRange(new List<LU_SQLInputParameters>
            {
                new() {
                    ParameterName = "PageSize",
                    InputValue = Convert.ToString(PageSize)
                },
                new() {
                    ParameterName = "PageIndex",
                    InputValue = Convert.ToString(PageIndex)
                },
                new() {
                    ParameterName = "TotalRecords",
                    InputValue = "0"
                }
            });
                }

                List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
                LU_ReportTemplates ReportTemplate = LookupUtility.GetAllReportTemplates().FirstOrDefault(x => x.Report_Id == Report_Id);
                if (ReportTemplate != null)
                {
                    outputTablesList = reportsUtility.GetReportOutputTablesWithoutTotalrecords(Report_Id, SQLInputParameters, null, Client_Id);
                }
                else
                {
                    outputTablesList = reportsUtility.GetReportOutputTables(Report_Id, SQLInputParameters, out totalRecords, FileType_Id);
                }
                DateTime? AsOfDate = null;

                var asOfDateValue = SQLInputParameters.Where(x => x.ParameterName == "As_Of_Date").Select(x => x.InputValue).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(asOfDateValue))
                {
                    if (DateTime.TryParseExact(
                            asOfDateValue,
                            new[] { "MM/dd/yyyy", "M/d/yyyy" },
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var parsedDate))
                    {
                        AsOfDate = parsedDate;
                    }
                    else
                    {
                        AsOfDate = DateTime.Now;
                    }
                }
                bool IsOutstandingInvoices = false;
                bool IsSimplifiedView = false;
                if (ReportTemplate != null)
                {
                    if (SQLInputParameters.Count > 0)
                        IsOutstandingInvoices = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "OutstandingBalances").Select(x => x.InputValue).FirstOrDefault());

                    if (SQLInputParameters.Count > 0 && ReportsUtility.ValidReportWithTemplates.Contains(Report_Id))
                    {
                        IsSimplifiedView = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "Simplified").Select(x => x.InputValue).FirstOrDefault());
                    }
                }
                DocumentFileInfo documentFileInfo = _IReportService.Download(FileType_Id, Client_Id, outputTablesList, Report_Id,string.Empty, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate, IsAgentId_Include);
                //var bytes = Convert.FromBase64String(documentFileInfo.Base64String);

                //System.IO.File.WriteAllBytes(@"D:\Temp\Test.xlsx", bytes);
                //byte[] zipBytes = Convert.FromBase64String(documentFileInfo.Base64String);

                //string zipPath = @"D:\Temp\Response.zip";

                //System.IO.File.WriteAllBytes(zipPath, zipBytes);
                

                if (!string.IsNullOrEmpty(documentFileInfo?.Base64String))
                {
                    serviceResponse.Status = true;
                    serviceResponse.Data = documentFileInfo;
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "There was an error while processing your request.";
                }
                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
                return this.StatusCode((int)HttpStatusCode.InternalServerError, serviceResponse);
            }
        }
        [HttpGet("DownloadLog")]
        public IActionResult DownloadLog([Required] DateTime date, bool download = false)
        {
            try
            {
                FileLogger.LogInformation($"CommonServiceController: DownloadLog Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized("Authorization token missing.");
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    return Unauthorized("Invalid token.");
                }

                string fileName = $"{date:ddMMMyyyy}.txt";
                string filePath = Path.Combine(MasterLookup.LogsFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Log file not found.");
                }

                if (download)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", fileName);
                }

                string fileContent = System.IO.File.ReadAllText(filePath);
                return Content(fileContent, "text/plain");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonServiceController: DownloadLog: Date: {date}: Download: {download}: Error While Processing: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("GenerateAndUploadCommissionDocuments")]
        public IActionResult GenerateAndUploadCommissionDocuments([FromQuery] long Report_Id, [FromQuery] long Client_Id, [FromQuery] byte FileType_Id, [FromBody] List<JsonElement> selectedInvoicesList)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Authorization token missing.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid token.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                DateTime AsOfDate = DateTime.Now;
                bool IsAgentId_Include = true;
                var SQLInputParameters = new List<LU_SQLInputParameters>();
                SQLInputParameters.AddRange(new List<LU_SQLInputParameters>
                {
                    new() {
                        ParameterName = "As_Of_Date",
                        InputValue = AsOfDate.ToString("MM-dd-yyyy")
                    },
                    new() {
                        ParameterName = "OutstandingBalances",
                        InputValue = "true"
                    },
                    new() {
                        ParameterName = "Simplified",
                        InputValue = "true"
                    },
                    new() {
                        ParameterName = "Client_Id",
                        InputValue = Convert.ToString(Client_Id)
                    }
                });

                var selectedInvoices = selectedInvoicesList.Select(x => x.Deserialize<SelectedInvoice>()).ToList();
                int? totalRecords;
                int PageSize = MasterLookup._DefaultPageSize;
                int PageIndex = MasterLookup._DefaultPageNumber;
                int? TotalRecords = 0;

                if (selectedInvoices?.Count == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invoices are Required.";
                    return this.StatusCode((int)HttpStatusCode.BadRequest, serviceResponse);
                }

                if (Report_Id == MasterLookup._Reports.InvoiceProduction || Report_Id == MasterLookup._Reports.Payment
                || Report_Id == MasterLookup._Reports.Transaction || Report_Id == MasterLookup._Reports.AccountReceivable
                || Report_Id == MasterLookup._Reports.AccountPayable || Report_Id == MasterLookup._Reports.TaxFiling)
                {
                    SQLInputParameters.AddRange(new List<LU_SQLInputParameters>
                    {
                        new() {
                            ParameterName = "PageSize",
                            InputValue = Convert.ToString(PageSize)
                        },
                        new() {
                            ParameterName = "PageIndex",
                            InputValue = Convert.ToString(PageIndex)
                        },
                        new() {
                            ParameterName = "TotalRecords",
                            InputValue = "0"
                        }
                    });
                }
                List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
                LU_ReportTemplates ReportTemplate = LookupUtility.GetAllReportTemplates().FirstOrDefault(x => x.Report_Id == Report_Id);
                if (ReportTemplate != null)
                {
                    outputTablesList = reportsUtility.GetReportOutputTablesWithoutTotalrecords(Report_Id, SQLInputParameters, FileType_Id, Client_Id);
                }
                else
                {
                    outputTablesList = reportsUtility.GetReportOutputTables(Report_Id, SQLInputParameters, out totalRecords, FileType_Id);
                }

                var asOfDateValue = SQLInputParameters.Where(x => x.ParameterName == "As_Of_Date").Select(x => x.InputValue).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(asOfDateValue))
                {
                    if (DateTime.TryParseExact(asOfDateValue, new[] { "MM/dd/yyyy", "M/d/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        AsOfDate = parsedDate;
                    }
                    else
                    {
                        AsOfDate = DateTime.Now;
                    }
                }

                bool IsOutstandingInvoices = false;
                bool IsSimplifiedView = false;
                if (ReportTemplate != null)
                {
                    if (SQLInputParameters.Count > 0)
                        IsOutstandingInvoices = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "OutstandingBalances").Select(x => x.InputValue).FirstOrDefault());

                    if (SQLInputParameters.Count > 0 && ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectBilledAccountStatement)
                    {
                        IsSimplifiedView = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "Simplified").Select(x => x.InputValue).FirstOrDefault());
                    }
                }

                DocumentFileInfo documentFileInfo = MakePaymentDownload(Client_Id, FileType_Id, outputTablesList, Report_Id, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate, IsAgentId_Include, selectedInvoices);
                if (!string.IsNullOrEmpty(documentFileInfo?.Base64String))
                {
                    List<DocumentFileInfo> docs = ExtractDocuments(documentFileInfo);
                    if (docs != null && docs.Count > 0 && Client_Id != null)
                    {
                        EI_ConnectUtility eiUtility = new();
                        serviceResponse = eiUtility.CommissionDocumentsUpload(Client_Id, docs);
                    }
                    else
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Message = "No Document Available";
                    }
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "There was an error while processing your request.";
                }
                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
                return this.StatusCode((int)HttpStatusCode.InternalServerError, serviceResponse);
            }
        }

        [HttpPost("CreateVendorAndExternalBill")]
        public IActionResult CreateVendorAndExternalBill([FromQuery] long Client_Id, [FromQuery] DateTime PaymentToDate, [FromQuery] string EntityName,
            [FromQuery] long Payment_Id, [FromQuery] long User_Id, [FromQuery] byte EntityType_Id, [FromQuery] long Entity_Id, [FromQuery] long Bill_Id, [FromQuery] byte Direction_Id,
            [FromBody] List<JsonElement> requestPolicyDetailsList, [FromQuery] string MemoDescription = "")
        {
            ServiceResponse serviceResponse = new ServiceResponse
            {
                Status = true,
                Message = "Vendor and External Bill Processed Successfully."
            };
            try
            {

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Authorization token missing.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Invalid token.";
                    return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
                }
                
                var clients = _accountRepository.GetClients(Client_Id);
                var billDetail = new Accounting.Entities.DB.Transactions.Bill
                {
                    PaymentToDate = PaymentToDate,
                    MemoDescription = MemoDescription,
                    EntityName = EntityName,
                    EntityType_Id = EntityType_Id,
                    Entity_Id = Entity_Id,
                    Bill_Id = Bill_Id
                };

                SqlTransaction sqlTransaction1 = _baseRepository.GetNewTransaction();
                Payments OutgoingPayment = new Payments();
                OutgoingPayment = _paymentsRepository.GetPaymentDetails(Payment_Id);
                var policyDetailsList = requestPolicyDetailsList.Select(x => x.Deserialize<PolicyDetails>()).ToList();
                if (policyDetailsList?.Count == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Request PolicyDetails List not found.";
                    return this.StatusCode((int)HttpStatusCode.BadRequest, serviceResponse);
                }

                if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                {
                    try
                    {
                        OutgoingPayment.PolicyDetailsList = policyDetailsList;
                        OutgoingPayment.TxnDate = DateTime.Now;
                        OutgoingPayment.PaymentDate = billDetail.PaymentToDate;
                        OutgoingPayment.MemoDescription = billDetail.MemoDescription;
                        QBOUtility qBOUtility = new QBOUtility(Client_Id, User_Id, MasterLookup._Services.Intuit, clients.AccountingType_Id);
                        if (billDetail.EntityType_Id == MasterLookup._EntityTypes.State)
                        {
                            billDetail.EntityName = $"State of {billDetail.EntityName}";
                            StateDetails statedetails = _policyDetailsRepository.GetStateDetailsByName(billDetail.EntityName, Client_Id);
                            billDetail.Entity_Id = statedetails.StateDetail_Id;
                        }
                        var externalVendorEntityReference = _externalVendorEntityReferenceRepository.GetExternalVendorEntityReference(Client_Id, billDetail.EntityType_Id, billDetail.Entity_Id, clients.AccountingService_Id);
                        bool IsVendorResolved = false;
                        if (externalVendorEntityReference == null)
                        {
                            ReadByQueryRes vendorByName = qBOUtility.ReadVendorByDisplayName("EI - " + billDetail.EntityName);
                            if (vendorByName?.QueryResponse?.Vendor == null)
                            {
                                Entities.External.QBO.Vendor.CreateVendorRes createVendorRes = qBOUtility.CreateVendor(billDetail.EntityType_Id, billDetail.Entity_Id, billDetail.EntityName, Client_Id);
                                if (!string.IsNullOrWhiteSpace(createVendorRes?.Vendor?.Id))
                                {
                                    externalVendorEntityReference = new ExternalVendorEntityReference
                                    {
                                        Client_Id = Client_Id,
                                        EntityType_Id = billDetail.EntityType_Id,
                                        Entity_Id = billDetail.Entity_Id,
                                        Service_Id = clients.AccountingService_Id,
                                        ExternalRefId = createVendorRes.Vendor.Id
                                    };

                                    externalVendorEntityReference = _externalVendorEntityReferenceRepository.InsertExternalVendorEntityReference(sqlTransaction1, externalVendorEntityReference);
                                    if (externalVendorEntityReference?.ExternalEntityReference_Id > 0)
                                    {
                                        IsVendorResolved = true;
                                    }
                                }
                                else
                                {
                                    if (sqlTransaction1?.Connection != null)
                                    {
                                        sqlTransaction1.Rollback();
                                    }

                                    // serviceResponse = DeletePaymentDetails(Payment_Id, Client_Id, User_Id);
                                    serviceResponse.Status = false;
                                    serviceResponse.Message += $"Error while creating a vendor in QBO.\n{createVendorRes?.Fault?.Error?[0]?.Message}";
                                    return Ok(serviceResponse);
                                }
                            }
                            else
                            {
                                Accounting.Entities.External.QBO.Vendor.Vendor vendor = null;
                                if (vendorByName?.QueryResponse?.Vendor?.Any() == true)
                                {
                                    vendor = vendorByName?.QueryResponse?.Vendor?.FirstOrDefault();
                                    externalVendorEntityReference = new ExternalVendorEntityReference();
                                    externalVendorEntityReference.Client_Id = Client_Id;
                                    externalVendorEntityReference.EntityType_Id = billDetail.EntityType_Id;
                                    externalVendorEntityReference.Entity_Id = billDetail.Entity_Id;
                                    externalVendorEntityReference.Service_Id = clients.AccountingService_Id;
                                    externalVendorEntityReference.ExternalRefId = vendor?.Id;
                                    externalVendorEntityReference = _externalVendorEntityReferenceRepository.InsertExternalVendorEntityReference(sqlTransaction1, externalVendorEntityReference);
                                    if (externalVendorEntityReference.ExternalEntityReference_Id > 0)
                                    {
                                        IsVendorResolved = true;
                                    }
                                }
                                else
                                {
                                    if (sqlTransaction1?.Connection != null)
                                    {
                                        sqlTransaction1.Rollback();
                                    }
                                    // serviceResponse = DeletePaymentDetails(Payment_Id, Client_Id, User_Id);
                                    serviceResponse.Status = false;
                                    serviceResponse.Message += $"Error while creating a vendor in QBO.\n{vendorByName?.Fault?.Error?[0]?.Message}";
                                    return Ok(serviceResponse);
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(externalVendorEntityReference.ExternalRefId))
                            {
                                Entities.External.QBO.Vendor.UpdateVendorRes updateVendorRes = qBOUtility.UpdateVendor(externalVendorEntityReference.ExternalRefId, billDetail.EntityType_Id, billDetail.Entity_Id, billDetail.EntityName, Client_Id);
                                if (!string.IsNullOrWhiteSpace(updateVendorRes?.Vendor?.Id))
                                {
                                    IsVendorResolved = true;
                                }
                                else
                                {
                                    if (sqlTransaction1?.Connection != null)
                                    {
                                        sqlTransaction1.Rollback();
                                    }
                                    serviceResponse.Status = false;
                                    serviceResponse.Message += $"Error while updating a vendor in QBO.\n{updateVendorRes?.Fault?.Error?[0]?.Message}";
                                    return Ok(serviceResponse);
                                }
                            }
                        }
                        if (IsVendorResolved)
                        {
                            var externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, billDetail.Bill_Id);
                            if (externalBillReference == null)
                            {
                                Entities.External.QBO.Bill.CreateBillRes createBillRes = qBOUtility.CreateBill(OutgoingPayment, externalVendorEntityReference.ExternalRefId, Direction_Id);
                                if (!string.IsNullOrWhiteSpace(createBillRes?.Bill?.Id))
                                {
                                    externalBillReference = new ExternalBillReference();
                                    externalBillReference.Client_Id = Client_Id;
                                    externalBillReference.Service_Id = clients.AccountingService_Id;
                                    externalBillReference.SourceType_Id = MasterLookup._Sources.Bill;
                                    externalBillReference.Source_Id = billDetail.Bill_Id;
                                    externalBillReference.ExternalBillIdentifier = createBillRes.Bill.Id;
                                    externalBillReference = _externalBillReferenceRepository.InsertExternalBillReference(sqlTransaction1, externalBillReference);
                                    if (externalBillReference.ExternalBillReference_Id > 0)
                                    {
                                        sqlTransaction1.Commit();
                                    }
                                }
                                else
                                {
                                    if (sqlTransaction1?.Connection != null)
                                    {
                                        sqlTransaction1.Rollback();
                                    }
                                    serviceResponse.Status = false;
                                    serviceResponse.Message += $"Error while creating a Bill in QBO.\n{createBillRes?.Fault?.Error?[0]?.Message}";
                                    return Ok(serviceResponse);
                                }
                            }
                        }
                        else
                        {
                            if (sqlTransaction1?.Connection != null)
                            {
                                sqlTransaction1.Rollback();
                            }
                            serviceResponse.Status = false;
                            serviceResponse.Message += $"There was an error resolving Vendor (Vendor Entity_Id: {billDetail.Entity_Id}, EntityType_Id: {billDetail.EntityType_Id}";
                            return Ok(serviceResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (sqlTransaction1?.Connection != null)
                        {
                            sqlTransaction1.Rollback();
                        }
                        serviceResponse.Status = false;
                        serviceResponse.Message = ex.Message;
                        return Ok(serviceResponse);
                    }
                }
                else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                {
                    try
                    {
                        OutgoingPayment.PolicyDetailsList = policyDetailsList;
                        OutgoingPayment.TxnDate = DateTime.Now;
                        OutgoingPayment.PaymentDate = billDetail.PaymentToDate;
                        //string vendorId = $"EI-{bill.EntityName.Replace(" ", "-").ToUpper()}";
                        string entityName = billDetail.EntityName?.Trim() ?? string.Empty;
                        string vendorName;
                        if (entityName.Length > 15)
                        {
                            var words = entityName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            vendorName = words.Length >= 2
                                ? $"{words[0]}-{words[1]}"
                                : words[0];
                        }
                        else
                        {
                            vendorName = entityName.Replace(" ", "-");
                        }

                        string vendorId = $"EI-{vendorName.ToUpper()}";
                        SageUtility sageUtility = new SageUtility(Client_Id, User_Id, MasterLookup._Services.Sage, clients.AccountingType_Id);
                        if (billDetail.EntityType_Id == MasterLookup._EntityTypes.State)
                        {
                            billDetail.EntityName = $"State of {billDetail.EntityName}";
                            StateDetails statedetails = _policyDetailsRepository.GetStateDetailsByName(billDetail.EntityName, Client_Id);
                            billDetail.Entity_Id = statedetails.StateDetail_Id;
                        }
                        string readVendorResponse = sageUtility.ReadVendorByVendorId(vendorId);
                        bool vendorExists = readVendorResponse.Contains("<vendor>");
                        var externalVendorEntityReference = _externalVendorEntityReferenceRepository.GetExternalVendorEntityReference(Client_Id, billDetail.EntityType_Id, billDetail.Entity_Id, clients.AccountingService_Id);
                        bool IsVendorResolved = false;
                        if (externalVendorEntityReference == null && !vendorExists)
                        {
                            SageResponse createVendorRes = sageUtility.CreateVendor(vendorId, billDetail.EntityName);

                            if (!string.IsNullOrWhiteSpace(createVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo))
                            {
                                var recordNo = createVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo;
                                externalVendorEntityReference =
                                new ExternalVendorEntityReference
                                {
                                    Client_Id = Client_Id,
                                    EntityType_Id = billDetail.EntityType_Id,
                                    Entity_Id = billDetail.Entity_Id,
                                    Service_Id = clients.AccountingService_Id,
                                    ExternalRefId = createVendorRes.Operation.Result.Data.Vendor.RecordNo
                                };
                                externalVendorEntityReference = _externalVendorEntityReferenceRepository.InsertExternalVendorEntityReference(sqlTransaction1, externalVendorEntityReference);
                                IsVendorResolved = true;

                            }
                            else
                            {
                                if (sqlTransaction1?.Connection != null)
                                {
                                    sqlTransaction1.Rollback();
                                }

                                var error = createVendorRes?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
                                serviceResponse.Status = false;
                                serviceResponse.Message += $"Error while creating vendor in Sage.\n" + $"Description: {error?.Description2}";
                                return Ok(serviceResponse);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(externalVendorEntityReference?.ExternalRefId) && vendorExists)
                            {
                                SageResponse updateVendorRes = sageUtility.UpdateVendor(externalVendorEntityReference.ExternalRefId, billDetail.EntityName);

                                if (!string.IsNullOrWhiteSpace(updateVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo))
                                {
                                    var recordNo = updateVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo;
                                    IsVendorResolved = true;

                                }
                                else
                                {
                                    if (sqlTransaction1?.Connection != null)
                                    {
                                        sqlTransaction1.Rollback();
                                    }

                                    var error = updateVendorRes?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
                                    serviceResponse.Status = false;
                                    serviceResponse.Message += $"Error while updating vendor in Sage.\n" + $"ErrorNo: {error?.Errorno}\n" + $"Description: {error?.Description2}\n" + $"Correction: {error?.Correction}";
                                    return Ok(serviceResponse);
                                }
                                //IsVendorResolved = true;
                            }
                        }

                        if (IsVendorResolved)
                        {
                            var externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, billDetail.Bill_Id);
                            if (externalBillReference == null)
                            {
                                //var getAccountno = _chartofAccountsRepository.GetChartOfAccountsByService_Id();

                                SageResponse createBillRes = sageUtility.CreateBill(policyDetailsList, OutgoingPayment, vendorId);

                                if (!string.IsNullOrWhiteSpace(createBillRes?.Operation?.Result?.Data?.APBill?.RecordNo))
                                {
                                    externalBillReference = new ExternalBillReference();
                                    externalBillReference.Client_Id = Client_Id;
                                    externalBillReference.Service_Id = clients.AccountingService_Id;
                                    externalBillReference.SourceType_Id = MasterLookup._Sources.Bill;
                                    externalBillReference.Source_Id = billDetail.Bill_Id;
                                    externalBillReference.ExternalBillIdentifier = createBillRes.Operation.Result.Data.APBill.RecordNo;
                                    externalBillReference = _externalBillReferenceRepository.InsertExternalBillReference(sqlTransaction1, externalBillReference);
                                    if (externalBillReference.ExternalBillReference_Id > 0)
                                    {
                                        sqlTransaction1.Commit();
                                    }
                                    else
                                    {
                                        if (sqlTransaction1?.Connection != null)
                                        {
                                            sqlTransaction1.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Failed to save Sage Bill reference.";
                                        return Ok(serviceResponse);
                                    }
                                }
                                else
                                {
                                    if (sqlTransaction1?.Connection != null)
                                    {
                                        sqlTransaction1.Rollback();
                                    }
                                    var error = createBillRes?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
                                    //serviceResponse = DeletePaymentDetails(Payment_Id, Client_Id, User_Id);
                                    serviceResponse.Status = false;
                                    serviceResponse.Message += $"Error while creating Bill in Sage.\n" + $"ErrorNo: {error?.Errorno}\n" + $"Description: {error?.Description2}\n" + $"Correction: {error?.Correction}";
                                    return Ok(serviceResponse);
                                }
                            }
                        }
                        else
                        {
                            if (sqlTransaction1?.Connection != null)
                            {
                                sqlTransaction1.Rollback();
                            }
                            //serviceResponse = DeletePaymentDetails(Payment_Id, Client_Id, User_Id);
                            serviceResponse.Status = false;
                            serviceResponse.Message += $"There was an error resolving Vendor (Vendor Entity_Id: {billDetail.Entity_Id}, EntityType_Id: {billDetail.EntityType_Id}";
                            return Ok(serviceResponse);
                        }

                    }
                    catch (Exception ex)
                    {
                        if (sqlTransaction1?.Connection != null)
                        {
                            sqlTransaction1.Rollback();
                        }
                        //serviceResponse = DeletePaymentDetails(Payment_Id, Client_Id, User_Id);
                        serviceResponse.Status = false;
                        serviceResponse.Message = ex.Message;
                        return Ok(serviceResponse);
                    }
                }
                else
                {
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Vendor and External Bill was skipped as Client was not Associated with any External Accounting Services.";
                    return Ok(serviceResponse);
                }

                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
                return this.StatusCode((int)HttpStatusCode.InternalServerError, serviceResponse);
            }
        }
        [HttpPost("UnapplyPayment")]
        public async Task<ActionResult<ServiceResponse>> ProcessUnapplyPayment(long paymentId, [FromQuery] long Client_Id, [FromQuery] long User_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Authorization token missing.";
                return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
            }

            if (token != MasterLookup.ApplicationAuthToken)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Invalid token.";
                return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
            }

            CreateJournalRes createJournalResponse = new CreateJournalRes();
            SqlTransaction transaction = _baseRepository.GetNewTransaction();

            try
            {
                var paymentLineItems = _paymentsRepository.GetPaymentApplicationLineItemsByPayment_Id(paymentId);
                if (paymentLineItems == null || paymentLineItems.Count == 0)
                {
                    return new ServiceResponse
                    {
                        Status = false,
                        Message = $"No transactions found with Payment ID {paymentId}"
                    };
                }
                var groupedLineItems = paymentLineItems.GroupBy(x => x.InvoiceLineItem_Id).ToList();
                var clients = _accountRepository.GetClients(Client_Id);
                foreach (var group in groupedLineItems)
                {
                    var combinedItem = new PaymentApplicationLineItems
                    {
                        InvoiceLineItem_Id = group.Key,
                        AppliedAmount = group.Sum(x => x.AppliedAmount),
                        UnAppliedAmount = group.Sum(x => x.AppliedAmount),
                        PaymentApplication_Id = group.First().PaymentApplication_Id,
                        AccountingType_Id = group.First().AccountingType_Id
                    };
                    var response = UnApplyPayment(combinedItem, clients,Client_Id, User_Id, group.ToList(), true);
                    if (!response.Status)
                    {
                        transaction.Rollback();
                        return response;
                    }
                }
                if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                {
                    var qboUtility = new QBOUtility(Client_Id, User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                    var journalEntries = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, paymentId);
                    var journalEntry = journalEntries?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();
                    if (journalEntry == null)
                    {
                        transaction.Rollback();
                        return new ServiceResponse
                        {
                            Status = false,
                            Message = "No previous journal entry found."
                        };
                    }
                    var payments = _paymentsRepository.GetPaymentDetails(paymentId);
                    var groupedSummary = groupedLineItems
                        .Select(g => new PaymentApplicationLineItems
                        {
                            InvoiceLineItem_Id = g.Key,
                            AppliedAmount = g.Sum(x => x.AppliedAmount),
                            PaymentApplication_Id = g.First().PaymentApplication_Id,
                            AccountingType_Id = g.First().AccountingType_Id
                        }).ToList();
                    createJournalResponse = qboUtility.CreateUnapplyPaymentJournalEntry(transaction, payments, groupedSummary, journalEntry, false);
                    if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                    {
                        transaction.Rollback();
                        return new ServiceResponse
                        {
                            Status = false,
                            Message = "Cannot sync payment with Intuit!"
                        };
                    }
                }

                transaction.Commit();
                return new ServiceResponse
                {
                    Status = true,
                    Message =  "Payments unapplied successfully."
                };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();

                return new ServiceResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }
        [HttpPost("ApplyPayment")]
        public async Task<ActionResult<ServiceResponse>> ProcessApplyPayment(long Payment_Id, [FromQuery] long Client_Id, [FromQuery] long User_Id, [FromQuery] byte Direction_Id, [FromQuery] bool IsOverride, [FromQuery] bool AllowInvoiceCredit, [FromQuery] bool AllowCredit, [FromBody] List<SelectedInvoiceDetailDTO> checkedInvoiceDetails)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Authorization token missing.";
                return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
            }

            if (token != MasterLookup.ApplicationAuthToken)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Invalid token.";
                return this.StatusCode((int)HttpStatusCode.Unauthorized, serviceResponse);
            }

            double RemainingBalance = 0;
            bool Paid = false;
            DateTime? PaidOn = null;
            Clients clients = _accountRepository.GetClients(Client_Id);
            List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();

            try
            {
                if (Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = $"No payments were associated.";
                    return serviceResponse;
                }
                else
                {
                    if (checkedInvoiceDetails?.Count() == 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Data = "Please select at least one full or partial invoice to apply payment";
                        return serviceResponse;
                    }
                    else
                    {
                        Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
                        //checkedInvoiceDetails = PrepareInvoicePayload(checkedInvoiceDetails);
                        double PaymentsRemainingAmount = payments.RemainingAmount;
                        bool hasPositive = checkedInvoiceDetails.Any(x => x.Amount > 0);
                        bool hasNegative = checkedInvoiceDetails.Any(x => x.Amount < 0);
                        bool allPositive = hasPositive && !hasNegative;
                        bool allNegative = hasNegative && !hasPositive;
                        bool hasMixed = hasPositive && hasNegative;

                        bool isAR = Direction_Id == MasterLookup._Directions.AR;
                        bool isAP = Direction_Id == MasterLookup._Directions.AP;
                        bool isIncoming = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming;
                        bool isOutgoing = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing;
                        double netAmount = (double)checkedInvoiceDetails.Sum(x => Convert.ToDecimal(x.Amount));
                        bool isNetPositive = netAmount > 0;
                        bool isNetNegative = netAmount < 0;
                        if (((isAR && isNetNegative && isIncoming) || (isAR && isNetNegative && isIncoming) || (isAP && isNetNegative && isOutgoing) || (isAP && isNetPositive && isIncoming)) && !AllowCredit)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Credit will make remaining amount exceed the original payment amount. Do you want to continue?";
                            serviceResponse.Data = new { AllowCredit = true };
                            return serviceResponse;
                        }
                        bool isNegativeContext = (isIncoming && isAP) || (isOutgoing && isAR);
                        decimal displaySign = isNegativeContext ? -1 : 1;
                        decimal effectivePaymentBalance = (decimal)payments.RemainingAmount;
                        decimal effectiveAppliedTotal = checkedInvoiceDetails.Sum(x => (decimal)x.Amount) * displaySign;
                        if (effectivePaymentBalance < effectiveAppliedTotal)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "The amount selected exceeds the payment amount.";
                            return serviceResponse;
                        }

                        var allInvoiceDetailIds = checkedInvoiceDetails?.Where(x => x.InvoiceDetail_Ids != null && x.InvoiceDetail_Ids?.Any() == true)?
                                                  .SelectMany(x => x.InvoiceDetail_Ids)?
                                                  .Distinct()?
                                                  .ToList();
                        string invoiceDetailIds = string.Join(",", allInvoiceDetailIds);
                        bool IsEntityMismatch = _invoiceRepository.ValidateInvoiceEntityMismatch(invoiceDetailIds, payments.Entity_Id, (payments.EntityType_Id ?? 0));

                        if (IsEntityMismatch && !IsOverride)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Payment entity type must match the invoice entity type.";
                            serviceResponse.Data = new
                            {
                                EntityMismatch = true
                            };

                            return serviceResponse;
                        }
                        bool invoiceOverApplied = checkedInvoiceDetails?.Any(x => Math.Abs(x.Amount) > Math.Abs(x.RemainingBalance)) == true;
                        if (invoiceOverApplied && !AllowInvoiceCredit)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "You are about to apply more than the original amount on this invoice. Only do so if you intend to have a credit on this invoice.";
                            serviceResponse.Data = new
                            {
                                AllowInvoiceCredit = true
                            };

                            return serviceResponse;
                        }
                        if (clients.AccountingService_Id > 0)
                        {
                            List<Accounting.Entities.External.QBO.Journal.Error> errors = _paymentsRepository.ValidateInvoiceCOAConfiguration(clients.Client_Id, invoiceDetailIds, clients.AccountingService_Id, Payment_Id);
                            if (errors?.Count > 0)
                            {
                                var groupedErrors = errors
                                                    .GroupBy(x => x.Message)
                                                    .Select(g =>
                                                        $"{g.Key}:{Environment.NewLine}" +
                                                        string.Join(
                                                            Environment.NewLine,
                                                            g.Select(e => $"• {e.Detail}")
                                                        )
                                                    );

                                serviceResponse.Status = false;
                                serviceResponse.Message =
                                    "One or more invoice line items are not properly configured in the Chart of Accounts."
                                    + Environment.NewLine + Environment.NewLine
                                    + string.Join(Environment.NewLine + Environment.NewLine, groupedErrors);
                                return serviceResponse;
                            }
                        }
                        try
                        {
                            string PostJson = JsonConvert.SerializeObject(checkedInvoiceDetails);
                            bool IsNegative = (checkedInvoiceDetails.Sum(x => Convert.ToDecimal(x.Amount)) < 0 && ((isAR && isOutgoing) || (isAP && isIncoming)));
                            PaymentApplicationLineItemsList = _paymentsRepository.ApplyPayments(clients.Client_Id, PostJson, Payment_Id, Direction_Id, IsNegative);
                        }
                        catch (Exception ex)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = ex.Message;
                            return serviceResponse;
                        }
                        SqlTransaction PaymentsTransaction = _baseRepository.GetNewTransaction();
                        //    if (PaymentsTransaction == null)
                        //    {
                        //        serviceResponse.Status = false;
                        //        serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                        //        return serviceResponse;
                        //    }
                        try
                        {
                            if (PaymentApplicationLineItemsList.Count() > 0)
                            {
                                string paymentApplicationLineitemJson = JsonConvert.SerializeObject(PaymentApplicationLineItemsList.Select(x => new { PaymentApplicationLineItem_Id = x.PaymentApplicationLineItem_Id }).ToList());
                                List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();

                                if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                {
                                    QBOUtility qBOUtility = new QBOUtility(Client_Id, User_Id, clients.AccountingService_Id, clients.AccountingType_Id);

                                    //UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
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

                                        string Result = _paymentsRepository.UnApplyPayments(clients.Client_Id, Direction_Id, paymentApplicationLineitemJson);
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = createJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {createJournalRes.Fault.Error[0].Detail}" : $"Unable to sync to Intuit! {Result}";
                                        return serviceResponse;
                                    }
                                }
                                else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                                {
                                    SageUtility sageUtility = new SageUtility(Client_Id, User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                    var createJournalRes = Task.Run(() =>
                                    sageUtility.CreateJournalEntry_Intacct(
                                        PaymentsTransaction,
                                        payments,
                                        PaymentApplicationLineItemsList,
                                        journalEntry,
                                        true
                                    )
                                ).Result;
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
                                        string Result = _paymentsRepository.UnApplyPayments(clients.Client_Id, Direction_Id, paymentApplicationLineitemJson);
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = $"Unable to sync to Sage. {Result}";
                                        return serviceResponse;
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
                        }

                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Applied payment successfully.";
                    return serviceResponse;
                }

            }
            catch (Exception ex)
            {

                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;

        }
        private DocumentFileInfo Download(PolicyDetails _PolicyDetails)
        {
            DocumentFileInfo documentFileInfo = new DocumentFileInfo();
            documentFileInfo.Name = $"Invoice-{_PolicyDetails.Invoices.InvoiceNo}.pdf";
            var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = _PolicyDetails,
            };
            byte[] CarrierLogo = null;
            var SettingsLogo = CommonUtility.GetEntitySettingValue<byte[]>((_PolicyDetails?.Client_Id ?? 0), MasterLookup._EntityTypes.Carrier, _PolicyDetails.CarrierDetail_Id, MasterLookup._Settings.InvoiceCarrierLogo);
            if (SettingsLogo?.Count() > 0)
            {
                CarrierLogo = SettingsLogo;
            }
            var htmlContent = this.view.Render("Invoices/InvoicePDF", viewDataDictionary);
            var hasDefaultTemplate = LookupUtility.GetClientSettings(_PolicyDetails?.Client_Id ?? 0, MasterLookup._ClientSettings.RTWHasDefaultTemplate);
            if (!string.IsNullOrEmpty(hasDefaultTemplate))
            {
                var Footer = LookupUtility.GetClientSettings(_PolicyDetails?.Client_Id ?? 0, MasterLookup._ClientSettings.InvoiceFooter);
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
                    documentFileInfo.ContentType = "application/pdf";
                    documentFileInfo.Base64String = response.Data.ToString();
                    documentFileInfo.Bytes = Convert.FromBase64String(response.Data.ToString());
                }
            }
            else
            {
                byte[] coverPayPaymentLogo = null;
                if (!string.IsNullOrEmpty(_PolicyDetails.ExternalPaymentLink))
                {
                    var coverPayimagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "CoverPay", "coverpay-payment-logo.png");
                    coverPayPaymentLogo = System.IO.File.ReadAllBytes(coverPayimagePath);
                }

                byte[] bytes = _IPDFOptions.ExportToPdf(htmlContent, _PolicyDetails.Clients.Logo, CarrierLogo, coverPayPaymentLogo, _PolicyDetails.ExternalPaymentLink);
                documentFileInfo.Bytes = bytes;
                documentFileInfo.ContentType = "application/pdf";
                documentFileInfo.Base64String = Convert.ToBase64String(bytes);
            }

            return documentFileInfo;
        }

        [NonAction]
        public DocumentFileInfo MakePaymentDownload(long client_Id, byte FileType_Id, List<ReportOutputTables> reportOutputTables, long Report_Id, LU_ReportTemplates ReportTemplate = null, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null, bool IsAgentId_Include = false, List<SelectedInvoice> selectedInvoices = null)
        {
            if (selectedInvoices == null || !selectedInvoices.Any())
                return new DocumentFileInfo();
            var selectedInvoiceIds = new HashSet<long>(selectedInvoices.Select(x => x.Invoice_Id));

            DataSet ds = new DataSet();
            DataSet dsAgency = new DataSet();
            byte[]? content = null;
            var FileTypesList = LookupUtility.GetFileTypes();
            LU_FileTypes FileType = FileTypesList?.Where(c => c.FileType_Id == FileType_Id)?.FirstOrDefault();
            DocumentFileInfo documentFileInfo = new DocumentFileInfo();
            List<DocumentFileInfo> listDocumentFileInfo = new List<DocumentFileInfo>();
            if (FileType != null)
            {
                documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.{FileType.Extension}";
                documentFileInfo.FileType_Id = FileType.FileType_Id;
                int counter = 0;
                foreach (var reportOutputTable in reportOutputTables)
                {
                    counter++;
                    string tableName = reportOutputTable.Name;
                    int count = reportOutputTables.Where(c => c.OutputDataTable.TableName == tableName)?.Count() ?? 0;
                    if (count > 0)
                    {
                        tableName += $"_{counter}";
                    }
                    reportOutputTable.OutputDataTable.TableName = tableName;
                    ds.Tables.Add(reportOutputTable.OutputDataTable.Copy());
                }
                if (FileType.FileType_Id == MasterLookup._LUFileTypes.PDF || FileType.FileType_Id == MasterLookup._LUFileTypes.AgentSpecificPDF)
                {
                    if (ds?.Tables.Count > 0)
                    {
                        if (ReportTemplate != null)
                        {
                            try
                            {
                                if (FileType.FileType_Id == MasterLookup._LUFileTypes.AgentSpecificPDF)
                                {
                                    var tableAgency = reportOutputTables[0];

                                    var tablePolicies = reportOutputTables[1];

                                    var tableInvoices = reportOutputTables[2];

                                    var table4 = reportOutputTables[3];

                                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));

                                    List<(string fileName, byte[] fileBytes)> pdfFiles = new List<(string, byte[])>();

                                    foreach (var agencyGroup in agencyGroups)
                                    {

                                        long agentDetailId = agencyGroup.Key;

                                        var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
                                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId)
                                            .ToList();

                                        if (!policiesForAgent.Any())
                                            continue;

                                        var validPolicyIds = table4.DataTable.AsEnumerable()
                                            .Where(r => !r.IsNull("PolicyDetails_Id"))
                                            .Select(r => r.Field<long>("PolicyDetails_Id"))
                                            .ToHashSet();

                                        var filteredPolicies = policiesForAgent
                                            .Where(r => validPolicyIds.Contains(r.Field<long>("Id")))
                                            .ToList();

                                        if (!filteredPolicies.Any())
                                            continue;

                                        var policiesTableClone = tablePolicies.DataTable.Clone();
                                        foreach (var row in filteredPolicies)
                                            policiesTableClone.ImportRow(row);

                                        var policyNos = filteredPolicies
                                            .Select(r => r.Field<string>("PolicyNumber"))
                                            .ToHashSet();

                                        var policyIds = filteredPolicies
                                            .Select(r => r.Field<long>("Id"))
                                            .ToHashSet();

                                        var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                                            .Where(r => !r.IsNull("AgentDetail_Id") &&
                                                        r.Field<long>("AgentDetail_Id") == agentDetailId &&
                                                        !r.IsNull("PolicyNumber") &&
                                                        policyNos.Contains(r.Field<string>("PolicyNumber")))
                                            .ToList();

                                        if (!invoicesForAgent.Any())
                                            continue;

                                        var invoicesTableClone = tableInvoices.DataTable.Clone();
                                        foreach (var row in invoicesForAgent)
                                            invoicesTableClone.ImportRow(row);

                                        var filteredTable4Rows = table4.DataTable.AsEnumerable()
                                            .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")))
                                            .ToList();

                                        if (!filteredTable4Rows.Any())
                                            continue;

                                        var table4Clone = table4.DataTable.Clone();
                                        foreach (var row in filteredTable4Rows)
                                            table4Clone.ImportRow(row);

                                        var agencyTableClone = tableAgency.DataTable.Clone();
                                        foreach (var row in agencyGroup)
                                            agencyTableClone.ImportRow(row);

                                        var filteredRows = table4Clone.AsEnumerable().Where(row => selectedInvoiceIds.Contains(row.Field<long>("Invoice_Id")));
                                        bool IsPaymentBasedDirectBillCommissionEnabled = Convert.ToBoolean(LookupUtility.GetClientSettings(client_Id, MasterLookup._ClientSettings.EasternDirectBillCommissionStatementPaymentBased));
                                        DataTable filteredTable;

                                        if (filteredRows.Any())
                                        {
                                            filteredTable = filteredRows.CopyToDataTable();
                                        }
                                        else
                                        {
                                            filteredTable = table4Clone.Clone();
                                        }
                                        if (IsPaymentBasedDirectBillCommissionEnabled)
                                        {
                                            ApplyPaymentBasedDirectBillCommission(filteredTable);
                                        }
                                        var agencyOutputTables = new List<ReportOutputTables>
                                        {
                                            new ReportOutputTables { DataTable = agencyTableClone },
                                            new ReportOutputTables { DataTable = policiesTableClone },
                                            new ReportOutputTables { DataTable = invoicesTableClone },
                                            new ReportOutputTables { DataTable = filteredTable }
                                        };

                                        if (table4Clone.Rows.Count > 0)
                                        {
                                            // Build HTML
                                            Clients clients = _accountRepository.GetClients(client_Id);
                                            string htmlContent = MakePaymentsBuildReportHtmlFromTemplate(client_Id, Report_Id, agencyOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
                                            if (htmlContent != "")
                                            {
                                                string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                                                string year = DateTime.Now.Year.ToString();

                                                string headerHtml = $@"<table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;"">
                                    <tbody><tr><td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt;"">{month} {year}</td>
                                    <td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt;"">{ReportTemplate.Name}</td></tr></tbody></table>
                                    <style>body {{margin: 0px;padding: 0px;}}</style>";

                                                string footerHtml = @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>";

                                                var request = new PDFRequest
                                                {
                                                    BodyHTMLString = htmlContent,
                                                    HeaderHTMLString = headerHtml,
                                                    FooterHTMLString = footerHtml,
                                                    IsCoverPageExcluded = false
                                                };

                                                string serviceUrl = MasterLookup._PDFCoverterAPIURL;
                                                ServiceLogInfo logInfo = new ServiceLogInfo();
                                                ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(logInfo, serviceUrl, request, typeof(ServiceResponse), true);

                                                if (response.Status && response.Data != null)
                                                {
                                                    byte[] pdfBytes = Convert.FromBase64String(response.Data.ToString());
                                                    string agencyName = agencyGroup.First().Field<string>("Name")?.Replace(" ", "_") ?? $"Agency_{agentDetailId}";
                                                    var Agent_ID = _policyDetailsRepository.GetAgentDetails(agentDetailId).AgentReference_Id;
                                                    //string fileName = $"{agencyName}_{ReportTemplate.Name}_{DateTime.Now:yyyyMMddHHmmss}_{Agent_ID}.pdf";                                                  
                                                    var datePart = (AsOfDate ?? DateTime.Now).ToString("yyyyMMdd");
                                                    string fileName = "";
                                                    if (IsAgentId_Include)
                                                    {
                                                        fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}_{Agent_ID}.pdf";
                                                    }
                                                    else
                                                    {
                                                        fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}.pdf";
                                                    }
                                                    pdfFiles.Add((fileName, pdfBytes));
                                                }
                                            }
                                        }
                                    }

                                    if (pdfFiles?.Count == 1)
                                    {
                                        foreach (var file in pdfFiles)
                                        {
                                            listDocumentFileInfo.Add(new DocumentFileInfo
                                            {
                                                Name = file.fileName,
                                                Base64String = file.fileBytes.ToString(),
                                                Bytes = file.fileBytes,
                                                ContentType = "application/pdf"

                                            });
                                        }

                                        content = _IZip.Compress(listDocumentFileInfo);
                                        documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.zip";
                                        documentFileInfo.Bytes = content;
                                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                                        documentFileInfo.ContentType = "application/zip";
                                    }
                                    else
                                    {
                                        foreach (var file in pdfFiles)
                                        {
                                            listDocumentFileInfo.Add(new DocumentFileInfo
                                            {
                                                Name = file.fileName,
                                                Base64String = file.fileBytes.ToString(),
                                                Bytes = file.fileBytes,
                                                ContentType = "application/pdf"

                                            });
                                        }

                                        content = _IZip.Compress(listDocumentFileInfo);
                                        documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.zip";
                                        documentFileInfo.Bytes = content;
                                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                                        documentFileInfo.ContentType = "application/zip";
                                    }
                                }
                                else
                                {
                                    //Clients clients = _accountRepository.GetClients(ReportTemplate.Client_Id);
                                    var HtmlContent = MakePaymentsBuildReportHtmlFromTemplate(client_Id, Report_Id, reportOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
                                    string Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                                    string Year = DateTime.Now.Year.ToString();
                                    string HeaderHTMLString = @" <table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;""><tbody><tr><td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt; "">" + Month + @" " + Year + @" </td><td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt; "">" + ReportTemplate.Name + "</td></tr></tbody></table><style>body {margin: 0px;padding: 0px;}</style>";

                                    //pDFConverterController.SetFooterForHtmlConverstion(PDFConverter.Document, 30, @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>", null);
                                    string FooterHTMLString = @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>";

                                    //byte[] PDFBytes = pDFConverterController.ConvertHtmlToPDF(PDFConverter, RenderedHtml);
                                    bool IsCoverPageExcluded = false;
                                    PDFRequest request = new PDFRequest()
                                    {
                                        BodyHTMLString = HtmlContent,
                                        HeaderHTMLString = HeaderHTMLString,
                                        FooterHTMLString = FooterHTMLString,
                                        IsCoverPageExcluded = IsCoverPageExcluded
                                    };
                                    string Service_Url = MasterLookup._PDFCoverterAPIURL;
                                    ServiceLogInfo serviceLogInfo = new ServiceLogInfo();
                                    ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, request, typeof(ServiceResponse), true);
                                    if (response.Status)
                                    {
                                        documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.pdf";
                                        documentFileInfo.Base64String = response.Data.ToString();
                                        documentFileInfo.ContentType = "application/pdf";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            List<DocumentFileInfo> documentFileInfoList = new List<DocumentFileInfo>();
                            foreach (DataTable table in ds?.Tables)
                            {
                                var viewDataDictionary = new ViewDataDictionary
                                    (
                            new EmptyModelMetadataProvider(),
                                 new ModelStateDictionary()
                                    )
                                {
                                    Model = table,
                                };
                                viewDataDictionary.Add("FontSize", 6);
                                var htmlContent = this.view.Render("Reports/ReportPDF", viewDataDictionary);
                                List<string> CSSReferenceList = new List<string> { "css/Reports/ReportPDF.css" };
                                byte[] bytes = _IPDFOptions.DownloadPDF(htmlContent, _IWebHostEnvironment.WebRootPath, CSSReferenceList);
                                if (bytes != null && bytes.Length > 0)
                                {
                                    DocumentFileInfo fileInfo = new DocumentFileInfo();
                                    fileInfo.Bytes = bytes;
                                    fileInfo.Name = $"{table.TableName}.pdf";
                                    documentFileInfoList.Add(fileInfo);
                                }
                                //content = _IPDFOptions.DownloadPDF(htmlContent, _IWebHostEnvironment.WebRootPath, CSSReferenceList);
                            }
                            if (documentFileInfoList?.Count == 1)
                            {
                                content = documentFileInfoList.First().Bytes;
                                documentFileInfo.ContentType = FileType.ContentType;
                            }
                            else
                            {
                                content = _IZip.Compress(documentFileInfoList);
                                documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.zip";
                                documentFileInfo.ContentType = "application/zip";
                            }
                            documentFileInfo.Bytes = content;
                            documentFileInfo.Base64String = Convert.ToBase64String(content);
                        }

                    }
                }
            }

            return documentFileInfo;
        }

        [NonAction]
        private string MakePaymentsBuildReportHtmlFromTemplate(long client_Id,long Report_Id, List<ReportOutputTables> reportOutputTablesList, LU_ReportTemplates ReportTemplate, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null)
        {
            var Clients = _accountRepository.GetClients(client_Id);
            string FinalHTMLAlongWithTemplate = "";
            string DynamicReportHTMLContent = "";
            string ClientLogo = $"<img src=\"data:image/jpg;base64,{Convert.ToBase64String(Clients.Logo)}\" alt=\"Client Logo\" style=\"height:45px;vertical-align: middle;\">";
            string ApplyFonts = "<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\" />\r\n<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin=\"anonymous\" />\r\n<link href=\"https://fonts.googleapis.com/css2?family=Poppins:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800;1,900&display=swap\" rel=\"stylesheet\" />";
            if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectBilledAccountStatement)
            {
                double totalNetAmount = 0;
                var tableAgency = reportOutputTablesList[0];
                var tablePolicies = reportOutputTablesList[1].DataTable;
                var table3 = reportOutputTablesList[2].DataTable;
                var table4 = reportOutputTablesList[3].DataTable;

                // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

                var agencyTableFinal = tableAgency.DataTable.Clone();
                var policiesTableFinal = tablePolicies.Clone();
                var invoicesTableFinal = table3.Clone();
                var table4Final = table4.Clone();
                var finalInvoiceTable = table4.Clone();
                if (IsOutstandingInvoices)
                {
                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                    .GroupBy(r => r.Field<long>("AgentDetail_Id"));


                    foreach (var agencyGroup in agencyGroups)
                    {
                        long agentDetailId = agencyGroup.Key;

                        var agencyTableTemp = tableAgency.DataTable.Clone();
                        foreach (var row in agencyGroup)
                            agencyTableTemp.ImportRow(row);

                        var policiesForAgent = tablePolicies.AsEnumerable()
                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                        var policiesTableTemp = tablePolicies.Clone();
                        foreach (var row in policiesForAgent)
                            policiesTableTemp.ImportRow(row);

                        var policyIds = policiesForAgent
                            .Where(r => !r.IsNull("Id"))
                            .Select(r => r.Field<long>("Id"))
                            .ToHashSet();

                        var invoicesForAgent = table3.AsEnumerable()
                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                        var invoicesTableTemp = table3.Clone();
                        foreach (var row in invoicesForAgent)
                            invoicesTableTemp.ImportRow(row);

                        var filteredTable4Rows = table4.AsEnumerable()
                            .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                        var table4Temp = table4.Clone();
                        foreach (var row in filteredTable4Rows)
                            table4Temp.ImportRow(row);

                        //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
                        var policiesToRemove = table4Temp.AsEnumerable()
                            .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
                            .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
                            .Select(g => g.Key)
                            .ToHashSet();

                        //if (policiesToRemove.Any())
                        //{
                        //    foreach (var row in policiesTableTemp.AsEnumerable().ToList())
                        //    {
                        //        if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                        //            policiesTableTemp.Rows.Remove(row);
                        //    }

                        //    foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
                        //    {
                        //        if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                        //            invoicesTableTemp.Rows.Remove(row);
                        //    }

                        //    foreach (var row in table4Temp.AsEnumerable().ToList())
                        //    {
                        //        if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                        //            table4Temp.Rows.Remove(row);
                        //    }
                        //}

                        if (table4Temp.Rows.Count > 0)
                        {
                            foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                            foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                            foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                            foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                        }
                    }

                    foreach (DataRow row in table4Final.Rows)
                    {
                        finalInvoiceTable.ImportRow(row);
                    }
                    var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
                        .Select(r => r.Field<long>("AgentDetail_Id")).Distinct().ToList();

                    var filteredAgencies = tableAgency.DataTable.AsEnumerable()
                        .Where(a => validAgencyIds.Contains(a.Field<long>("AgentDetail_Id")));

                    if (filteredAgencies.Any())
                        tableAgency.DataTable = filteredAgencies.CopyToDataTable();
                    else
                        tableAgency.DataTable = tableAgency.DataTable.Clone();

                }
                else
                {
                    finalInvoiceTable = table4;

                }

                if (IsSimplifiedView)
                {
                    var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

                    if (filteredRows.Any())
                        finalInvoiceTable = filteredRows.CopyToDataTable();
                }
                var commPaidRows = finalInvoiceTable.AsEnumerable()
                .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
                IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

                var groupedRows = finalInvoiceTable.AsEnumerable()
                        .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
                        .GroupBy(r => new
                        {
                            PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
                            InvoiceId = r.Field<long>("Invoice_Id")
                        })
                        .Select(g => g.First());

                DataTable tempGroupedRows;
                DataTable result;

                // if (groupedRows != null && groupedRows.Any())
                // {
                finalDistinct = groupedRows.Concat(commPaidRows);
                if (finalDistinct != null && finalDistinct.Any())
                {
                    result = finalDistinct.CopyToDataTable();

                    reportOutputTablesList[0].DataTable = tableAgency.DataTable;
                    reportOutputTablesList[3].DataTable = result;

                    if (reportOutputTablesList.Count > 3)
                    {
                        var invoiceDetails = result;
                        totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
                    }

                    var viewDataDictionary = new ViewDataDictionary
                                           (
                                   new EmptyModelMetadataProvider(),
                                        new ModelStateDictionary()
                                           )
                    {
                        Model = reportOutputTablesList,
                    };
                    viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;

                    DynamicReportHTMLContent = this.view.Render("Reports/DirectBilledAccountStatementDynamicReport", viewDataDictionary);
                    if (!IsSimplifiedView)
                    {
                        // Remove Comm. Paid to Date, Comm. Due columns + simplified-block
                        DynamicReportHTMLContent = MakePaymentsRemoveSimplifiedColumns(DynamicReportHTMLContent,
                            new[] { "Comm. Paid to Date", "Comm. Due" });
                    }
                    else
                    {
                        DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                    FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                        .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                       .Replace("{{clientAddress}}", Clients?.Address ?? "")
                       .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                       .Replace("{{clientLogo}}", ClientLogo)
                       //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                       .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                       .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
                       .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
                }
                //}
            }
            else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.AgencyBilledAccountStatement)
            {
                double totalNetAmount = 0;
                var tableAgency = reportOutputTablesList[0];
                var tablePolicies = reportOutputTablesList[1];
                var tableInvoices = reportOutputTablesList[2];
                var table4 = reportOutputTablesList[3];
                var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                    .GroupBy(r => r.Field<long>("AgentDetail_Id"));

                var agencyTableFinal = tableAgency.DataTable.Clone();
                var policiesTableFinal = tablePolicies.DataTable.Clone();
                var invoicesTableFinal = tableInvoices.DataTable.Clone();
                var table4Final = table4.DataTable.Clone();

                foreach (var agencyGroup in agencyGroups)
                {
                    long agentDetailId = agencyGroup.Key;

                    var agencyTableTemp = tableAgency.DataTable.Clone();
                    foreach (var row in agencyGroup)
                        agencyTableTemp.ImportRow(row);

                    var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
                        .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                    var policiesTableTemp = tablePolicies.DataTable.Clone();
                    foreach (var row in policiesForAgent)
                        policiesTableTemp.ImportRow(row);

                    var policyIds = policiesForAgent
                        .Where(r => !r.IsNull("Id"))
                        .Select(r => r.Field<long>("Id"))
                        .ToHashSet();

                    var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                        .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                    var invoicesTableTemp = tableInvoices.DataTable.Clone();
                    foreach (var row in invoicesForAgent)
                        invoicesTableTemp.ImportRow(row);

                    var filteredTable4Rows = table4.DataTable.AsEnumerable()
                        .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                    var table4Temp = table4.DataTable.Clone();
                    foreach (var row in filteredTable4Rows)
                        table4Temp.ImportRow(row);

                    if (table4Temp.Rows.Count > 0)
                    {
                        foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                        foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                        foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                        foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                    }
                }
                if (table4Final.Rows.Count > 0)
                {
                    List<ReportOutputTables> agencyOutputTables = new()
                    {
                        new() { DataTable = agencyTableFinal },
                        new() { DataTable = policiesTableFinal },
                        new() { DataTable = invoicesTableFinal },
                        new() { DataTable = table4Final }
                    };

                    if (agencyOutputTables.Count > 3)
                    {
                        var invoiceDetails = agencyOutputTables[3].DataTable;
                        totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("NetAmount")) ?? 0;
                    }
                    var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = agencyOutputTables,
                    };

                    DynamicReportHTMLContent = this.view.Render("Reports/AgencyBilledAccountStatementDynamicReport", viewDataDictionary);
                    FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                                                                           .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                                                                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
                                                                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                                                                           .Replace("{{clientLogo}}", ClientLogo)
                                                                           //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ServerDateFormat))
                                                                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                                                                           .Replace("{{dynamicAgentloopingLogic}}", DynamicReportHTMLContent)
                                                                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString())
                                                                           ;

                }
            }
            else
            {
                FinalHTMLAlongWithTemplate = "";
            }
            return FinalHTMLAlongWithTemplate;
        }

        [NonAction]
        public static string MakePaymentsRemoveSimplifiedColumns(string html, string[] columnHeadersToRemove)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables == null) return html;

            foreach (var table in tables)
            {
                var headerRow = table.SelectSingleNode(".//tr");
                if (headerRow == null) continue;

                var headers = headerRow.SelectNodes("./th|./td").ToList();
                try
                {

                    var indexesToRemove = headers
                        .Select((th, idx) => new { th, idx })
                        .Where(x => columnHeadersToRemove.Contains(x.th.InnerText.Trim()))
                        .Select(x => x.idx)
                        .OrderByDescending(i => i)
                        .ToList();

                    foreach (var row in table.SelectNodes(".//tr"))
                    {
                        var cells = row.SelectNodes("./th|./td");
                        if (cells == null) continue;

                        foreach (var colIndex in indexesToRemove)
                        {
                            if (colIndex < cells.Count)
                                cells[colIndex].Remove();
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        [NonAction]
        public List<DocumentFileInfo> ExtractDocuments(DocumentFileInfo fileInfo)
        {
            List<DocumentFileInfo> documents = new List<DocumentFileInfo>();

            if (fileInfo == null || fileInfo.Bytes == null || fileInfo.Bytes.Length == 0)
                return documents;

            if (fileInfo.ContentType == "application/zip" || fileInfo.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using (MemoryStream zipStream = new MemoryStream(fileInfo.Bytes))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        using (var entryStream = entry.Open())
                        using (var ms = new MemoryStream())
                        {
                            entryStream.CopyTo(ms);
                            byte[] fileBytes = ms.ToArray();
                            string[] parts = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                            long agentId = 0;
                            if (parts.Length > 0 && long.TryParse(parts[^1], out var parsedId))
                                agentId = parsedId;
                            documents.Add(new DocumentFileInfo
                            {
                                Name = entry.Name,
                                Bytes = fileBytes,
                                Base64String = Convert.ToBase64String(fileBytes),
                                ContentType = CommonUtility.GetMimeType(entry.Name),
                                Agent_Id = agentId
                            });
                        }
                    }
                }
            }
            else
            {
                documents.Add(fileInfo);
            }

            return documents;
        }

        [NonAction]
        public ServiceResponse UnApplyPayment(PaymentApplicationLineItems paymentApplicationLineItems, Clients clients, long Client_Id, long User_Id, List<PaymentApplicationLineItems> PaymentApplicationLineItems_List = null, bool Is_PaymentDeleted = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                double RemainingBalance = 0;
                bool Paid = false;
                DateTime? PaidOn = null;
                byte _AccountingType_Id = 0;
                if (paymentApplicationLineItems != null)
                {
                    InvoiceDetails invoiceDetails = _invoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                    if (invoiceDetails != null)
                    {
                        Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                        PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                        LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                        if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                        {
                            _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                        }
                        else
                        {
                            _AccountingType_Id = clients.AccountingType_Id;
                        }
                        paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                        LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                        RemainingBalance = invoiceDetails.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                        Paid = RemainingBalance == 0;
                        _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, invoiceDetails.Id, RemainingBalance, Paid);
                        if (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (invoices != null)
                            {
                                var invoiceDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(invoices.Id, LineItemsDirections.Direction_Id);

                                //RemainingBalance = invoices.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                                //Paid = RemainingBalance == 0;
                                Paid = invoiceDetailsList.All(x => x != null && x.RemainingBalance == 0);
                                PaidOn = Paid == true ? DateTime.Now : null;
                                double invoiceRemainingBalance = invoices.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                                _invoiceRepository.UpdateBalanceInInvoices(PaymentsTransaction, invoices.Id, invoiceRemainingBalance, Paid);
                            }
                        }
                        PaymentApplications paymentApplications = _paymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);
                        if (paymentApplications != null)
                        {
                            Payments payments = _paymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
                            if (payments != null)
                            {
                                if (paymentApplicationLineItems.isNegativeContext == true)
                                {
                                    RemainingBalance = payments.RemainingAmount + Math.Abs(paymentApplicationLineItems.AppliedAmount);
                                }
                                else
                                {
                                    RemainingBalance = payments.RemainingAmount + paymentApplicationLineItems.AppliedAmount;
                                }
                                _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, paymentApplications.Payment_Id, RemainingBalance);
                                if (PaymentApplicationLineItems_List != null)
                                {
                                    foreach (var item in PaymentApplicationLineItems_List)
                                    {
                                        _paymentsRepository.InactivePaymentApplicationLineItems(PaymentsTransaction, item.PaymentApplicationLineItem_Id);
                                    }
                                }
                                else
                                {
                                    _paymentsRepository.InactivePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems.PaymentApplicationLineItem_Id);
                                }

                                if (!Is_PaymentDeleted && PaymentApplicationLineItems_List == null)
                                {
                                    List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                    JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();

                                    if (journalEntry == null && payments?.PaymentType_Id == MasterLookup._PaymentTypes.Bill)
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Unapply is not supported for payments created through Bill Creation.";
                                        return serviceResponse;
                                    }

                                    if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                    {
                                        QBOUtility qBOUtility = new QBOUtility(Client_Id, User_Id, clients.AccountingService_Id, _AccountingType_Id);
                                        //UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, new Payments(), new List<PaymentApplicationLineItems>(), journalEntries, false, paymentApplicationLineItems);
                                        CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, payments, new List<PaymentApplicationLineItems>(), journalEntry, false, paymentApplicationLineItems);
                                        if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                        {
                                            serviceResponse.Status = true;
                                            serviceResponse.Message = "Un Applied payment successfully and synced to accounting software successfully.";
                                            PaymentsTransaction.Commit();
                                        }
                                        else
                                        {
                                            if (PaymentsTransaction.Connection != null)
                                            {
                                                PaymentsTransaction.Rollback();
                                            }
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Couldn't un apply and sync to accounting software";
                                            return serviceResponse;
                                        }
                                    }
                                    else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                                    {
                                        SageUtility sageUtility = new SageUtility(Client_Id, User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                        var createJournalRes = Task.Run(() =>
                                        sageUtility.CreateJournalEntry_Intacct(
                                            PaymentsTransaction,
                                            payments,
                                             new List<PaymentApplicationLineItems>(),
                                            journalEntry,
                                            false, paymentApplicationLineItems
                                        )).Result;

                                        var serializer = new XmlSerializer(typeof(SageResponse));
                                        SageResponse createSageJournalResponse = new SageResponse();
                                        using (var reader = new StringReader(createJournalRes))
                                        {
                                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                        }
                                        if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
                                        {
                                            serviceResponse.Status = true;
                                            serviceResponse.Message = $"Un Applied payment successfully and synced to accounting software successfully.";
                                            PaymentsTransaction.Commit();
                                            return serviceResponse;
                                        }
                                        else
                                        {
                                            if (PaymentsTransaction.Connection != null)
                                            {
                                                PaymentsTransaction.Rollback();
                                            }
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Couldn't un apply and sync to accounting software." /*+ createSageJournalResponse.Operation?.Result?.*/;
                                            return serviceResponse;
                                        }
                                    }
                                    else
                                    {
                                        PaymentsTransaction.Commit();
                                    }

                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                    }
                }
                serviceResponse.Status = true;
                serviceResponse.Message = string.IsNullOrEmpty(serviceResponse.Message) ? $"Un Applied payments succesfully." : serviceResponse.Message;
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        private void ApplyPaymentBasedDirectBillCommission(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
                return;

            foreach (DataRow row in table.Rows)
            {
                decimal commissionPaidToDate = row["Comm. Paid to Date"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Comm. Paid to Date"]);
                decimal agentCommissionPercentage = row["AgentCommPercentage"] == DBNull.Value ? 0m : Convert.ToDecimal(row["AgentCommPercentage"]);
                decimal premiumCollected = 0m;
                if (agentCommissionPercentage != 0)
                {
                    premiumCollected = Math.Round(commissionPaidToDate / (agentCommissionPercentage / 100m), 2);
                }
                row["InferredPremiumCollected"] = (double)premiumCollected;
                row["Comm."] = (double)commissionPaidToDate;
                row["CommsNetDue"] = (double)commissionPaidToDate;
            }
        }
    }
}
