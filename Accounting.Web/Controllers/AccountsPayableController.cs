using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Accounting.Web.Controllers
{
    public class AccountsPayableController : BaseController
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
        public IActionResult LoadGrid(int pageIndex, int pageSize, string SortOrder)
        {
            Search search = new Search();
            search.Client_Id = userSession.Client_Id;
            List<Invoices> InvoicesList = new List<Invoices>();
            List<InvoiceDetails> List = _policyDetailsRepository.GetAPSearchResults(search, pageIndex, pageSize, SortOrder);
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
                            inv.Amount = Convert.ToDouble(List.Where(c => c.Invoice_Id == inv.Id)?.Sum(c => Convert.ToDecimal(c.Amount)) ?? 0);
                            inv.RemainingBalance = Convert.ToDouble(List.Where(c => c.Invoice_Id == inv.Id)?.Sum(c => Convert.ToDecimal(c.RemainingBalance)) ?? 0);
                            inv.InvoicesDetailsList = List.Where(c => c.Invoice_Id == inv.Id).ToList();
                            inv.BillingType = policyDetails?.BillType;
                            InvoicesList.Add(inv);
                        }
                    }
                }
            }
            return PartialView(InvoicesList);
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
                        //    SalesRepDetails existingSalesRep = _policyDetailsRepository.GetSalesRepDetails(invoiceDetails.Entity_Id);
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
    }
}
