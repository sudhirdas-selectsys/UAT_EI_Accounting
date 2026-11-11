using Accounting.Entities.DB.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            if (!Is_AjaxRequest)
                return View();
            return PartialView();
        }

        public IActionResult Dashboard()
        {
            //string filepath = @"C:\Users\Navyatha\Desktop\BRS Logo.jpg";
            //byte[] contentimagebytes = System.IO.File.ReadAllBytes(filepath);
            //_accountRepository.UpdateImage(10010, contentimagebytes);
            return PartialView();

            //List<PolicyDetails> policyDetails = new List<PolicyDetails>();
            //policyDetails = _policyDetailsRepository.GetAllPolicyDetails(userSession.Client_Id);
            //if (policyDetails?.Count > 0)
            //{
            //    foreach (var policy in policyDetails)
            //    {
            //        policy.PolicyDetailsLineItemsList = _policyDetailsRepository.GetPolicyDetailsLineItems(policy.Id);
            //    }
            //}
            //if (!Is_AjaxRequest)
            //    return View(policyDetails);
            //return PartialView(policyDetails);                        
        }
        public IActionResult DashboardGrid()
        {
            return PartialView(_PolicyDetailList);
        }


    }
}
