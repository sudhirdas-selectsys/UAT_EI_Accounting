using Accounting.Entities.Internal;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Web.Controllers
{
    public class TransactionHistoryController : BaseController
    {
        public IActionResult Index()
        {
            if (!Is_AjaxRequest)
                return View();
            return PartialView();
        }

        public ServiceResponse GetPayorOrPayeeNames(string Name)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (Name == null)
                {
                    serviceResponse.Status = false;
                }
                else
                {
                    List<LookupEntity>? namesList = new List<LookupEntity>();
                    namesList = _transactionhistoryRepository.GetAllPayorOrPayeeDetailsByName(Name);
                    serviceResponse.Data = namesList;
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ServiceResponse GetPolicyNumbers(string PolicyNumber)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (PolicyNumber == null)
                {
                    serviceResponse.Status = false;
                }
                else
                {
                    List<LookupEntity>? namesList = new List<LookupEntity>();
                    namesList = _transactionhistoryRepository.GetAllPolicyNumbersByNumber(PolicyNumber, userSession.Client_Id);
                    serviceResponse.Data = namesList;
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public IActionResult TransactionHistoryGrid()
        {
            return PartialView(_TransactionHistoryList);

            //TransactionHistorySearch search = new TransactionHistorySearch();
            //search.Client_Id = userSession.Client_Id;
            //List<TransactionHistory> List = _transactionhistoryRepository.GetAllTransactionHistory(search);
            //return PartialView(List);
        }

        public IActionResult TransactionHistoryDetailsViewOnly(long PaymentApplicationLineItem_Id)
        {
            TransactionHistory transactionhistory = _transactionhistoryRepository.GetTransactionHistoryDetails(PaymentApplicationLineItem_Id);
            return PartialView(transactionhistory);
        }

        [HttpPost]
        public IActionResult TransactionHistorySearch(TransactionHistorySearch TransactionHistorySearch)
        {
            if (TransactionHistorySearch != null)
            {
                int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
                int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);               
                long TotalRecords = 0;
                TransactionHistorySearch.Client_Id = userSession.Client_Id;
                List<TransactionHistory> transactionhistoryList = _transactionhistoryRepository.GetAllTransactionHistory(TransactionHistorySearch, PageSize, PageIndex,ref TotalRecords);
                ViewBag.TotalRecords = TotalRecords;
                ViewBag.PageIndex = PageIndex;
                ViewBag.PageSize = PageSize;
                return PartialView("~/Views/TransactionHistory/TransactionHistoryGrid.cshtml", transactionhistoryList);
            }
            else
            {
                return BadRequest("Search object can not be null.");
            }
        }
    }
}
