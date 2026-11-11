using Accounting.Entities.DB.Transactions;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Utilities.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Accounting.Web.Models;
using Accounting.Entities.Internal;
using Accounting.Web.Abstract;

namespace Accounting.Web.Controllers
{
    public class InvoicesController : BaseController
    {
        private readonly ViewRender view;
        private readonly IPDFOptions _IPDFOptions;
        private readonly ICoverPayService coverPayService;

        public InvoicesController(ViewRender view, IPDFOptions iPDFOptions, ICoverPayService coverPayService)
        {
            this.view = view;
            _IPDFOptions = iPDFOptions;
            this.coverPayService = coverPayService;
        }
        public IActionResult Index()
        {
            ViewBag.Client_Id = userSession?.Client_Id ?? 0;
            if (!Is_AjaxRequest)
                return View(_PolicyDetails);
            return PartialView(_PolicyDetails);
        }
        public IActionResult GenerateInvoicePdf(long PolicyDetails_Id, int InstallmentNo)
        {
            if (PolicyDetails_Id > 0)
            {
                PolicyDetails policyDetails = new PolicyDetails();
                policyDetails = _policyDetailsRepository.GetPolicyDetails(PolicyDetails_Id);
                policyDetails.Clients = _accountRepository.GetClients(policyDetails.Client_Id);
                policyDetails.AgentDetails = _policyDetailsRepository.GetAgentDetails(policyDetails.AgentDetail_Id);
                policyDetails.CarrierDetails = _policyDetailsRepository.GetCarrierDetails(policyDetails.CarrierDetail_Id);
                policyDetails.InsuredDetails = _policyDetailsRepository.GetInsuredDetails(policyDetails.InsuredDetail_Id);
                policyDetails.WholesalerDetails = _policyDetailsRepository.GetWholesalerDetails(policyDetails.WholesalerDetail_Id ?? 0);
                policyDetails.Invoices = _invoiceRepository.GetInvoices(policyDetails.Id, InstallmentNo);
                policyDetails.TotalAppliedAmount = Convert.ToDouble(_paymentsRepository.GetInvoiceAppliedPaymentDetails(policyDetails.Invoices.Id, userSession.ConfigurationClient_Id)?.TotalAppliedAmount);
                policyDetails.Invoices.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(policyDetails.Invoices.Id);
                if (policyDetails.Invoices.InvoicesDetailsList?.Count > 0)
                {
                    policyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(policyDetails.Invoices.InvoicesDetailsList, policyDetails, true);
                }
                if (policyDetails?.Clients?.PaymentService_Id == MasterLookup._Services.ePay)
                {
                    byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                    long Entity_Id = 0;
                    byte EntityType_Id = 0;
                    string PostalCode = string.Empty;
                    if (BillingType_Id == MasterLookup._BillingTypes.Agency || BillingType_Id == MasterLookup._BillingTypes.AgencyDetailed)
                    {
                        Entity_Id = _PolicyDetails.AgentDetail_Id;
                        EntityType_Id = MasterLookup._EntityTypes.Agency;
                        PostalCode = policyDetails?.AgentDetails?.ZipCode;
                    }
                    else if (BillingType_Id == MasterLookup._BillingTypes.Insured)
                    {
                        Entity_Id = _PolicyDetails.InsuredDetail_Id;
                        EntityType_Id = MasterLookup._EntityTypes.Insured;
                        PostalCode = policyDetails?.InsuredDetails?.ZipCode;
                    }
                    policyDetails.ExternalAccountInformation = _externalAccountInformationRepository.GetExternalAccountInformation(Entity_Id, EntityType_Id, policyDetails.Client_Id);
                    if (policyDetails.ExternalAccountInformation != null)
                    {
                        policyDetails.ExternalAccountInformation.ZipCode = PostalCode;
                        string paymentUrlTemplate = LookupUtility.GetClientSettings(policyDetails.Clients.Client_Id, MasterLookup._ClientSettings.InvoicePaymentURLTemplate);
                        string finalPaymentUrl = CommonUtility.ApplyTemplateFromObject(paymentUrlTemplate, policyDetails.ExternalAccountInformation);
                        policyDetails.ExternalAccountInformation.PaymentURL = finalPaymentUrl;
                    }
                }
                var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = policyDetails,
                };
                byte[] CarrierLogo = null;
                var SettingsLogo = CommonUtility.GetEntitySettingValue<byte[]>((policyDetails?.Client_Id ?? 0), MasterLookup._EntityTypes.Carrier, policyDetails?.CarrierDetail_Id, MasterLookup._Settings.InvoiceCarrierLogo);
                if (SettingsLogo?.Count() > 0)
                {
                    CarrierLogo = SettingsLogo;
                }
                var htmlContent = this.view.Render("Invoices/InvoicePDF", viewDataDictionary);
                byte[] pdfbytes = _IPDFOptions.ExportToPdf(htmlContent, policyDetails.Clients.Logo, CarrierLogo);
                string filename = $"Invoice_{policyDetails.Invoices.InvoiceNo}.pdf";
                return File(pdfbytes, "application/pdf", filename);
            }
            else
            {
                return NoContent();
            }
        }
        public IActionResult GenerateInvoice(long PolicyDetails_Id, int InstallmentNo)
        {
            if (PolicyDetails_Id > 0)
            {
                _PolicyDetails = _policyDetailsRepository.GetPolicyDetails(PolicyDetails_Id);
                if(!string.IsNullOrEmpty(_PolicyDetails.PaymentPlan))
                {
                    _PolicyDetails.PaymentPlan = _installmentPlansRepository.GetPaymentPlanBy_PaymentCode(_PolicyDetails?.PaymentPlan, userSession.ConfigurationClient_Id)?.Name;
                }
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
                    _PolicyDetails.TotalAppliedAmount = Convert.ToDouble(_paymentsRepository.GetInvoiceAppliedPaymentDetails(_PolicyDetails.Invoices.Id, userSession.ConfigurationClient_Id)?.TotalAppliedAmount);
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
                    if (!string.IsNullOrEmpty(_PolicyDetails.PaymentPlan))
                    {
                        List<InstallmentDetails> installmentDetails = _policyDetailsRepository.GetInstallmentDetailsByPolicy(PolicyDetails_Id, userSession.Client_Id, InstallmentNo);
                        _PolicyDetails.Installments = installmentDetails?
                                                        .Where(x => x.InstallmentNo > 0)
                                                        .GroupBy(x => new
                                                        {
                                                            x.InstallmentNo,
                                                            DueDate = x.DueDate.Date
                                                        })
                                                        .Select(g => new Installment
                                                        {
                                                            InstallmentNo = g.Key.InstallmentNo,
                                                            DueDate = g.Key.DueDate,
                                                            TotalPremium = g.Sum(x => x.Premium),
                                                            Transactions = g.ToList()
                                                        })
                                                        .OrderBy(x => x.InstallmentNo)
                                                        .ToList();

                    }                      
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
                                var enableCoverPayPaymentLink = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.EnableCoverPayPaymentLink);
                                if (!string.IsNullOrEmpty(enableCoverPayPaymentLink))
                                {
                                    var enableCoverPayStripePaymentLink = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.EnableCoverPayStripePaymentLink);
                                    if (!string.IsNullOrEmpty(enableCoverPayStripePaymentLink))
                                    {
                                        var coverPayExternalClient_Id = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.CoverPayExternalClient_Id);
                                        var coverPayExternalPaymentLinkURL = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.CoverPayExternalPaymentLinkURL);
                                        if (!string.IsNullOrEmpty(coverPayExternalClient_Id) && !string.IsNullOrEmpty(coverPayExternalPaymentLinkURL))
                                        {
                                            string dynamicCoverPayURL = string.Format(coverPayExternalPaymentLinkURL, coverPayExternalClient_Id, _PolicyDetails?.ExternalAccountInformation?.AccountNumber, _PolicyDetails?.ExternalAccountInformation?.ZipCode);
                                            _PolicyDetails.ExternalPaymentLink = dynamicCoverPayURL;
                                        }
                                    }
                                    else
                                    {
                                        var externalPaymentDetails = coverPayService.GetExternalPaymentDetails(userSession.Client_Id, MasterLookup._Services.CorePay, PolicyDetails_Id);
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

                                            var coverPayURL = coverPayService.GenerateCoverPayPaymentLink(userSession.Client_Id, PolicyDetails_Id, _PolicyDetails?.PolicyNumber ?? string.Empty, userSession.User_Id, coverPayEntityType, coverPayEntityName, coverPayEntityEmail, _PolicyDetails?.ExternalAccountInformation?.AccountNumber, _PolicyDetails?.ExternalAccountInformation?.ZipCode);
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
                }

            }
            return PartialView(_PolicyDetails);
        }

        public IActionResult DownloadInvoice(long PolicyDetails_Id, int InstallmentNo)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            if (_PolicyDetails != null)
            {
                _PolicyDetails = _policyDetailsRepository.GetPolicyDetails(PolicyDetails_Id);
                if (!string.IsNullOrEmpty(_PolicyDetails.PaymentPlan))
                {
                    _PolicyDetails.PaymentPlan = _installmentPlansRepository.GetPaymentPlanBy_PaymentCode(_PolicyDetails?.PaymentPlan, userSession.ConfigurationClient_Id)?.Name;
                }
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
                    _PolicyDetails.TotalAppliedAmount = Convert.ToDouble(_paymentsRepository.GetInvoiceAppliedPaymentDetails(_PolicyDetails.Invoices.Id, userSession.ConfigurationClient_Id)?.TotalAppliedAmount);
                    _PolicyDetails.InvoicesList = _invoiceRepository.GetInvoicesByPolicyDetail_Id(PolicyDetails_Id);
                    _PolicyDetails.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(_PolicyDetails?.Invoices?.PolicyDetails_Id ?? 0);
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
                    if (!string.IsNullOrEmpty(_PolicyDetails.PaymentPlan))
                    {
                        List<InstallmentDetails> installmentDetails = _policyDetailsRepository.GetInstallmentDetailsByPolicy(PolicyDetails_Id, userSession.Client_Id, InstallmentNo);
                        _PolicyDetails.Installments = installmentDetails?
                                                        .Where(x => x.InstallmentNo > 0)
                                                        .GroupBy(x => new
                                                        {
                                                            x.InstallmentNo,
                                                            DueDate = x.DueDate.Date
                                                        })
                                                        .Select(g => new Installment
                                                        {
                                                            InstallmentNo = g.Key.InstallmentNo,
                                                            DueDate = g.Key.DueDate,
                                                            TotalPremium = g.Sum(x => x.Premium),
                                                            Transactions = g.ToList()
                                                        })
                                                        .OrderBy(x => x.InstallmentNo)
                                                        .ToList();
                    }    
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
                                var enableCoverPayPaymentLink = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.EnableCoverPayPaymentLink);
                                if (!string.IsNullOrEmpty(enableCoverPayPaymentLink))
                                {
                                    var enableCoverPayStripePaymentLink = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.EnableCoverPayStripePaymentLink);
                                    if (!string.IsNullOrEmpty(enableCoverPayStripePaymentLink))
                                    {
                                        var coverPayExternalClient_Id = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.CoverPayExternalClient_Id);
                                        var coverPayExternalPaymentLinkURL = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.CoverPayExternalPaymentLinkURL);
                                        if (!string.IsNullOrEmpty(coverPayExternalClient_Id) && !string.IsNullOrEmpty(coverPayExternalPaymentLinkURL))
                                        {
                                            string dynamicCoverPayURL = string.Format(coverPayExternalPaymentLinkURL, coverPayExternalClient_Id, _PolicyDetails?.ExternalAccountInformation?.AccountNumber, _PolicyDetails?.ExternalAccountInformation?.ZipCode);
                                            _PolicyDetails.ExternalPaymentLink = dynamicCoverPayURL;
                                        }
                                    }
                                    else
                                    {
                                        var externalPaymentDetails = coverPayService.GetExternalPaymentDetails(userSession.Client_Id, MasterLookup._Services.CorePay, PolicyDetails_Id);
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

                                            var coverPayURL = coverPayService.GenerateCoverPayPaymentLink(userSession.Client_Id, PolicyDetails_Id, _PolicyDetails?.PolicyNumber ?? string.Empty, userSession.User_Id, coverPayEntityType, coverPayEntityName, coverPayEntityEmail, _PolicyDetails?.ExternalAccountInformation?.AccountNumber, _PolicyDetails?.ExternalAccountInformation?.ZipCode);
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
            }

            return Ok(serviceResponse);

        }

        public DocumentFileInfo Download(PolicyDetails _PolicyDetails)
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
            var hasDefaultTemplate = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.RTWHasDefaultTemplate);
            if (!string.IsNullOrEmpty(hasDefaultTemplate))
            {
                var Footer = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.InvoiceFooter);
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

        public IActionResult UploadInvoicesPayment()
        {
            return PartialView();
        }

    }
}
