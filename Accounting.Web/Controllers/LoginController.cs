using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.Microsoft.SSO;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.Microsoft.SSO;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Implementations;
using Accounting.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Accounting.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        internal readonly IUsersRepository _usersRepository = new UsersRepository();
        internal readonly IClientsRepository _clientRepository = new ClientsRepository();
        internal readonly ITrustedDevicesRepository _ITrustedDevicesRepository = new TrustedDevicesRepository();
        private readonly ClientInfoHelper _clientInfoHelper;
        public UserSession userSession { get; set; }
        public int trustedDays = int.TryParse(WebAppReferenceUtility._configuration["TrustedDeviceDays"], out var d) ? d : 30;
        public string? secret = WebAppReferenceUtility._configuration["TrustedDeviceSecret"]?.ToString();
        public int OTPexpiryMinutes = int.TryParse(WebAppReferenceUtility._configuration["OTPexpiryMinutes"], out var m) ? m : 5;
        public string? CookieNameSubstring = WebAppReferenceUtility._configuration["CookieNameSubstring"]?.ToString() ?? "EI_Accounting";
        public LoginController(ClientInfoHelper clientInfoHelper, IWebHostEnvironment IWebHostEnvironment)
        {
            _IWebHostEnvironment = IWebHostEnvironment;
            _clientInfoHelper = clientInfoHelper;
        }
        public IActionResult Index(string flow = "")
        {
            ViewBag.PasswordFlow = flow;
            var userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            if (userSession != null && string.IsNullOrEmpty(flow))
            {
                HttpContext.Session.Clear();
            }
            return View();
        }
        [HttpPost]
        public IActionResult Index(Users user)
        {
            ServiceResponse response = new ServiceResponse();
            var ValidUserDetails = _usersRepository.ValidateUsers(user.Username, user.Password, false);

            if (ValidUserDetails == null || ValidUserDetails.Password != user.Password)
            {
                response.Status = false;
                response.Message = "Invalid username or password.";
                return Json(response);
            }

            response.IsMFA_Enable = true;

            if (response.IsMFA_Enable)
            {
                if (TryBypassOtpWithTrustedDevice(ValidUserDetails.Username, ValidUserDetails.User_Id))
                {
                    response.IsTrustedDevice = true;
                    user.Is_OtpVerified = true;
                    StoreUserSession(user);
                    response.Status = true;
                    return Json(response);
                }
                else
                {
                    response.IsTrustedDevice = false;
                    if (StoreUserSession(user))
                        response.Status = true;
                    response.Message = "Login success. Choose MFA method.";
                    return Json(response);
                }
            }
            return View();
        }
        #region MFAMethods
        public IActionResult MFAoption() => PartialView("_MfaOptions");
        public IActionResult TrustedDevice() => PartialView("_TrustedDevice");

        public IActionResult OtpVerfication(string method)
        {
            var model = new OtpViewModel
            {
                OtpType = method
            };

            try
            {

                if (string.IsNullOrWhiteSpace(method) ||
                    !(method.Equals("Email", StringComparison.OrdinalIgnoreCase) || method.Equals("Phone", StringComparison.OrdinalIgnoreCase)))
                {
                    model.ErrorMessage = "Invalid verification method.";
                    return PartialView("_OtpVerification", model);
                }
                var userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
                var username = userSession.Username;
                if (string.IsNullOrEmpty(username))
                {
                    model.ErrorMessage = "Session expired. Please log in again.";
                    return PartialView("_OtpVerification", model);
                }

                var user = _usersRepository.GetUsers(username);

                if (user == null)
                {
                    model.ErrorMessage = "User account not found.";
                    return PartialView("_OtpVerification", model);
                }
                model.UserEmail = user.Email;
                return PartialView("_OtpVerification", model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "An unexpected error occurred: " + ex.Message;
                return PartialView("_OtpVerification", model);
            }
        }
        public IActionResult SendOtp(string method)
        {
            var response = new ServiceResponse();
            userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            var username = userSession.Username;
            if (username == null)
            {
                response.Status = false;
                response.Message = "Session expired. Please log in again.";
                return Json(response);
            }

            var user = _usersRepository.GetUsers(username);
            if (user == null)
            {
                response.Status = false;
                response.Message = "User not found.";
                return Json(response);
            }


            var otp = new Random().Next(100000, 999999).ToString();
            var otpHash = EncryptionHelperUtlity.Encrypt(otp);
            var expiresAt = DateTime.Now.AddMinutes(OTPexpiryMinutes); //Default 5 Min
            _usersRepository.SaveOtpRecord(userSession.User_Id, otpHash, method, expiresAt);

            bool sent = false;
            //sent = true;
            sent = SendUserOtpEmail(user, otp, OTPexpiryMinutes);
            //  if (method == "Email")
            //  sent = email serivce

            // else if (method == "Phone")
            // sent = sms service

            response.Status = true;
            response.Message = sent ? "OTP sent successfully." : "Failed to send OTP.";
            return Json(response);
        }
        private bool SendUserOtpEmail(Users userAccount, string otp, int OTPexpiryMinutes)
        {
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));
            if (string.IsNullOrWhiteSpace(otp)) throw new ArgumentException("OTP is required", nameof(otp));

            if (string.IsNullOrWhiteSpace(_IWebHostEnvironment.WebRootPath))
                throw new InvalidOperationException("WebRootPath is not available.");

            var templatePath = Path.Combine(_IWebHostEnvironment.WebRootPath, "EmailTemplates", "LoginOtpEmail.html");
            if (!System.IO.File.Exists(templatePath))
            {

                return false;
            }

            var content = System.IO.File.ReadAllText(templatePath);

            var sb = new StringBuilder();
            foreach (var ch in otp)
            {
                sb.AppendFormat(
                    "<span style=\"display:inline-block; padding:15px 20px; font-size:20px; font-weight:bold; border: solid 1px #B8AAFF; background:#ffffff; border-radius:6px; margin:0 5px;\">{0}</span>",
                    WebUtility.HtmlEncode(ch.ToString())
                );
            }
            var otpHtml = sb.ToString();


            var userName = userAccount.FirstName;

            content = content.Replace("{{UserName}}", WebUtility.HtmlEncode(userName));
            content = content.Replace("{{OTP}}", otpHtml);
            content = content.Replace("{{OtpExpiryMinutes}}", OTPexpiryMinutes.ToString());

            var recipientEmail = userAccount.Email;

            HttpContext.Session.SetString("User_Email", recipientEmail);


            var toList = new List<MailAddress> { new MailAddress(recipientEmail) };


            var images = new[]
            {
            new { CID = "otp-banner", Rel = Path.Combine("images","login","otp","otp-banner.jpg") },
            new { CID = "fb", Rel = Path.Combine("images","login","otp","fb.png") },
            new { CID = "x", Rel = Path.Combine("images","login","otp","x.png") },
            new { CID = "in", Rel = Path.Combine("images","login","otp","in.png") },
            new { CID = "footer", Rel = Path.Combine("images","login","otp","footer.png") }
        };

            List<InLineAttachmentItem> inLineAttachmentItems = new List<InLineAttachmentItem>();
            foreach (var img in images)
            {
                string imagePath = Path.Combine(_IWebHostEnvironment.WebRootPath, img.Rel);
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                inLineAttachmentItems.Add(new InLineAttachmentItem()
                {
                    IsInline = true,
                    BinaryImage = imageBytes,
                    Name = img.CID,
                    ContentID = img.CID
                });
            }

            EWS_EmailUtility _EmailUtility = new EWS_EmailUtility();
            var client = _clientRepository.GetClients(userAccount.Client_Id);
            string Subject = "ExpertInsured Accounting – Your One-Time Login Password (OTP)";
            _EmailUtility.SendExceptionReport(Subject, content, client.CompanyName, null, recipientEmail, null, inLineAttachmentItems);

            return true;

        }
        public IActionResult VerifyOtp(string otp)
        {
            // IsEmailVerified = false;

            var response = new ServiceResponse();
            userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            long? userId = userSession.User_Id;
            string userEmail = userSession.Email;
            TempData["PendingEmail"] = userEmail;


            if (userId == null)
            {
                response.Status = false;
                response.Message = "Session expired. Please log in again.";
                return Json(response);
            }

            var otpRecord = _usersRepository.GetValidOtpForUser(userId.Value);
            if (otpRecord == null)
            {
                response.Status = false;
                response.Message = "OTP expired or not found.";
                return Json(response);
            }


            string decrryptotp = EncryptionHelperUtlity.Decrypt(otpRecord.OtpHash);
            if (otp != decrryptotp)
            {
                response.Status = false;
                response.Message = "Invalid OTP. Please try again.";
                return Json(response);
            }

            _usersRepository.MarkOtpUsed(otpRecord.OtpId);
            var user = _usersRepository.GetUserByUserId(userId.Value);

            //HttpContext.Session.Remove("User_UserId");
            //HttpContext.Session.Remove("userEmail");

            if (user != null)
            {
                user.Is_OtpVerified = true;
                StoreUserSession(user);
            }

            //IsEmailVerified = true;

            response.Status = true;
            response.Message = "OTP verified successfully.";
            return Json(response);
        }
        public IActionResult SaveTrustedDevice()
        {
            var info = _clientInfoHelper.GetClientInfo();

            string browser = info.Browser;
            string deviceLabel = info.DeviceLabel;
            string userAgent = info.UserAgent;

            var response = new ServiceResponse();

            var currentdatetime = DateTime.Now;


            if (string.IsNullOrWhiteSpace(secret))
            {
                response.Status = false;
                response.Message = "TrustedDeviceSecret setting is required.";
                return Json(response);
            }

            // Generate device + token
            var deviceId = Guid.NewGuid();
            var rawToken = TrustedDeviceHelper.GenerateRandomToken(32);
            var tokenHash = TrustedDeviceHelper.ComputeTokenHash(rawToken, secret);
            var expiresAt = currentdatetime.AddDays(trustedDays);

            // Get IP and machine safely
            var ip = OTPHelper.GetClientIp(HttpContext);
            var currentMachine = OTPHelper.GetRemoteAddress(HttpContext.Request);

            // Example: replace "User.UserID" with the authenticated user's ID
            long userId = 0;
            userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            if (userSession != null)
            {
                userId = userSession.User_Id;
            }

            var entity = new TrustedDevice
            {
                UserId = userId,
                DeviceId = deviceId,
                TokenHash = tokenHash,
                Browser = browser ?? string.Empty,
                DeviceLabel = deviceLabel ?? string.Empty,
                MachineName = currentMachine,
                UserAgent = userAgent ?? string.Empty,
                IpAddress = ip,
                CreatedOn = currentdatetime,
                Status = true,
                CreatedBy = userId
            };

            try
            {
                var trustedDevice = _ITrustedDevicesRepository.Insert(entity);
                var trustedDeviceId = trustedDevice.DeviceId;
                // Set cookie
                TrustedDeviceHelper.SetTrustedCookie(userId, deviceId, rawToken, expiresAt, HttpContext, CookieNameSubstring);

                response.Status = true;
                response.Message = "Trusted device saved successfully.";
                response.IsTrustedDevice = true;
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"Error saving trusted device: {ex.Message}";
            }

            return Json(response);
        }
        private bool TryBypassOtpWithTrustedDevice(string userName, long userId)
        {
            if (userSession != null)
            {
                userId = userSession.User_Id;
            }

            if (string.IsNullOrWhiteSpace(userName)) return false;


            if (userId <= 0) return false;

            // Read cookie (deviceId + Base64 token)
            if (!TrustedDeviceHelper.TryGetTrustedCookie(HttpContext, userId, CookieNameSubstring, out var deviceId, out var tokenB64))
                return false;

            if (string.IsNullOrWhiteSpace(tokenB64)) return false;

            // Decode token
            byte[] rawToken;
            try
            {
                rawToken = Convert.FromBase64String(tokenB64);
                if (rawToken.Length == 0) return false;
            }
            catch
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(secret)) return false;

            string currentMachine = OTPHelper.GetRemoteAddress(HttpContext.Request);
            var hash = TrustedDeviceHelper.ComputeTokenHash(rawToken, secret);

            if (!_ITrustedDevicesRepository.Validate(userId, deviceId, hash, currentMachine, trustedDays, out var expiresUtc, out var isRevoked))
                return false;

            if (!expiresUtc.HasValue || expiresUtc.Value <= DateTime.UtcNow) return false;
            if (isRevoked == false) return false;

            // Update last seen / IP
            var ip = OTPHelper.GetClientIp(HttpContext);
            _ITrustedDevicesRepository.Touch(userId, deviceId, hash, ip);

            // Success: sign in
            //SignInAndRedirect(userName);
            return true;
        }
        #endregion
        private bool StoreUserSession(Users user)
        {
            UserSession userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");

            if (userSession == null)
                userSession = new UserSession();

            if (user != null)
            {
                Users loggedInUser = _usersRepository.ValidateUsers(user.Username, user.Password, false);
                if (loggedInUser != null)
                {
                    userSession.User_Id = loggedInUser.User_Id;
                    userSession.FirstName = loggedInUser.FirstName;
                    userSession.LastName = loggedInUser.LastName;
                    userSession.Email = loggedInUser.Email;
                    userSession.Username = loggedInUser.Username;
                    userSession.UserRoleMappingList = _usersRepository.GetUserRoleMappingByUser_Id(loggedInUser.User_Id);
                    if (userSession.UserRoleMappingList.Count > 0)
                    {
                        if (userSession.UserRoleMappingList.Any(x => x.Role_Id == MasterLookup._Roles.SuperAdmin))
                        {
                            if (loggedInUser.DefaultClient_Id != null)
                            {
                                userSession.Client_Id = loggedInUser.DefaultClient_Id ?? 0;
                                userSession.ConfigurationClient_Id = loggedInUser.DefaultClient_Id ?? 0;
                            }
                            else
                            {
                                userSession.Client_Id = loggedInUser.Client_Id;
                                userSession.ConfigurationClient_Id = loggedInUser.Client_Id;
                            }
                        }
                        else
                        {
                            userSession.Client_Id = loggedInUser.Client_Id;
                            userSession.ConfigurationClient_Id = loggedInUser.Client_Id;
                        }
                    }
                    userSession.CompanyName = _clientRepository.GetClients(userSession.Client_Id).CompanyName;
                    userSession.ConfigurationCompanyName = _clientRepository.GetClients(userSession.ConfigurationClient_Id).CompanyName;
                    userSession.Is_OtpVerified = user.Is_OtpVerified;
                    HttpContext.Session.SetObjectAsJson("UserSession", userSession);
                }
                else
                {
                    ViewBag.Message = "Invalid username or password.";
                }
            }
            return (userSession != null);
        }
        public IActionResult Logout()
        {
            var userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            if (userSession != null)
            {
                HttpContext.Session.Clear();
            }
            return RedirectToAction("Index");
        }
        public IActionResult ForgotPassword()
        {
            var userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
            if (userSession != null)
            {
                HttpContext.Session.Clear();
            }
            return View();
        }
        [HttpPost]
        public ServiceResponse ForgotPassword(string Username)
        {
            ServiceResponse response = new ServiceResponse();
            var Is_Validuser = _usersRepository.GetUsers(Username);
            if (Is_Validuser != null)
            {
                response.Status = true;
                TempData["Users"] = JsonConvert.SerializeObject(Is_Validuser);
                var user = _usersRepository.GetUserByUserId(Is_Validuser.User_Id);
                StoreUserSession(user);
                return response;
            }
            else
            {
                response.Status = false;
                return response;
            }
        }
        public IActionResult NewPassword()
        {
            Users user = null;
            if (TempData["Users"] != null)
            {
                user = JsonConvert.DeserializeObject<Users>(TempData["Users"].ToString());
            }

            if (user == null)
            {
                var userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");
                if (userSession == null)
                {
                    return RedirectToAction("Index");
                }

                user = new Users
                {
                    Username = userSession.Username,
                    Email = userSession.Email
                };
            }

            ViewBag.data = user;
            return PartialView();
        }
        [HttpPost]
        public ServiceResponse NewPassword(string Password, string Username)
        {
            ServiceResponse response = new ServiceResponse();
            if (!(Password == "" && Username == ""))
            {
                if (_usersRepository.UpdatePassword(Username, Password) > 0)
                {
                    
                    response.Status = true;
                    return response;
                }
            }
            response.Status = false;
            return response;
        }
        public IActionResult MicrosoftLogin()
        {
            string RedirectionURLAfterLogIn = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/signin-microsoft";

            // Store redirectURL safely in session (Azure will NOT return it)
            if (!string.IsNullOrEmpty(Request.Query["redirectURL"]))
            {
                HttpContext.Session.SetString("SSO_RedirectURL", Request.Query["redirectURL"]);
            }
            SAMLUtility _SAMLUtility = new SAMLUtility();
            string SAMLRequest = _SAMLUtility.CreateSAMLAuthRequest(MasterLookup._microsoftSSO.ApplicationID, MasterLookup._microsoftSSO.DirectoryID, RedirectionURLAfterLogIn);
            ViewBag.ActionURL = $"{MasterLookup._microsoftSSO.BaseURL}{MasterLookup._microsoftSSO.DirectoryID}/saml2?";
            string samlRequestBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(SAMLRequest));

            var model = new Dictionary<string, string>
            {
                { "SAMLRequest", samlRequestBase64 }
            };
            ViewData["Title"] = "Azure SSO";

            return View("~/Views/Shared/GenericFormPost.cshtml", model);
        }

        [HttpPost]
        [Route("signin-microsoft")]
        public IActionResult SignInMicrosoft()
        {
            return AssertionConsumerService();
        }

        [HttpPost]
        public IActionResult AssertionConsumerService()
        {
            string message = string.Empty;

            try
            {
                string samlResponse = string.Empty;
                if (HttpContext?.Request?.Form.Count > 0)
                {
                    samlResponse = HttpContext.Request.Form["SAMLResponse"];
                }

                if (string.IsNullOrEmpty(samlResponse))
                {
                    return RedirectToAction("AccessDenied");
                }

                // Decode response (for logging/debug if needed)
                string decodedResponse = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse));

                SAMLUtility samlUtility = new SAMLUtility();

                SAML_AuthnResponse authnResponse = samlUtility.ValidateSamlResponse(samlResponse, _IWebHostEnvironment.WebRootPath, $"spn:{MasterLookup._microsoftSSO.ApplicationID}", GetMetaDataXML());

                var attributes = authnResponse.Assertion.AttributeStatement.Attributes;
                var emailAddress = attributes.FirstOrDefault(a => a.Name?.Contains("emailaddress") == true)?.AttributeValue;
                var givenName = attributes.FirstOrDefault(a => a.Name?.Contains("givenname") == true)?.AttributeValue;
                var surName = attributes.FirstOrDefault(a => a.Name?.Contains("surname") == true)?.AttributeValue;

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    var authorizeUser = _usersRepository.GetUserByEmail(emailAddress);
                    UserSession userSession = new UserSession();
                    if (authorizeUser != null)
                    {
                        userSession.User_Id = authorizeUser.User_Id;
                        userSession.FirstName = authorizeUser.FirstName;
                        userSession.LastName = authorizeUser.LastName;
                        userSession.Email = authorizeUser.Email;
                        userSession.Username = authorizeUser.Username;
                        userSession.UserRoleMappingList = _usersRepository.GetUserRoleMappingByUser_Id(authorizeUser.User_Id);
                        userSession.Client_Id = authorizeUser.Client_Id;
                        userSession.ConfigurationClient_Id = authorizeUser.Client_Id;
                        userSession.CompanyName = _clientRepository.GetClients(userSession.Client_Id).CompanyName;
                        userSession.ConfigurationCompanyName = _clientRepository.GetClients(userSession.ConfigurationClient_Id).CompanyName;
                        HttpContext.Session.SetObjectAsJson("UserSession", userSession);
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied");
                    }
                    var redirectURL = HttpContext.Session.GetString("SSO_RedirectURL");

                    if (!string.IsNullOrEmpty(redirectURL))
                    {
                        HttpContext.Session.Remove("SSO_RedirectURL");
                        return Redirect(redirectURL);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("AccessDenied");
            }
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View("");
        }

        public IActionResult PasswordVerification()
        {
            var userSession = HttpContext.Session.GetObjectFromJson<UserSession>("UserSession");

            if (userSession == null)
            {
                return RedirectToAction("Index");
            }

            var model = new OtpViewModel
            {
                OtpType = "Email",
                UserEmail = userSession.Email,
            };

            return PartialView("PasswordVerification", model);
        }

        private string GetMetaDataXML()
        {
            try
            {
                return System.IO.File.ReadAllText($"{_IWebHostEnvironment.WebRootPath}/MSADSSOMetadataXMLFiles/Selectsys.xml");
            }
            catch (Exception ex) { }
            return null;
        }

    }
}
