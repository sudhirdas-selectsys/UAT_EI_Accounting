using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;

namespace Accounting.Web.Controllers
{
    public class SearchController : BaseController
    {
        [HttpPost]
        public IActionResult SearchAR(Search search, int pageIndex, int pageSize, string SortOrder)
        {
            List<Invoices> InvoicesList = new List<Invoices>();
            if (search?.Client_Id == 0)
            {
                search.Client_Id = userSession.Client_Id;
            }
            List<InvoiceDetails> List = _policyDetailsRepository.GetARSearchResults(search, pageIndex, pageSize, SortOrder);
            if (List?.Count > 0)
            {

                InvoicesList = List
                    //.OrderByDescending(x => x.Invoice_Id)
                    .GroupBy(x => x.Invoice_Id)
                    .Select(g =>
                    {
                        var first = g.First();

                        return new Invoices
                        {
                            PolicyDetails_Id = first.PolicyDetails_Id,
                            Id = first.Invoice_Id,
                            InvoiceNo = first.InvoiceNo,
                            DueDate = first.InvoiceDueDate,
                            Paid = first.InvoicePaid,
                            PaidOn = first.InvoicePaidOn,
                            InvoiceDueDate = first.InvoiceDueDate,
                            BillDate = first.BillDate,
                            InstallmentNo = first.InstallmentNo,
                            AgencyName = first.AgencyName,
                            InsuredsName = first.InsuredName,
                            PolicyNumber = first.PolicyNumber,
                            TransactionType = first.TransactionType,
                            BillingType = first.BillType,
                            Is_Financed = first.Is_Financed,
                            FinanceName = first.FinanceName,
                            CreatedOn = first.InvoiceCreatedOn,
                            Entity_Id = first.Entity_Id,
                            EntityType_Id = first.EntityType_Id,
                            Schedule_Id = first.Schedule_Id,
                            // Aggregate values
                            Amount = Convert.ToDouble(g.Sum(x => Convert.ToDecimal(x.Amount))),
                            RemainingBalance = Convert.ToDouble(g.Sum(x => Convert.ToDecimal(x.RemainingBalance))),
                            Submission_Id = first.Submission_Id,
                            // Child list
                            InvoicesDetailsList = g.ToList()
                        };
                    })
                    .ToList();
            }
            return PartialView("~/Views/AccountsReceivable/LoadGrid.cshtml", InvoicesList);

            //if (search != null)
            //{
            //    if (search.Client_Id == 0)
            //        search.Client_Id = userSession.Client_Id;
            //    List<ARList> ARList = _policyDetailsRepository.GetARSearchResults(search);
            //    return PartialView("~/Views/AccountsReceivable/LoadGrid.cshtml", ARList);
            //}
            //else
            //{
            //    return BadRequest("Search object can not be null.");
            //}
        }
        [HttpPost]
        public IActionResult PaymentsSearch(Payments PaymentsSearch, String SortOrder)
        {
            if (PaymentsSearch != null)
            {
                int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
                int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);
                //string SortOrder = "DESC";
                //string SortByColumn = "EntityName";
                long TotalRecords = 0;
                PaymentsSearch.Client_Id = userSession.Client_Id;
                List<Payments> PaymentsList = _paymentsRepository.GetPaymentsSearchResults(PaymentsSearch, PageSize, PageIndex, SortOrder,   ref TotalRecords);
                ViewBag.TotalRecords = TotalRecords;
                ViewBag.PageIndex = PageIndex;
                ViewBag.PageSize = PageSize;
                return PartialView("~/Views/Payments/PaymentsGrid.cshtml", PaymentsList);
            }
            else
            {
                return BadRequest("Search object can not be null.");
            }
        }
        [HttpPost]
        public IActionResult PaymentsSearchForSelection(Payments PaymentsSearch)
        {
            if (PaymentsSearch != null)
            {
                PaymentsSearch.Client_Id = userSession.Client_Id;
                if (PaymentsSearch.EntityType_Id > 0)
                {
                    PaymentsSearch.EntityType_Id = 0;
                }
                List<Payments> PaymentsList = _paymentsRepository.GetPaymentsSearchResults(PaymentsSearch);
                PaymentsList = PaymentsList.Where(p => p.PaymentStatus_Id != MasterLookup._PaymentStatus.BouncedReversed).ToList();
                ViewBag.Direction_Id = PaymentsSearch.Direction_Id;
                ViewBag.IsOutstandingChecked = PaymentsSearch.OutstandingBalances;
                return PartialView("~/Views/Payments/PaymentsSelectionGrid.cshtml", PaymentsList);
            }
            else
            {
                return BadRequest("Search object can not be null.");
            }
        }
        [HttpPost]
        public IActionResult SearchAP(Search search, int pageIndex, int pageSize, string SortOrder)
        {
            if (search?.Client_Id == 0)
            {
                search.Client_Id = userSession.Client_Id;
            }
            List<Invoices> InvoicesList = new List<Invoices>();
            List<InvoiceDetails> List = _policyDetailsRepository.GetAPSearchResults(search, pageIndex, pageSize, SortOrder);
            if (List?.Count > 0)
            {

                InvoicesList = List
                    //.OrderByDescending(x => x.Invoice_Id)
                    .GroupBy(x => x.Invoice_Id)
                    .Select(g =>
                    {
                        var first = g.First();

                        return new Invoices
                        {
                            Id = first.Invoice_Id,
                            PolicyDetails_Id = first.PolicyDetails_Id,
                            InvoiceNo = first.InvoiceNo,
                            DueDate = first.InvoiceDueDate,
                            Paid = first.InvoicePaid,
                            PaidOn = first.InvoicePaidOn,
                            InvoiceDueDate = first.InvoiceDueDate,
                            BillDate = first.BillDate,
                            InstallmentNo = first.InstallmentNo,
                            AgencyName = first.AgencyName,
                            InsuredsName = first.InsuredName,
                            PolicyNumber = first.PolicyNumber,
                            TransactionType = first.TransactionType,
                            BillingType = first.BillType,
                            Is_Financed = first.Is_Financed,
                            CreatedOn = first.InvoiceCreatedOn,
                            Entity_Id = first.Entity_Id,
                            EntityType_Id = first.EntityType_Id,
                            Schedule_Id = first.Schedule_Id,
                            // Aggregate values
                            Amount = Convert.ToDouble(g.Sum(x => Convert.ToDecimal(x.Amount))),
                            RemainingBalance = Convert.ToDouble(g.Sum(x => Convert.ToDecimal(x.RemainingBalance))),

                            // Child list
                            InvoicesDetailsList = g.ToList()
                        };
                    })
                    .ToList();
            }
            // return PartialView("~/Views/Home/AccountsPayableGrid.cshtml",InvoicesList);
            return PartialView("~/Views/AccountsPayable/LoadGrid.cshtml", InvoicesList);
        }
        [HttpPost]
        public IActionResult SearchInvoicesPaymentDetails(List<UploadedInvoicesPaymentDTO> uploadedInvoicesPayment, byte Direction_Id)
        {
            List<Invoices> InvoicesList = new List<Invoices>();
            if (uploadedInvoicesPayment != null && Direction_Id != 0)
            {
                foreach (var invoicespayment in uploadedInvoicesPayment)
                {
                    var InvoiceorPolicyNumber = invoicespayment.InvoiceNumber ?? invoicespayment.PolicyNumber;
                    List<InvoiceDetails> List = _policyDetailsRepository.GetSearchInvoicesPaymentDetails(InvoiceorPolicyNumber, Direction_Id, userSession.Client_Id);
                    if (List?.Count() > 0)
                    {
                        List = CommonUtility.GetInvoiceDetailsListWithDescription(List, new PolicyDetails());
                        List<long> Invoice_Ids = List.OrderByDescending(x => x.Invoice_Id).GroupBy(x => x.Invoice_Id).Select(g => g.Key).ToList();
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
                                    inv.Amount = List.Where(c => c.Invoice_Id == inv.Id)?.Sum(x => x.Amount) ?? 0.0;
                                    inv.RemainingBalance = List.Where(c => c.Invoice_Id == inv.Id)?.Sum(x => x.RemainingBalance) ?? 0.0;
                                    inv.BillingType = policyDetails.BillType;
                                    if (!InvoicesList.Any(x => x.Id == inv.Id))
                                    {
                                        InvoicesList.Add(inv);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Direction_Id == MasterLookup._Directions.AR)
            {
                return PartialView("~/Views/AccountsReceivable/LoadGrid.cshtml", InvoicesList);
            }
            else if (Direction_Id == MasterLookup._Directions.AP)
            {
                return PartialView("~/Views/AccountsPayable/LoadGrid.cshtml", InvoicesList);
            }
            else
            {
                return BadRequest("Invalid Direction.");
            }
        }
        public IActionResult DashboardSearch(PolicyDetails policysearch)
        {
            if (policysearch != null)
            {
                int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
                int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);
                long TotalRecords = 0;

                List<PolicyDetails> policylist = _policyDetailsRepository.GetAllPolicyDetails(policysearch, userSession.Client_Id, PageSize, PageIndex, ref TotalRecords);
                if (policylist?.Count > 0)
                {
                    foreach (var policy in policylist)
                    {
                        policy.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(policy.Id);
                    }
                }
                ViewBag.TotalRecords = TotalRecords;
                ViewBag.PageIndex = PageIndex;
                ViewBag.PageSize = PageSize;
                return PartialView("~/Views/Home/DashboardGrid.cshtml", policylist);
            }
            else
            {
                return BadRequest("Search object can not be null.");
            }
        }

        [HttpPost]
        public IActionResult MakePaymentsSearch(PaymentsEntity paymentEntity, int pageIndex, int pageSize)
        {
            try
            {
                if (paymentEntity != null)
                {
                    long clientId = userSession.Client_Id;
                    List<PaymentsEntity> agencyList = _makepaymentRepository.GetInvoicedEntity(clientId, paymentEntity, pageIndex, pageSize) ?? new List<PaymentsEntity>();
                    return PartialView("~/Views/MakePayments/_EntityInfo.cshtml", agencyList);
                }
                else
                {
                    return BadRequest("Search object can not be null.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        [HttpPost]
        public IActionResult CheckPaymentsSearch(Payments PaymentsSearch, String SortOrder)
        {
            if (PaymentsSearch != null)
            {
                int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
                int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);
                //string SortOrder = "DESC";
                //string SortByColumn = "EntityName";
                long TotalRecords = 0;
                PaymentsSearch.Client_Id = userSession.Client_Id;
                List<Payments> PaymentsList = _paymentsRepository.GetCheckPaymentsSearchResults(PaymentsSearch, PageSize, PageIndex, SortOrder, ref TotalRecords);
                ViewBag.TotalRecords = TotalRecords;
                ViewBag.PageIndex = PageIndex;
                ViewBag.PageSize = PageSize;
                return PartialView("~/Views/PrintChecks/CheckPaymentsGrid.cshtml", PaymentsList);
            }
            else
            {
                return BadRequest("Search object can not be null.");
            }
        }

    }

}

