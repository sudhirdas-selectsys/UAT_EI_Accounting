using Microsoft.AspNetCore.Mvc;

namespace Accounting.Web.Controllers
{
    public class PolicyDetailsController : BaseController
    {
        public IActionResult Index()
        {
            if (!Is_AjaxRequest)
                return View(_PolicyDetails);
            return PartialView(_PolicyDetails);
        }
        public IActionResult GeneralInfo()
        {
            return PartialView(_PolicyDetails);
        }

        public IActionResult AgentsDetails(long id)
        {
            return PartialView(_PolicyDetails);
        }

        public IActionResult CarriersDetails(long id)
        {
            return PartialView(_PolicyDetails);
        }

        public IActionResult InsuredsDetails(long id)
        {
            return PartialView(_PolicyDetails);
        }

        public IActionResult FinancesDetails(long id)
        {
            return PartialView(_PolicyDetails);
        }

        public IActionResult UnderwritersDetails(long id)
        {
            return PartialView(_PolicyDetails);
        }

        public IActionResult InterestsDetails(long id)
        {
            return PartialView(_PolicyDetails);
        }
        public IActionResult PolicyDetailsTransfer()
        {
            if (!Is_AjaxRequest)
                return View(_PolicyDetails);
            return PartialView(_PolicyDetails);
        }
    }
}
