using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.RepositoryServices;
using Accounting.SchedulerJobs.Utilities;
using Newtonsoft.Json;

namespace Accounting.SchedulerJobs.Services
{
    public class EmailAttachmentService : IEmailAttachmentService
    {
        public async Task<List<EmailAttachmentItem>> BuildAttachmentsAsync<T>(List<SchedulerJobEmailAttachments> attachmentConfigs, T data, byte[] clientLogo = null)
        {
            var attachments = new List<EmailAttachmentItem>();
            try
            {
                FileLogger.LogInformation($"EmailAttachmentService: BuildAttachmentsAsync Started");
                if (attachmentConfigs == null || !attachmentConfigs.Any())
                {
                    return attachments;
                }

                foreach (var config in attachmentConfigs)
                {
                    var parameters = JsonConvert.DeserializeObject<List<string>>(config.AttachmentBodyParameters ?? "[]");
                    var replaceValues = Utilities.Utils.BuildEmailParameters(data, parameters);
                    string body = config.AttachmentBody ?? string.Empty;
                    foreach (var item in replaceValues)
                    {
                        body = body.Replace(item.Key, item.Value ?? string.Empty);
                    }

                    byte[] fileBytes = GenerateAttachment(config?.AttachmentType, body, clientLogo);
                    if (fileBytes != null)
                    {
                        attachments.Add(new EmailAttachmentItem
                        {
                            FileName = $"{config.AttachmentName}_{DateTime.Now:yyyyMMdd_HHmmss}.{config?.AttachmentType?.ToLower()}",
                            FileContent = fileBytes,
                            ContentType = GetContentType(config?.AttachmentType)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailAttachmentService: BuildAttachmentsAsync Exception: {ex} Message: {ex.Message}");
            }

            return attachments;
        }


        public async Task<object> BuildInvoiceNoticeAttachmentDataAsync(long clientId, string agentAddress, byte[] clientLogo)
        {
            try
            {
                FileLogger.LogInformation($"EmailAttachmentService: BuildInvoiceNoticeAttachmentDataAsync Started");
                var companyAddressSetting = await CommonRepositoryService.GetSchedulerClientSettings(clientId, Enum_SchedulerClientSettings.CompanyAddress.ToString());
                var companyWebsite = await CommonRepositoryService.GetSchedulerClientSettings(clientId, Enum_SchedulerClientSettings.CompanyWebSite.ToString());
                var paymentLink = await CommonRepositoryService.GetSchedulerClientSettings(clientId, Enum_SchedulerClientSettings.PaymentLink.ToString());
                string companyAddress = Utils.FormatAddress(companyAddressSetting);
                string formattedAgentAddress = Utils.FormatAddress(agentAddress);
                return new
                {
                    CompanyAddress = companyAddress,
                    CompanyWebsite = companyWebsite,
                    PaymentLink = paymentLink,
                    AgencyAddress = formattedAgentAddress,
                    CurrentDate = DateTime.Now,
                };
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailAttachmentService: BuildInvoiceNoticeAttachmentDataAsync Exception: {ex} Message: {ex.Message}");
                return null;
            }
        }

        private byte[] GenerateAttachment(string attachmentType, string content, byte[] clientLogo = null)
        {
            try
            {
                switch (attachmentType?.ToUpper())
                {
                    case "HTML":
                        return System.Text.Encoding.UTF8.GetBytes(content);

                    case "TXT":
                        return System.Text.Encoding.UTF8.GetBytes(content);

                    case "PDF":
                        return HtmlPdfConverter.ConvertToPdf(content, clientLogo);

                    default:
                        return System.Text.Encoding.UTF8.GetBytes(content);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetContentType(string attachmentType)
        {
            try
            {
                return attachmentType?.ToUpper() switch
                {
                    "PDF" => "application/pdf",
                    "HTML" => "text/html",
                    "TXT" => "text/plain",
                    _ => "application/octet-stream"
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}