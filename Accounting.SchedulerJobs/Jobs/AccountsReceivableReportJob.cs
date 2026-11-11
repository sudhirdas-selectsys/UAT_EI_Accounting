using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.RepositoryServices;
using Accounting.SchedulerJobs.Utilities;
using Accounting.SchedulerJobsJobs.Utilities;

namespace Accounting.SchedulerJobs.Jobs
{
    public class AccountsReceivableReportJob : ISchedulerJob
    {
        private readonly ISchedulerRepositoryService schedulerRepositoryService;
        private readonly ICommonRepositoryService commonRepositoryService;

        public AccountsReceivableReportJob(ISchedulerRepositoryService schedulerRepositoryService, ICommonRepositoryService commonRepositoryService)
        {
            this.schedulerRepositoryService = schedulerRepositoryService;
            this.commonRepositoryService = commonRepositoryService;
        }

        public async Task ExecuteAsync(SchedulerClientJobs jobConfig, CancellationToken cancellationToken, string inputField, string createdOrUpdatedBy)
        {
            try
            {
                FileLogger.LogInformation($"AccountsReceivableReportJob: ExecuteAsync Start: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                var clientSchedulerJobProcedures = await this.schedulerRepositoryService.GetClientSchedulerJobProcedures(jobConfig.SchedulerClientJob_Id);
                var schedulerClientEmailConfigurations = await this.schedulerRepositoryService.GetSchedulerClientEmailConfigurations(jobConfig.SchedulerClientJob_Id);
                var schedulerEmailProviderSettings = await this.schedulerRepositoryService.GetAccountingSchedulerEmailProviderSettings(jobConfig.Client_Id, jobConfig.SchedulerClientJob_Id);
                DateTime startTime = DateTime.Now;
                var clientLogo = jobConfig?.ClientLogo;

                string fromDateStr = string.Empty;
                string toDateStr = string.Empty;

                var invoiceReportDateRangeType = await CommonRepositoryService.GetSchedulerClientSettings(jobConfig.Client_Id, $"{Enum_SchedulerClientSettings.InvoiceReportDateRangeType.ToString()}_{jobConfig.SchedulerClientJob_Id}");
                var invoiceReportDateRange = await CommonRepositoryService.GetSchedulerClientSettings(jobConfig.Client_Id, Enum_SchedulerClientSettings.InvoiceReportDateRange.ToString());
                DateTime toDate = DateTime.Today;
                DateTime fromDate;
                switch (invoiceReportDateRangeType?.Trim().ToUpperInvariant())
                {
                    case "YTD": // Year to Date
                        fromDate = new DateTime(toDate.Year, 1, 1);
                        break;

                    case "LM": // Last Month
                        fromDate = new DateTime(toDate.Year, toDate.Month, 1).AddMonths(-1);
                        toDate = new DateTime(toDate.Year, toDate.Month, 1).AddDays(-1);
                        break;

                    case "LW": // Last Week (Monday to Sunday)
                        int daysSinceMonday = ((int)toDate.DayOfWeek + 6) % 7;
                        DateTime currentWeekStart = toDate.AddDays(-daysSinceMonday);
                        fromDate = currentWeekStart.AddDays(-7);
                        toDate = currentWeekStart.AddDays(-1);
                        break;

                    case "LY": // Last Year
                        fromDate = new DateTime(toDate.Year - 1, 1, 1);
                        toDate = new DateTime(toDate.Year - 1, 12, 31);
                        break;

                    case "TM": // This Month
                        fromDate = new DateTime(toDate.Year, toDate.Month, 1);
                        break;

                    case "TW": // This Week (Monday to today)
                        int daysFromMonday = ((int)toDate.DayOfWeek + 6) % 7;
                        fromDate = toDate.AddDays(-daysFromMonday);
                        break;

                    default: // Configured number of days
                        int days = int.TryParse(invoiceReportDateRange, out var d) && d > 0 ? d : 7;
                        fromDate = toDate.AddDays(-(days - 1));
                        break;
                }

                fromDateStr = fromDate.ToString("yyyy-MM-dd");
                toDateStr = toDate.ToString("yyyy-MM-dd");

                var reportInputData = new
                {
                    Client_Id = jobConfig.Client_Id,
                    InvoiceCreatedFromDate = fromDateStr,
                    InvoiceCreatedToDate = toDateStr,
                };

                var reportDataSet = await this.commonRepositoryService.ExecuteDyanmicGetDataSetAsync(clientSchedulerJobProcedures, reportInputData);
                if (reportDataSet != null && reportDataSet?.Tables?[0]?.Rows?.Count > 0)
                {
                    var emailAttachment = CSVUtility.DownloadCSV(reportDataSet, $"AccountsReceivableReport_{DateTime.Now:yyyyMMdd_HHmmss}");
                    List<InLineAttachmentItem> inLineAttachmentItems = null;
                    if (emailAttachment?.Count > 0)
                    {
                        inLineAttachmentItems = new List<InLineAttachmentItem>();
                        foreach (var item in emailAttachment)
                        {
                            inLineAttachmentItems.Add(new InLineAttachmentItem
                            {
                                BinaryImage = item.FileContent,
                                Name = item.FileName
                            });
                        }
                    }

                    var placeholders = Utils.GetDistinctPlaceholders(schedulerClientEmailConfigurations.EmailSubjectParameters, schedulerClientEmailConfigurations.EmailBodyParameters);
                    var reportEmailInputData = new
                    {
                        InvoiceCreatedFromDate = fromDate,
                        InvoiceCreatedToDate = toDate,
                        ReportDate = DateTime.Now,
                        ClientLogo = Convert.ToBase64String(clientLogo),
                    };

                    var emailParameters = Utils.BuildEmailParameters(reportEmailInputData, placeholders, null);
                    var sendEmail = EmailUtility.SendSchedulerEmail(jobConfig.Client_Id, jobConfig.SchedulerClientJob_Id, jobConfig.JobName, schedulerClientEmailConfigurations, emailParameters, jobConfig.CompanyName, null, inLineAttachmentItems, schedulerEmailProviderSettings);

                    await schedulerRepositoryService.InsertSchedulerJobQueue(
                        new SchedulerClientJobQueue
                        {
                            CreatedOrUpdatedBy = createdOrUpdatedBy,
                            SchedulerClientJob_Id = jobConfig.SchedulerClientJob_Id,
                            Job_Id = jobConfig.Job_Id,
                            Client_Id = jobConfig.Client_Id,
                            QueueReferenceType = "AccountsReceivableReportJob",
                            QueueReferenceValue = "AccountsReceivableReportJob",
                            QueueStatus_Id = sendEmail.EmailSent ? (int)Enum_SchedulerJobQueueStatus.Completed : (int)Enum_SchedulerJobQueueStatus.Failed,
                            ExecutionStartTime = startTime,
                            ExecutionEndTime = DateTime.Now,
                            ErrorMessage = sendEmail.EmailSent ? string.Empty : sendEmail.ErrorMessage,
                        }
                    );
                }

            }
            catch (Exception ex)
            {
                FileLogger.LogError($"AccountsReceivableReportJob: ExecuteAsync: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }
    }
}
