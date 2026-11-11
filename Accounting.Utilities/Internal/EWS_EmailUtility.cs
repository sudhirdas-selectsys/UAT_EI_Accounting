using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Identity.Client;
using System.Net;

namespace Accounting.Utilities.Internal
{
    public class UserAccountInfo
    {
        public string ResourceID { get; set; }
        public string EmailAddress { get; set; }
        public string Domain { get; set; }
        public bool EWS_OAUTH { get; set; } = true;
        public string EWS_URL { get; set; }
        public string EWS_UserName { get; set; }
        public string EWS_Password { get; set; }
    }
    public class InLineAttachmentItem
    {
        public bool IsInline { get; set; }
        public byte[] BinaryImage { get; set; }
        public string Name { get; set; }
        public string ContentID { get; set; }
    }
    public class EWS_EmailUtility
    {
        public string Token { get; set; }
        public string App_Id { get; set; }
        public string App_IdPW { get; set; }
        public string Support_CCEmailAddress { get; set; }
        public string EI_NoReplyEmailAddress { get; set; }
        public string Service_URL { get; set; }

        public EWS_EmailUtility()
        {
            Service_URL = MasterLookup._EI_EWSDetails.Service_URL;
            Token = MasterLookup._EI_EWSDetails.Token;
            App_Id = MasterLookup._EI_EWSDetails.App_Id;
            App_IdPW = MasterLookup._EI_EWSDetails.App_IdPW;
            Support_CCEmailAddress = MasterLookup._EI_EWSDetails.Support_CCEmailAddress;
            EI_NoReplyEmailAddress = MasterLookup._EI_EWSDetails.EI_NoReplyEmailAddress;
        }
        
        public UserAccountInfo GetUserAccountInfo(string emailAddress)
        {
            return new UserAccountInfo
            {
                ResourceID = "",
                EmailAddress = emailAddress,
                Domain = emailAddress.Split('@')[1],
                EWS_OAUTH = true,
                EWS_URL = Service_URL,
                EWS_UserName = "",
                EWS_Password = ""
            };
        }
        public string getToken(string tenantID)
        {
            return getToken(tenantID, false);
        }
        public string getToken(string tenantID, bool noCache)
        {
            string cacheID = Token + tenantID;
            string returnValue = null;

            var ar = getNewToken(tenantID).Result;
            returnValue = ar.AccessToken;
            var exp = ar.ExpiresOn;

            return returnValue;
        }
        private async Task<AuthenticationResult> getNewToken(string tenantID)
        {
            //string appID = ExpertInsure.Data.Core.Provider.ApplicationSetting.Get("EWSAppID", "");
            //string clientSecret = ExpertInsure.Data.Core.Provider.ApplicationSetting.Get("EWSAppIDPW", "");

            System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)3072;
            var cca = ConfidentialClientApplicationBuilder
               .Create(App_Id)
               .WithClientSecret(App_IdPW)
               .WithTenantId(tenantID)
               .WithLegacyCacheCompatibility(false)
               .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
               .Build();
            var ewsScopes = new string[] { "https://outlook.office365.com/.default" };

            var authResult = await cca.AcquireTokenForClient(ewsScopes)
                    .ExecuteAsync();
            return authResult;
        }
        private ExchangeService GetExchangeServiceInstance(UserAccountInfo info)
        {
            ExchangeService service;

            if (info.EWS_OAUTH)
            {
                string token = getToken(info.Domain);

                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, cert, chain, errors) => true;

                service = new ExchangeService(ExchangeVersion.Exchange2013_SP1)
                {
                    Credentials = new OAuthCredentials(token),
                    Url = new Uri(info.EWS_URL),
                    ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, info.EmailAddress)
                };

                service.HttpHeaders.Add("X-AnchorMailbox", info.EmailAddress);

                // Same as your Folder.Bind validation
                _ = Folder.Bind(service, WellKnownFolderName.MsgFolderRoot);
            }
            else
            {
                // Legacy fallback (you probably won’t use this)
                service = new ExchangeService(ExchangeVersion.Exchange2013)
                {
                    Credentials = new WebCredentials(info.EWS_UserName, info.EWS_Password),
                    Url = new Uri(info.EWS_URL)
                };

                _ = Folder.Bind(service, WellKnownFolderName.MsgFolderRoot);
            }

            return service;
        }
        public void SetExtendedProperty(ref EmailMessage message, string name, string value)
        {
            Guid myPropertySetId = new Guid("{20B5C09F-7CAD-44c6-BDBF-8FCBEEA08544}");
            ExtendedPropertyDefinition myExtendedPropertyDefinition = new ExtendedPropertyDefinition(myPropertySetId, name, MapiPropertyType.String);
            message.SetExtendedProperty(myExtendedPropertyDefinition, value);
        }
        public void SendExceptionReport(string Subject, string message, string clientName, string ccAddress, string toAddress, string bccAddress, List<InLineAttachmentItem> InLineAttachmentItem = null)
        {
            UserAccountInfo info = GetUserAccountInfo(EI_NoReplyEmailAddress);
            if (info == null) { return; }
            ExchangeService srv = GetExchangeServiceInstance(info);
            EmailMessage msg = new EmailMessage(srv);
            if (!string.IsNullOrEmpty(toAddress))
            {
                var toItems = toAddress.Split(';');
                foreach (var to in toItems)
                {
                    msg.ToRecipients.Add(to);
                }

            }
            if (!string.IsNullOrEmpty(bccAddress))
            {
                var bccItems = bccAddress.Split(';');
                foreach (var bcc in bccItems)
                {
                    msg.BccRecipients.Add(bcc);
                }
                    
            }
            if (!string.IsNullOrEmpty(ccAddress))
            {
                var ccItems = ccAddress.Split(';');
                foreach (var cc in ccItems)
                {
                    msg.CcRecipients.Add(cc);
                }
            }
            msg.Subject = Subject;
            if (InLineAttachmentItem?.Count > 0)
            {
                msg.Body = new MessageBody(BodyType.HTML, message);

                foreach (var a in InLineAttachmentItem)
                {
                    FileAttachment attachment = msg.Attachments.AddFileAttachment(a.Name, a.BinaryImage);
                    attachment.IsInline = true;
                    attachment.ContentId = a.ContentID;
                }
            }
            else
            {
                msg.Body = message;
            }

            SetExtendedProperty(ref msg, "EI_EXCEPTION", clientName);

            msg.Send();
        }
    }
}
