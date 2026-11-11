using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.RepositoryServices;
using Accounting.SchedulerJobs.Utilities;
using Accounting.SchedulerJobsJobs.Utilities;
using Newtonsoft.Json;

namespace Accounting.SchedulerJobs.Jobs
{
    public class IntentToCancelJob : ISchedulerJob
    {
        private readonly ISchedulerRepositoryService schedulerRepositoryService;
        private readonly ICommonRepositoryService commonRepositoryService;
        private readonly IInvoiceNoticeRepositoryService invoiceReminderRepositoryService;
        private readonly HttpClient httpClient;

        public IntentToCancelJob(ISchedulerRepositoryService schedulerRepositoryService, ICommonRepositoryService commonRepositoryService, IInvoiceNoticeRepositoryService invoiceReminderRepositoryService, HttpClient httpClient)
        {
            this.schedulerRepositoryService = schedulerRepositoryService;
            this.commonRepositoryService = commonRepositoryService;
            this.invoiceReminderRepositoryService = invoiceReminderRepositoryService;
            this.httpClient = httpClient;
        }

        public async Task ExecuteAsync(SchedulerClientJobs jobConfig, CancellationToken cancellationToken, string inputField, string createdOrUpdatedBy)
        {
            try
            {
                FileLogger.LogInformation($"IntentToCancelJob: ExecuteAsync Start: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                var clientName = jobConfig?.CompanyName ?? string.Empty;
                var clientLogo = jobConfig?.ClientLogo;
                var clientSchedulerJobProcedures = await this.schedulerRepositoryService.GetClientSchedulerJobProcedures(jobConfig.SchedulerClientJob_Id);
                var schedulerClientEmailConfigurations = await this.schedulerRepositoryService.GetSchedulerClientEmailConfigurations(jobConfig.SchedulerClientJob_Id);
                var schedulerEmailProviderSettings = await this.schedulerRepositoryService.GetAccountingSchedulerEmailProviderSettings(jobConfig.Client_Id,jobConfig.SchedulerClientJob_Id);

                dynamic sourceObject = Utils.ToExpandoObject(jobConfig);
                long? invoiceId = null;
                if (!string.IsNullOrEmpty(inputField))
                {
                    invoiceId = Convert.ToInt64(inputField);
                    sourceObject.Invoice_Id = invoiceId.Value;
                }

                var invoices = (List<InvoiceNoticeModel>)await this.commonRepositoryService.ExecuteDyanmicGetObjectListAsync<InvoiceNoticeModel>(clientSchedulerJobProcedures, sourceObject) ?? new List<InvoiceNoticeModel>();
                List<InvoiceNoticeModel> finalInvoices;
                if (invoiceId.HasValue)
                {
                    finalInvoices = invoices.Where(x => x.Invoice_Id == invoiceId.Value).ToList();
                }
                else
                {
                    var noticeLogs = await this.invoiceReminderRepositoryService.GetInvoiceNoticeLogs(jobConfig.Client_Id, (int)Enum_InvoiceNoticeType.IntentToCancel);
                    finalInvoices = invoices?.Where(invoice => !(noticeLogs?.Any(log => log.Invoice_Id == invoice.Invoice_Id && log?.IsEmailSent == true) ?? false)).ToList() ?? new List<InvoiceNoticeModel>();
                }

                var companyAddressSetting = await CommonRepositoryService.GetSchedulerClientSettings(jobConfig.Client_Id, Enum_SchedulerClientSettings.CompanyAddress.ToString());
                var companyWebsite = await CommonRepositoryService.GetSchedulerClientSettings(jobConfig.Client_Id, Enum_SchedulerClientSettings.CompanyWebSite.ToString());
                var paymentLink = await CommonRepositoryService.GetSchedulerClientSettings(jobConfig.Client_Id, Enum_SchedulerClientSettings.PaymentLink.ToString());
                var isDevelopmentMode = await CommonRepositoryService.GetSchedulerClientSettings(jobConfig.Client_Id, Enum_SchedulerClientSettings.IsDevelopmentMode.ToString());
                if (finalInvoices?.Count > 0)
                {
                    foreach (var invoice in finalInvoices)
                    {
                        DateTime startTime = DateTime.Now;
                        var agentEmail = invoice?.AgentEmail ?? string.Empty;
                        var invoiceNoticeLog = new InvoiceNoticeLog
                        {
                            CreatedOrUpdatedBy = createdOrUpdatedBy,
                            Job_Id = jobConfig.Job_Id,
                            SchedulerClientJob_Id = jobConfig.SchedulerClientJob_Id,
                            NoticeType_Id = (int)Enum_InvoiceNoticeType.IntentToCancel,
                            Client_Id = jobConfig.Client_Id,
                            PolicyDetails_Id = invoice?.PolicyDetails_Id,
                            Invoice_Id = invoice?.Invoice_Id,
                            InvoiceNo = invoice?.InvoiceNo,
                            EI_Document_Id = null,
                        };

                        var schedulerClientJobQueue = new SchedulerClientJobQueue
                        {
                            CreatedOrUpdatedBy = createdOrUpdatedBy,
                            SchedulerClientJob_Id = jobConfig.SchedulerClientJob_Id,
                            Job_Id = jobConfig.Job_Id,
                            Client_Id = jobConfig.Client_Id,
                            QueueReferenceType = Enum_InvoiceNoticeType.IntentToCancel.ToString(),
                            QueueReferenceValue = Convert.ToString(invoice?.Invoice_Id) ?? string.Empty,
                            ExecutionStartTime = startTime,
                        };

                        if (!string.IsNullOrEmpty(agentEmail))
                        {
                            var developmentModeValue = Utils.GetDevelopmentModeValue(isDevelopmentMode);
                            List<string> toEmail = null;
                            if (developmentModeValue is List<string> emailList && emailList.Any())
                            {
                                toEmail = emailList;
                            }
                            else if (developmentModeValue is string email && !string.IsNullOrWhiteSpace(email))
                            {
                                toEmail = new List<string> { email };
                            }

                            var placeholders = Utils.GetDistinctPlaceholders(schedulerClientEmailConfigurations.EmailSubjectParameters, schedulerClientEmailConfigurations.EmailBodyParameters);
                            var invoiceData = new
                            {
                                PolicyNumber = invoice?.PolicyNumber,
                                CompanyAddress = Utils.FormatAddress(companyAddressSetting),
                                CompanyWebSite = companyWebsite,
                                CurrentDate = DateTime.Now,
                                PaymentLink = paymentLink,
                                AgencyAddress = Utils.FormatAddress(invoice?.AgentAddress ?? string.Empty),
                                ClientLogo = Convert.ToBase64String(clientLogo),
                            };

                            var emailParameters = Utils.BuildEmailParameters(invoiceData, placeholders, null);
                            List<InLineAttachmentItem> inLineAttachmentItems = await Utils.GetInvoicePdfAttachmentsAsync(httpClient, invoice.PolicyDetails_Id, 1);

                            await EmailThrottleHelper.WaitForSlotAsync(jobConfig, cancellationToken);

                            var sendEmail = EmailUtility.SendSchedulerEmail(jobConfig.Client_Id, jobConfig.SchedulerClientJob_Id, jobConfig.JobName, schedulerClientEmailConfigurations, emailParameters, jobConfig.CompanyName, toEmail, inLineAttachmentItems, schedulerEmailProviderSettings);
                            if (sendEmail.EmailSent)
                            {
                                invoiceNoticeLog.SentOn = DateTime.Now;
                                invoiceNoticeLog.IsEmailSent = true;
                            }
                            else
                            {
                                invoiceNoticeLog.IsEmailSent = false;
                                invoiceNoticeLog.ErrorMessage = sendEmail.ErrorMessage;
                            }

                            await this.invoiceReminderRepositoryService.InsertInvoiceNoticeLog(invoiceNoticeLog);

                            schedulerClientJobQueue.QueueStatus_Id = sendEmail.EmailSent ? (int)Enum_SchedulerJobQueueStatus.Completed : (int)Enum_SchedulerJobQueueStatus.Failed;
                            schedulerClientJobQueue.ExecutionEndTime = DateTime.Now;
                            schedulerClientJobQueue.ErrorMessage = sendEmail.EmailSent ? string.Empty : sendEmail.ErrorMessage;

                            await schedulerRepositoryService.InsertSchedulerJobQueue(schedulerClientJobQueue);
                        }
                        else
                        {
                            string message = "Email Not Found for the Agency";
                            invoiceNoticeLog.IsEmailSent = false;
                            invoiceNoticeLog.ErrorMessage = message;
                            await this.invoiceReminderRepositoryService.InsertInvoiceNoticeLog(invoiceNoticeLog);

                            schedulerClientJobQueue.QueueStatus_Id = (int)Enum_SchedulerJobQueueStatus.Failed;
                            schedulerClientJobQueue.ExecutionEndTime = DateTime.Now;
                            schedulerClientJobQueue.ErrorMessage = message;

                            await schedulerRepositoryService.InsertSchedulerJobQueue(schedulerClientJobQueue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"IntentToCancelJob: ExecuteAsync: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }
    }
}
