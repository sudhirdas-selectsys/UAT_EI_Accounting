using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Services;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;


namespace Accounting.SchedulerJobsJobs.Utilities
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

    public class EmailUtility
    {
        public static UserAccountInfo GetUserAccountInfo(string emailAddress, string emailServiceURL)
        {
            return new UserAccountInfo
            {
                ResourceID = "",
                EmailAddress = emailAddress,
                Domain = emailAddress.Split('@')[1],
                EWS_OAUTH = true,
                EWS_URL = emailServiceURL,
                EWS_UserName = "",
                EWS_Password = ""
            };
        }

        public static string getToken(string tenantID, string emailToken, string emailAppId, string emailAppPassword)
        {
            return getToken(tenantID, false, emailToken, emailAppId, emailAppPassword);
        }
        public static string getToken(string tenantID, bool noCache, string emailToken, string emailAppId, string emailAppPassword)
        {
            try
            {
                FileLogger.LogInformation($"EmailUtility: getToken Started.");
                string cacheID = emailToken + tenantID;
                string returnValue = null;
                var ar = getNewToken(tenantID, emailAppId, emailAppPassword).Result;
                returnValue = ar.AccessToken;
                var exp = ar.ExpiresOn;
                return returnValue;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailUtility: getToken: TenantId: {tenantID}: NoCache: {noCache}: Error While Getting Token : {ex}");
                throw;
            }
        }
        private static async Task<AuthenticationResult> getNewToken(string tenantID, string emailAppId, string emailAppPassword)
        {
            try
            {
                FileLogger.LogInformation($"EmailUtility: getNewToken Started.");
                System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)3072;
                var cca = ConfidentialClientApplicationBuilder
                   .Create(emailAppId)
                   .WithClientSecret(emailAppPassword)
                   .WithTenantId(tenantID)
                   .WithLegacyCacheCompatibility(false)
                   .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                   .Build();
                var ewsScopes = new string[] { "https://outlook.office365.com/.default" };

                var authResult = await cca.AcquireTokenForClient(ewsScopes)
                        .ExecuteAsync();
                return authResult;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailUtility: getNewToken: TenantId: {tenantID}: Error While Getting Token : {ex}");
                throw;
            }
        }

        private static ExchangeService GetExchangeServiceInstance(UserAccountInfo info, string emailToken, string emailAppId, string emailAppPassword)
        {
            try
            {
                FileLogger.LogInformation($"EmailUtility: GetExchangeServiceInstance Started.");
                ExchangeService service;
                if (info.EWS_OAUTH)
                {
                    string token = getToken(info.Domain, emailToken, emailAppId, emailAppPassword);

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
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailUtility: GetExchangeServiceInstance: Info: {JsonConvert.SerializeObject(info)}: Error Processing : {ex}");
                throw;
            }
        }

        public static void SetExtendedProperty(ref EmailMessage message, string name, string value)
        {
            try
            {
                FileLogger.LogInformation($"EmailUtility: SetExtendedProperty Started.");
                Guid myPropertySetId = new Guid("{20B5C09F-7CAD-44c6-BDBF-8FCBEEA08544}");
                ExtendedPropertyDefinition myExtendedPropertyDefinition = new ExtendedPropertyDefinition(myPropertySetId, name, MapiPropertyType.String);
                message.SetExtendedProperty(myExtendedPropertyDefinition, value);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailUtility: SetExtendedProperty: Name: {name}: Value: {value}: Error Processing : {ex}");
                throw;
            }
        }

        public static (bool EmailSent, string ErrorMessage) SendSchedulerEmail(long client_Id, long SchedulerClientJob_Id, string jobName, SchedulerClientEmailConfigurations config, Dictionary<string, string> emailParameters, string clientName, List<string> additionalToEmails = null, List<InLineAttachmentItem> inLineAttachmentItems = null, Dictionary<string, SchedulerClientJobEmailSettings> schedulerEmailProviderSettings = null)
        {
            string errorMessage = string.Empty;
            try
            {
                FileLogger.LogInformation($"EmailUtility: SendSchedulerEmail Started: Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName}");
                if (schedulerEmailProviderSettings.TryGetValue(nameof(Enum_SchedulerEmailOptions.EWS), out var ewsSettings))
                {
                    return SendEWSSchedulerEmail(client_Id, SchedulerClientJob_Id, jobName, config, emailParameters, clientName, additionalToEmails, inLineAttachmentItems, schedulerEmailProviderSettings);
                }
                else if (schedulerEmailProviderSettings.TryGetValue(nameof(Enum_SchedulerEmailOptions.SMTP), out var smtpSettings))
                {
                    return SendSMTPSchedulerEmail(client_Id, SchedulerClientJob_Id, jobName, config, emailParameters, clientName, additionalToEmails, inLineAttachmentItems, schedulerEmailProviderSettings);
                }
                else
                {
                    return (false, $"No email provider configuration found for Client_Id: {client_Id}, SchedulerClientJob_Id: {SchedulerClientJob_Id}, JobName: {jobName}. Expected either SMTP or EWS settings.");
                }
            }
            catch (Exception ex)
            {
                string error = $"EmailUtility: SendSchedulerEmail: Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName} Error While Processing {ex}";
                FileLogger.LogError(error);
                return (false, error);
            }
        }

        private static (bool EmailSent, string ErrorMessage) SendEWSSchedulerEmail(long client_Id, long SchedulerClientJob_Id, string jobName, SchedulerClientEmailConfigurations config, Dictionary<string, string> emailParameters, string clientName, List<string> additionalToEmails = null, List<InLineAttachmentItem> inLineAttachmentItems = null, Dictionary<string, SchedulerClientJobEmailSettings> schedulerEmailProviderSettings = null)
        {
            string errorMessage = string.Empty;
            try
            {
                FileLogger.LogInformation($"EmailUtility: SendEWSSchedulerEmail Started: Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName}");
                if (config == null)
                {
                    return (false, $"Email Configuration Not Found for Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName}");
                }

                string serviceUrl = string.Empty, token = string.Empty, emailAddress = string.Empty, emailAppId = string.Empty, emailAppPassword = string.Empty;
                if (schedulerEmailProviderSettings.TryGetValue(nameof(Enum_SchedulerEmailOptions.EWS), out var ewsSettings))
                {
                    serviceUrl = ewsSettings?.schedulerClientJobEWSSettings?.ServiceUrl ?? string.Empty;
                    token = ewsSettings?.schedulerClientJobEWSSettings?.Token ?? string.Empty;
                    emailAddress = ewsSettings?.schedulerClientJobEWSSettings?.EmailAddress ?? string.Empty;
                    emailAppId = ewsSettings?.schedulerClientJobEWSSettings?.App_Id ?? string.Empty;
                    emailAppPassword = ewsSettings?.schedulerClientJobEWSSettings?.AppPassword ?? string.Empty;
                }

                UserAccountInfo info = GetUserAccountInfo(emailAddress, serviceUrl);

                if (info == null)
                {
                    return (false, $"Email UserAccountInfo Not Found for Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName}");
                }

                var missingFields = new List<string>();
                if (string.IsNullOrEmpty(serviceUrl))
                {
                    missingFields.Add(nameof(serviceUrl));
                }

                if (string.IsNullOrEmpty(token))
                {
                    missingFields.Add(nameof(token));
                }

                if (string.IsNullOrEmpty(emailAddress))
                {
                    missingFields.Add(nameof(emailAddress));
                }

                if (string.IsNullOrEmpty(emailAppId))
                {
                    missingFields.Add(nameof(emailAppId));
                }

                if (string.IsNullOrEmpty(emailAppPassword))
                {
                    missingFields.Add(nameof(emailAppPassword));
                }

                if (missingFields.Any())
                {
                    return (false, $"Required EWS Email Configuration is Missing for Client_Id: {client_Id}, SchedulerClientJob_Id: {SchedulerClientJob_Id}, JobName: {jobName}. Missing fields: {string.Join(", ", missingFields)}.");
                }

                ExchangeService srv = GetExchangeServiceInstance(info, token, emailAppId, emailAppPassword);
                EmailMessage msg = new EmailMessage(srv);
                EmailRecipients recipients = JsonConvert.DeserializeObject<EmailRecipients>(config?.EmailConfiguration ?? string.Empty);
                string subject = ReplaceEmailParameters(config?.EmailSubject, emailParameters);
                string body = ReplaceEmailParameters(config?.EmailBody, emailParameters);
                if (recipients?.To?.Count > 0)
                {
                    foreach (string to in recipients.To)
                    {
                        if (!string.IsNullOrWhiteSpace(to))
                        {
                            msg.ToRecipients.Add(to);
                        }
                    }
                }

                if (additionalToEmails?.Count > 0)
                {
                    foreach (string to in additionalToEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(to))
                        {
                            msg.ToRecipients.Add(to);
                        }
                    }
                }

                if (recipients?.CC?.Count > 0)
                {
                    foreach (string cc in recipients.CC)
                    {
                        if (!string.IsNullOrWhiteSpace(cc))
                        {
                            msg.CcRecipients.Add(cc);
                        }
                    }
                }

                if (recipients?.BCC?.Count > 0)
                {
                    foreach (string bcc in recipients.BCC)
                    {
                        if (!string.IsNullOrWhiteSpace(bcc))
                        {
                            msg.BccRecipients.Add(bcc);
                        }
                    }
                }

                msg.Subject = subject;

                if (inLineAttachmentItems?.Count > 0)
                {
                    msg.Body = new MessageBody(BodyType.HTML, body);
                    foreach (var attachmentItem in inLineAttachmentItems)
                    {
                        FileAttachment attachment = msg.Attachments.AddFileAttachment(attachmentItem.Name, attachmentItem.BinaryImage);
                        attachment.IsInline = true;
                        attachment.ContentId = attachmentItem.ContentID;
                    }
                }
                else
                {
                    msg.Body = new MessageBody(BodyType.HTML, body);
                }

                SetExtendedProperty(ref msg, "EI_EXCEPTION", clientName);

                msg.Send();
                return (true, "S");
            }
            catch (Exception ex)
            {
                string error = $"EmailUtility: SendEWSSchedulerEmail: Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName} Error While Processing {ex}";
                FileLogger.LogError(error);
                return (false, error);
            }
        }

        private static (bool EmailSent, string ErrorMessage) SendSMTPSchedulerEmail(long client_Id, long SchedulerClientJob_Id, string jobName, SchedulerClientEmailConfigurations config, Dictionary<string, string> emailParameters, string clientName, List<string> additionalToEmails = null, List<InLineAttachmentItem> inLineAttachmentItems = null, Dictionary<string, SchedulerClientJobEmailSettings> schedulerEmailProviderSettings = null)
        {
            string errorMessage = string.Empty;
            try
            {
                FileLogger.LogInformation($"EmailUtility: SendSMTPSchedulerEmail Started: Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName}");

                if (config == null)
                {
                    return (false, $"Email Configuration Not Found for Client_Id: {client_Id}");
                }

                string smtpHost = string.Empty, emailAddress = string.Empty, smtpUsername = string.Empty, smtpPassword = string.Empty, smtpDisplayName = string.Empty;
                int smtpPort = 0;
                bool smtpEnableSSL = false;
                if (schedulerEmailProviderSettings.TryGetValue(nameof(Enum_SchedulerEmailOptions.SMTP), out var smtpSettings))
                {
                    smtpHost = smtpSettings?.schedulerClientJobSMTPSettings?.SMTPHost ?? string.Empty;
                    emailAddress = smtpSettings?.schedulerClientJobSMTPSettings?.EmailAddress ?? string.Empty;
                    smtpPort = smtpSettings?.schedulerClientJobSMTPSettings?.SMTPPort ?? 0;
                    smtpUsername = smtpSettings?.schedulerClientJobSMTPSettings?.UserName ?? string.Empty;
                    smtpPassword = smtpSettings?.schedulerClientJobSMTPSettings?.Password ?? string.Empty;
                    smtpDisplayName = smtpSettings?.schedulerClientJobSMTPSettings?.DisplayName ?? string.Empty;
                    smtpEnableSSL = smtpSettings?.schedulerClientJobSMTPSettings?.EnableSSL ?? false;
                }

                var missingFields = new List<string>();
                if (string.IsNullOrEmpty(smtpHost))
                {
                    missingFields.Add(nameof(smtpHost));
                }

                if (string.IsNullOrEmpty(emailAddress))
                {
                    missingFields.Add(nameof(emailAddress));
                }

                if (string.IsNullOrEmpty(emailAddress))
                {
                    missingFields.Add(nameof(emailAddress));
                }

                if (smtpPort == 0)
                {
                    missingFields.Add(nameof(smtpPort));
                }

                if (string.IsNullOrEmpty(smtpUsername))
                {
                    missingFields.Add(nameof(smtpUsername));
                }

                if (string.IsNullOrEmpty(smtpPassword))
                {
                    missingFields.Add(nameof(smtpPassword));
                }

                if (missingFields.Any())
                {
                    return (false, $"Required SMTP Email Configuration is Missing for Client_Id: {client_Id}, SchedulerClientJob_Id: {SchedulerClientJob_Id}, JobName: {jobName}. Missing fields: {string.Join(", ", missingFields)}.");
                }


                EmailRecipients recipients = JsonConvert.DeserializeObject<EmailRecipients>(config.EmailConfiguration ?? string.Empty);
                string subject = ReplaceEmailParameters(config.EmailSubject, emailParameters);
                string body = ReplaceEmailParameters(config.EmailBody, emailParameters);
                if (!string.IsNullOrEmpty(smtpDisplayName))
                {
                    clientName = smtpDisplayName;
                }
                using (MailMessage mail = new MailMessage())
                {
                    // From Address
                    mail.From = new MailAddress(smtpUsername, clientName);

                    // To
                    if (recipients?.To != null)
                    {
                        foreach (string to in recipients.To.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            mail.To.Add(to);
                        }
                    }

                    // Additional To
                    if (additionalToEmails != null)
                    {
                        foreach (string to in additionalToEmails.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            mail.To.Add(to);
                        }
                    }

                    // CC
                    if (recipients?.CC != null)
                    {
                        foreach (string cc in recipients.CC.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            mail.CC.Add(cc);
                        }
                    }

                    // BCC
                    if (recipients?.BCC != null)
                    {
                        foreach (string bcc in recipients.BCC.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            mail.Bcc.Add(bcc);
                        }
                    }

                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    // Inline Attachments

                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    if (inLineAttachmentItems?.Count > 0)
                    {
                        foreach (var attachmentItem in inLineAttachmentItems)
                        {
                            MemoryStream ms = new MemoryStream(attachmentItem.BinaryImage);
                            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(ms, attachmentItem.Name);

                            // Optional: preserve the ContentId if you use it elsewhere
                            attachment.ContentId = attachmentItem.ContentID;
                            mail.Attachments.Add(attachment);
                        }
                    }

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = smtpHost;
                        smtp.Port = smtpPort;
                        smtp.EnableSsl = smtpEnableSSL;
                        smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                        smtp.Send(mail);
                    }
                }

                return (true, "S");
            }
            catch (Exception ex)
            {
                string error = $"EmailUtility: SendSMTPSchedulerEmail: Client_Id: {client_Id}: SchedulerClientJob_Id: {SchedulerClientJob_Id}: jobName: {jobName} Error While Processing {ex}";
                FileLogger.LogError(error);
                return (false, error);
            }
        }

        private static string ReplaceEmailParameters(string content, Dictionary<string, string> emailParameters)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return string.Empty;
                }

                if (emailParameters == null || emailParameters.Count == 0)
                {
                    return content;
                }

                foreach (var parameter in emailParameters)
                {
                    content = content.Replace(parameter.Key, parameter.Value ?? string.Empty);
                }

                return content;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
