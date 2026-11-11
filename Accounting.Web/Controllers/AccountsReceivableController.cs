using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Accounting.Entities.Internal.Enums;
using Newtonsoft.Json;
using Accounting.Entities.External.SchedulerJobs;
using Accounting.Utilities.External.SchedulerJobs;

namespace Accounting.Web.Controllers
{
    public class AccountsReceivableController : BaseController
    {
        public IActionResult Index()
        {
            List<LU_LineItems_Clients> ClientLineList = _lookupRepository.GetLineItems_ClientsByClient_Id(userSession.Client_Id);
            var ClientLineItemIds = ClientLineList?.Select(x => x.LineItem_Id)?.ToList();
            ViewBag.ClientLineItemIds = ClientLineItemIds;
            if (!Is_AjaxRequest)
                return View();
            return PartialView();
        }
        public IActionResult LoadGrid(Search search, int pageIndex, int pageSize, string SortOrder)
        {
            //Search search = new Search();
            if (search == null)
            {
                search = new Search();
            }
            search.Client_Id = userSession.Client_Id;
            List<Invoices> InvoicesList = new List<Invoices>();
            List<InvoiceDetails> List = _policyDetailsRepository.GetARSearchResults(search, pageIndex, pageSize, SortOrder);
            if (List?.Count() > 0)
            {
                List = CommonUtility.GetInvoiceDetailsListWithDescription(List, new PolicyDetails());
                List<long> Invoice_Ids = List.OrderByDescending(x => x.Invoice_Id).GroupBy(x => x.Invoice_Id)
    .Select(g => g.Key)
    .ToList();
                if (Invoice_Ids?.Count() > 0)
                {
                    foreach (var invoice in Invoice_Ids)
                    {
                        Invoices inv = _invoiceRepository.GetInvoicesByInvoice_Id(invoice);
                        PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(inv?.PolicyDetails_Id ?? 0);
                        AgentDetails agentDetails = _policyDetailsRepository.GetAgentDetails(policyDetails?.AgentDetail_Id ?? 0);
                        InsuredDetails insuredDetails = _policyDetailsRepository.GetInsuredDetails(policyDetails?.InsuredDetail_Id ?? 0);
                        if (inv != null)
                        {
                            inv.AgencyName = agentDetails.Name;
                            inv.InsuredsName = insuredDetails?.Name;
                            inv.PolicyNumber = policyDetails?.PolicyNumber;
                            inv.TransactionType = policyDetails?.TransactionType;
                            inv.InvoicesDetailsList = List.Where(c => c.Invoice_Id == inv.Id).ToList();
                            inv.Amount = Convert.ToDouble(List.Where(c => c.Invoice_Id == inv.Id)?.Sum(c => Convert.ToDecimal(c.Amount)) ?? 0);
                            inv.RemainingBalance = Convert.ToDouble(List.Where(c => c.Invoice_Id == inv.Id)?.Sum(c => Convert.ToDecimal(c.RemainingBalance)) ?? 0);
                            inv.BillingType = policyDetails?.BillType;
                            inv.Is_Financed = inv.Is_Financed ? inv.Is_Financed : policyDetails.FinanceDetail_Id > 0 ? true : false;
                            InvoicesList.Add(inv);
                        }
                    }
                }
            }
            return PartialView(InvoicesList);

            //Search search = new Search();
            //search.Client_Id = userSession.Client_Id;
            //List<ARList> ARList = _policyDetailsRepository.GetARSearchResults(search);
            //return PartialView(ARList);

        }
        public IActionResult Details(long policyDetail_Id, long Invoice_Id, long Payment_Id, byte Direction_Id)
        {
            PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(policyDetail_Id);
            if (policyDetails != null)
            {
                policyDetails.AgentDetails = _policyDetailsRepository.GetAgentDetails(policyDetails.AgentDetail_Id);
                policyDetails.InsuredDetails = _policyDetailsRepository.GetInsuredDetails(policyDetails.InsuredDetail_Id);
                policyDetails.CarrierDetails = _policyDetailsRepository.GetCarrierDetails(policyDetails.CarrierDetail_Id);
                policyDetails.Invoices = _invoiceRepository.GetInvoicesByInvoice_Id(Invoice_Id);
                if (policyDetails.Invoices != null)
                {
                    policyDetails.Invoices.Payment_Id = Payment_Id;
                    policyDetails.Invoices.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(Invoice_Id, Direction_Id);
                    if (policyDetails.Invoices.InvoicesDetailsList?.Count() > 0)
                    {
                        policyDetails.Invoices.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(policyDetails.Invoices.InvoicesDetailsList, policyDetails);
                    }

                }
            }
            else
            {
                return BadRequest("Invalid Request.");
            }
            return PartialView(policyDetails);
        }
        public ServiceResponse MarkFinanced(int Invoice_Id, bool Is_Financed)
        {
            ServiceResponse response = new ServiceResponse();
            try
            {
                if (Invoice_Id > 0)
                {
                    _invoiceRepository.MarkAsFinanced(Invoice_Id, Is_Financed);
                    response.Status = true;
                    response.Message = "Successfull Marked this Invoice as Financed";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        [HttpGet]
        public IActionResult EditEntityDetails(long InvoiceId)
        {
            ViewBag.InvoiceId = InvoiceId;
            return PartialView();
        }
        [HttpGet]
        public IActionResult GetEntityName(long InvoiceId, byte EntityType_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            try
            {
                long clientId = userSession.Client_Id;

                var entity = _policyDetailsRepository.GetEntityNameByEntityTypeIdAndInvoiceId(clientId, InvoiceId, EntityType_Id);

                if (entity != null)
                {
                    serviceResponse.Status = true;
                    serviceResponse.Data = entity;
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "No entity found for the selected type.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return Json(serviceResponse);
        }
        [HttpPost]
        public ServiceResponse UpdateEntityDetails(InvoiceDetails invoiceDetails)
        {
            ServiceResponse serviceResponse = new();
            SqlTransaction sqlTransaction = _baseRepository.GetNewTransaction();
            try
            {
                invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                invoiceDetails.EntityType_Id = invoiceDetails.PayeeType_Id;
                invoiceDetails.EntityName = invoiceDetails.PayeeName;
                if (invoiceDetails != null)
                {
                    if (invoiceDetails.EntityType_Id == MasterLookup._EntityTypes.Others)
                    {
                        long Client_Id = userSession.Client_Id;
                        InvoiceDetails OthersEntity =  _policyDetailsRepository.InsertUpdateOtherEntityDetails(sqlTransaction, invoiceDetails, Client_Id);  
                        if(OthersEntity!=null)
                        {
                            invoiceDetails.Entity_Id = OthersEntity.Entity_Id;
                            invoiceDetails.EntityType_Id = OthersEntity.EntityType_Id;
                        }
                        else
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Others Entity details cannot be null.";
                        }
                    }
                    else if (invoiceDetails.EntityType_Id == MasterLookup._EntityTypes.BillingCompany)
                    {
                        if (invoiceDetails.PayorType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                        {
                            invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.WholesalerMGA;
                        }
                        else
                        {
                            invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                            invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;
                        }

                    }
                    else if (invoiceDetails.EntityType_Id == MasterLookup._EntityTypes.IssuingCompany)
                    {
                        invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.Carrier;

                    }
                    else if (invoiceDetails.EntityType_Id == MasterLookup._EntityTypes.SalesRep)
                    {
                        invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.SalesRep;
                        //long Client_Id = userSession.Client_Id;
                        //invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                        //invoiceDetails.EntityType_Id = invoiceDetails.PayeeType_Id;
                        //invoiceDetails.EntityName = invoiceDetails.PayeeName;
                        //if (!string.IsNullOrEmpty(invoiceDetails.EntityName))
                        //{
                        //    SalesRepDetails existingSalesRep = _policyDetailsRepository.GetSalesRepByName(invoiceDetails.EntityName, Client_Id);
                        //    if (existingSalesRep != null)
                        //    {
                        //        invoiceDetails.Entity_Id = existingSalesRep.Id;
                        //        invoiceDetails.EntityName = existingSalesRep.Name;
                        //    }
                        //    else
                        //    {
                        //        InvoiceDetails SalesRepEntity = _policyDetailsRepository.InsertUpdateSalesRepDetails(sqlTransaction, invoiceDetails, Client_Id);
                        //        if (SalesRepEntity != null)
                        //        {

                        //            invoiceDetails.Entity_Id = SalesRepEntity.Entity_Id;
                        //            invoiceDetails.EntityType_Id = SalesRepEntity.EntityType_Id;
                        //            InvoiceDetails EditSalesRepIdinAgentDetails = new InvoiceDetails();
                        //            EditSalesRepIdinAgentDetails.Invoice_Id = invoiceDetails.Invoice_Id;
                        //            EditSalesRepIdinAgentDetails.Entity_Id = SalesRepEntity.Entity_Id;
                        //            _policyDetailsRepository.UpdateAssignedSalesRepIdByInvoiceIdandEntityId(sqlTransaction, EditSalesRepIdinAgentDetails);
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    serviceResponse.Status = false;
                        //}                      
                    }
                    else if (invoiceDetails.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                    {
                        invoiceDetails.Entity_Id = invoiceDetails.Payee_Id;
                        invoiceDetails.EntityType_Id = MasterLookup._EntityTypes.FinanceCompany;
                    }
                    _policyDetailsRepository.updatePayeerAndEntityByInvoiceDetailId(sqlTransaction, invoiceDetails);
                    sqlTransaction.Commit();
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Entity Name Updated successfully.";
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Entity details cannot be null.";
                }
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public IActionResult SubmissionDocuments(long Submission_Id, string TransactionType)
        {
            ViewBag.TransactionType = TransactionType;
            Enum_APIServiceDetails enum_APIServiceDetails = new Enum_APIServiceDetails();
            var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(userSession.Client_Id).Where(x => x.Service_Id == MasterLookup._Services.EIConnect);
            if (ClientLevelParameters?.Count() > 0)
            {
                enum_APIServiceDetails.Service_Url = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.Value;
                enum_APIServiceDetails.APIKey = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Secret_Id)?.Value;
            }
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", "Bearer " + enum_APIServiceDetails.APIKey);
            //var ServiceURL = "https://localhost:44309/api/Portals/Accounting/GetSubmissionDocument/" + Submission_Id; //For Test
            var getLog = new ServiceLogInfo()
            {
                Service_Id = MasterLookup._Services.EIConnect,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UploadCommissionsStatements,
                Client_Id = userSession.Client_Id
            };
            HttpServiceUtility.ExecuteJsonGetRequest(getLog, enum_APIServiceDetails.Service_Url + "GetSubmissionDocument/" + Submission_Id/*ServiceURL*/, typeof(string), false, Headers, null, null);
            var documentResponse = JsonConvert.DeserializeObject<List<SubmissionDocuments>>(getLog.Response);
            return PartialView(documentResponse);
        }
        public ServiceResponse GetFileData(int DTDDocumentId)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            Enum_APIServiceDetails enum_APIServiceDetails = new Enum_APIServiceDetails();
            var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(userSession.Client_Id).Where(x => x.Service_Id == MasterLookup._Services.EIConnect);
            if (ClientLevelParameters?.Count() > 0)
            {
                enum_APIServiceDetails.Service_Url = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.Value;
                enum_APIServiceDetails.APIKey = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Secret_Id)?.Value;
            }
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            Headers.Add("Authorization", "Bearer " + enum_APIServiceDetails.APIKey);
            //var ServiceURL = "https://localhost:44309/api/Portals/Accounting/GetFileData/" + DTDDocumentId; //For Test
            var getLog = new ServiceLogInfo()
            {
                Service_Id = MasterLookup._Services.EIConnect,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UploadCommissionsStatements,
                Client_Id = userSession.Client_Id
            };
            HttpServiceUtility.ExecuteJsonGetRequest(getLog,enum_APIServiceDetails.Service_Url + "GetFileData/" + DTDDocumentId, typeof(string), false, Headers, null, null);
            var documentResponse = getLog.Response;
            serviceResponse.Data = documentResponse.Trim().Trim('"').Replace("\\\"", "");
            return serviceResponse;
        }
        public ServiceResponse SendPaymentReminder(long Invoice_Id, int Job_Id)
        {
            ServiceResponse response = new ServiceResponse();
            try
            {
                if (Invoice_Id > 0 && Job_Id > 0)
                {
                    SchedulerJobsUtility schedulerJobsUtility = new SchedulerJobsUtility(userSession.Client_Id);
                    SchedulerJobsResponse schedulerJobsResponse = schedulerJobsUtility.SchedulerJobsResponse(Invoice_Id, Job_Id);
                    if (schedulerJobsResponse != null && schedulerJobsResponse.Status == 200)
                    {
                        response.Message = "Sucessfully sent the payment reminder";
                        response.Status = true;
                    }
                    else
                    {
                        response.Status = false;
                    }
                }
                else
                {
                    response.Status = false;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Status = false;
            }
            return response;
        }
    }
}
