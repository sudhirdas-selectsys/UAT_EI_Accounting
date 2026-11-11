using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.PrintCheck;
using Accounting.Utilities.Internal;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Web.Controllers
{
    public class PrintChecksController : BaseController
    {
        public IActionResult Index()
        {
            if (!Is_AjaxRequest)
                return View();
            return PartialView();
        }
        public IActionResult CheckPaymentsGrid()
        {
            int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
            int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);            
            long TotalRecords = 0;            
            List<Payments> _checkPaymentsList = _paymentsRepository.GetAllPaymentsForPrintCheck(userSession.Client_Id, PageSize, PageIndex, ref TotalRecords);
            return PartialView(_checkPaymentsList);
        }

        [HttpPost]
        public IActionResult PrintCheck(long OperatingBank_Id)
        {
            ViewBag.OperatingBank_Id = OperatingBank_Id;

            return PartialView();
        }

        [HttpPost]
        public ServiceResponse DownloadChecks(List<long> Payment_Ids, long OperatingBank_Id, int CheckNumber)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            if (Payment_Ids == null || !Payment_Ids.Any())
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Please select at least one payment.";

                return serviceResponse;
            }
            var operatingBank = LookupUtility.GetAllOperatingBanks().FirstOrDefault(x => x.OperatingBank_Id == OperatingBank_Id && x.Client_Id == userSession.Client_Id);

            if (operatingBank == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Bank Account not found.";
                return serviceResponse;
            }
            if (string.IsNullOrWhiteSpace(operatingBank.AccountNumber))
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Please add Account Number.";
                return serviceResponse;
            }

            if (operatingBank.AccountNumber.Length > 22)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Account number should not be greater than 22 characters.";
                return serviceResponse;
            }

            if (string.IsNullOrWhiteSpace(operatingBank.RoutingNumber))
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Please add Routing Number.";
                return serviceResponse;
            }

            if (operatingBank.RoutingNumber.Length != 9)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Routing Number must be 9 digits.";
                return serviceResponse;
            }

            if (string.IsNullOrWhiteSpace(operatingBank.SignerType))
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Please select Signature Type.";
                return serviceResponse;
            }

            if (string.IsNullOrWhiteSpace(operatingBank.SignerValue))
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Please add Signature Value.";
                return serviceResponse;
            }

            if (CheckNumber <= 0)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "Please enter a valid Check Number.";
                return serviceResponse;
            }

            var checkKeeperUtility = new ChecKeeperUtility(userSession.Client_Id);

            //foreach (long paymentId in Payment_Ids)
            //{
            serviceResponse = checkKeeperUtility.CreateChecks(/*paymentId*/Payment_Ids, operatingBank.AccountNumber, operatingBank.RoutingNumber, operatingBank.SignerValue, CheckNumber, operatingBank.SignerType);

            if (!serviceResponse.Status)
            {
                return serviceResponse;
            }
            foreach (long paymentId in Payment_Ids)
            {
                _paymentsRepository.UpdateCheckPaymentPrintStatus(paymentId);
            }
            //CheckNumber++;
            //}

            serviceResponse.Status = true;
            serviceResponse.Message = $"{Payment_Ids.Count} check(s) queued successfully.";

            return serviceResponse;
        }

    }
}
