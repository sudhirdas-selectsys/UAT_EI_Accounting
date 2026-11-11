using Accounting.Entities.Internal;
using Accounting.Utilities;
using Accounting.Utilities.Internal;
using Accounting.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Accounting.Entities.DB.Transactions;

namespace Accounting.Web.Controllers
{
    public class SSOController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            string Message = "";
            string token = Request.Query["token"];

            if (string.IsNullOrEmpty(token))
            {
                Message = "We're unable to process your sign-in. For assistance, please contact our support team.";
                ViewBag.Message = Message;
                return View("_oidc_callback");
            }
            string redirectURL = MasterLookup.SSORedirectURL;
            //string redirectURL = "https://localhost:44308/SSO/callback";
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var audience = jwt.Audiences.FirstOrDefault();
            
            var principal = JwtValidator.ValidateToken(token, audience, redirectURL);
            if (principal != null)
            {

                var Email = jwt.Claims.FirstOrDefault(c => c.Type == "UserEmail")?.Value;
                if (!string.IsNullOrEmpty(Email))
                {
                    var authorizeUser = _usersRepository.GetUserByEmail(Email);
                    var EI_Instance = _accountRepository.GetClientIdByEI_InstanceNames(audience);
                    UserSession userSession = new UserSession();
                    if (authorizeUser != null)
                    {
                        userSession.User_Id = authorizeUser.User_Id;
                        userSession.FirstName = authorizeUser.FirstName;
                        userSession.LastName = authorizeUser.LastName;
                        userSession.Email = authorizeUser.Email;
                        userSession.Username = authorizeUser.Username;
                        userSession.UserRoleMappingList = _usersRepository.GetUserRoleMappingByUser_Id(authorizeUser.User_Id);
                        userSession.Client_Id = EI_Instance.Client_Id;
                        userSession.ConfigurationClient_Id = EI_Instance.Client_Id;
                        userSession.CompanyName = _accountRepository.GetClients(userSession.Client_Id).CompanyName;
                        userSession.ConfigurationCompanyName = _accountRepository.GetClients(userSession.ConfigurationClient_Id).CompanyName;
                        HttpContext.Session.SetObjectAsJson("UserSession", userSession);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        Message = $"The account <strong>{Email}</strong> is not associated with the instance. For assistance, please contact our support team.";
                        return View("_oidc_callback");
                    }
                }
            }
            if (string.IsNullOrEmpty(Message))
            {
                Message = "We're unable to process your sign-in. For assistance, please contact our support team.";
            }
            ViewBag.Message = Message;
        InvalidRequest:
            return View("_oidc_callback");
        }
    }
}
