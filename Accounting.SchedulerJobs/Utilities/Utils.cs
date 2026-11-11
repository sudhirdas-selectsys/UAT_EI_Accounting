using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Models.MakePayments;
using Accounting.SchedulerJobs.RepositoryServices;
using Accounting.SchedulerJobsJobs.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Dynamic;
using System.Reflection;

namespace Accounting.SchedulerJobs.Utilities
{
    public class Utils
    {
        public static Dictionary<string, string> BuildEmailParameters<T>(T source, IEnumerable<string> placeholders, Dictionary<string, string> additionalParameters = null)
        {
            Dictionary<string, string> parameters = new();

            try
            {
                FileLogger.LogInformation($"Utils: BuildEmailParameters Started");

                if (source != null)
                {
                    Type sourceType = source.GetType();
                    foreach (string placeholder in placeholders ?? Enumerable.Empty<string>())
                    {
                        string propertyName = placeholder.Replace("[", "").Replace("]", "");
                        var property = sourceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (property != null)
                        {
                            object value = property.GetValue(source);
                            parameters[placeholder] = value switch
                            {
                                DateTime dateTime when propertyName is "ExecutionStartTime" or "ExecutionEndTime"
                                    => dateTime.ToString(),

                                DateTime dateTime
                                    => dateTime.ToString("MM/dd/yyyy"),

                                _ => value?.ToString() ?? string.Empty
                            };
                        }
                    }
                }

                if (additionalParameters != null)
                {
                    foreach (var item in additionalParameters)
                    {
                        parameters[item.Key] = item.Value ?? string.Empty;
                    }
                }

                return parameters;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"Utils: BuildEmailParameters Exception: {ex} Message: {ex.Message}");
                return parameters;
            }
        }

        public static string FormatAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return string.Empty;
            }

            try
            {
                var addressList = JsonConvert.DeserializeObject<List<string>>(address);
                if (addressList?.Count > 0)
                {
                    return string.Join("<br>", addressList);
                }
            }
            catch
            {
                return string.Join("<br>", address.Split(',', StringSplitOptions.RemoveEmptyEntries));
            }

            return string.Empty;
        }

        public static ResponseViewModel<T> ResponseMapping<T>(int status, string statusDescription, T response, List<ResponseViewModel<T>.Errors> error)
        {
            ResponseViewModel<T> res = new ResponseViewModel<T>();
            res.Status = status;
            res.StatusDescription = statusDescription;
            res.Response = response;
            res.Error = error;
            return res;
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(
                    prop.Name,
                    Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = properties.Select(p => p.GetValue(item)).ToArray();
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static List<string> GetDistinctPlaceholders(string? subjectParameters, string? bodyParameters)
        {
            var placeholders = new List<string>();
            if (!string.IsNullOrWhiteSpace(subjectParameters))
            {
                placeholders.AddRange(JsonConvert.DeserializeObject<List<string>>(subjectParameters) ?? new List<string>());
            }
            if (!string.IsNullOrWhiteSpace(bodyParameters))
            {
                placeholders.AddRange(JsonConvert.DeserializeObject<List<string>>(bodyParameters) ?? new List<string>());
            }
            return placeholders.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static async Task<List<InLineAttachmentItem>> GetInvoicePdfAttachmentsAsync(HttpClient httpClient, long policyDetailsId, int installmentNo = 1)
        {
            try
            {
                FileLogger.LogInformation($"Utils: GetInvoicePdfAttachmentsAsync Started");
                var invoicePdf = await RestExtension.GetAsync(httpClient, $"{ConfigurationHelper.Accounting_Base_URL}api/CommonService/GenerateInvoicePDF?PolicyDetails_Id={policyDetailsId}&InstallmentNo={installmentNo}", ConfigurationHelper.SchedulerJobAuthToken, "Authorization", false);
                if (invoicePdf?.StatusCode != 200)
                {
                    return null;
                }

                var result = invoicePdf.Result as JObject;
                string base64String = result?["Data"]?["Base64String"].ToString();
                string invoicePdfName = result?["Data"]?["Name"].ToString();

                if (string.IsNullOrWhiteSpace(base64String))
                {
                    return null;
                }

                return new List<InLineAttachmentItem>
                {
                    new InLineAttachmentItem
                    {
                        BinaryImage = Convert.FromBase64String(base64String),
                        Name = invoicePdfName
                    }
                };
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"Utils: GetInvoicePdfAttachmentsAsync Exception: {ex} Message: {ex.Message}");
                return null;
            }
        }

        public static ExpandoObject ToExpandoObject<T>(T source)
        {
            try
            {
                if (source == null)
                {
                    return null;
                }

                IDictionary<string, object> expando = new ExpandoObject();

                foreach (PropertyInfo property in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    expando[property.Name] = property.GetValue(source);
                }

                return (ExpandoObject)expando;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"Utils: ToExpandoObject Exception: {ex} Message: {ex.Message}");
                return null;
            }
        }
        public static object GetDevelopmentModeValue(string settingValue)
        {
            if (string.IsNullOrEmpty(settingValue))
            {
                return string.Empty;
            }

            try
            {
                if (settingValue.StartsWith("["))
                {
                    return JsonConvert.DeserializeObject<List<string>>(settingValue)?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
                }

                return settingValue;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"Utils: GetDevelopmentModeValue Exception: {ex} Message: {ex.Message}");
                return settingValue;
            }
        }

        public static async Task SendExceptionEmail(SchedulerClientJobs jobConfig, string createdOrUpdatedBy, DateTime startTime, Exception ex)
        {
            try
            {
                FileLogger.LogInformation($"Utils: SendExceptionEmail Started : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                var exceptionEmailConfiguration = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.ExceptionEmailConfiguration.ToString());
                var exceptionEmailSubject = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.ExceptionEmailSubject.ToString());
                var exceptionEmailBody = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.ExceptionEmailBody.ToString());
                var exceptionEmailSubjectParameters = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.ExceptionEmailSubjectParameters.ToString());
                var exceptionEmailBodyParameters = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.ExceptionEmailBodyParameters.ToString());
                var placeholders = Utils.GetDistinctPlaceholders(exceptionEmailSubjectParameters, exceptionEmailBodyParameters);
                var eWSEmailServiceURL = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailServiceURL.ToString());
                var eWSEmailToken = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailToken.ToString());
                var eWSEmailAppId = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailAppId.ToString());
                var eWSEmailAppPassword = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailAppPassword.ToString());
                var eWSEmailAddress = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailAddress.ToString());

                var exceptionData = new
                {
                    JobName = jobConfig?.JobName ?? string.Empty,
                    Job_Id = jobConfig?.Job_Id ?? 0,
                    SchedulerClientJob_Id = jobConfig?.SchedulerClientJob_Id ?? 0,
                    Client_Id = jobConfig?.Client_Id ?? 0,
                    CompanyName = jobConfig?.CompanyName ?? string.Empty,
                    ScheduledDate = DateTime.Now,
                    ExecutionStartTime = startTime,
                    ExecutionEndTime = DateTime.Now,
                    ExecutedBy = createdOrUpdatedBy,
                    ErrorMessage = ex?.Message ?? string.Empty,
                    StackTrace = ex?.ToString() ?? string.Empty,
                };

                var emailParameters = Utils.BuildEmailParameters(exceptionData, placeholders, null);
                var schedulerClientEmailConfigurations = new SchedulerClientEmailConfigurations
                {
                    Job_Id = jobConfig?.Job_Id ?? 0,
                    SchedulerClientJob_Id = jobConfig?.SchedulerClientJob_Id ?? 0,
                    EmailConfiguration = exceptionEmailConfiguration ?? string.Empty,
                    EmailSubject = exceptionEmailSubject ?? string.Empty,
                    EmailBody = exceptionEmailBody ?? string.Empty,
                };

                var schedulerEmailProviderSettings = new Dictionary<string, SchedulerClientJobEmailSettings>
                {
                    [nameof(Enum_SchedulerEmailOptions.EWS)] = new SchedulerClientJobEmailSettings
                    {
                        schedulerClientJobEWSSettings = new SchedulerClientJobEWSSettings
                        {
                            ServiceUrl = eWSEmailServiceURL ?? string.Empty,
                            Token = eWSEmailToken ?? string.Empty,
                            App_Id = eWSEmailAppId ?? string.Empty,
                            AppPassword = eWSEmailAppPassword ?? string.Empty,
                            EmailAddress = eWSEmailAddress ?? string.Empty,
                        }
                    }
                };

                var sendEmail = EmailUtility.SendSchedulerEmail(jobConfig.Client_Id, jobConfig.SchedulerClientJob_Id, jobConfig.JobName, schedulerClientEmailConfigurations, emailParameters, jobConfig.CompanyName, null, null, schedulerEmailProviderSettings);
                if (!sendEmail.EmailSent)
                {
                    FileLogger.LogInformation($"Utils: SendExceptionEmail Failed: ErrorMessage: {sendEmail.ErrorMessage} : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                }
            }
            catch (Exception exception)
            {
                FileLogger.LogError($"Utils: SendExceptionEmail Exception: {exception} Message: {exception.Message}");
            }
        }
        public static async Task SendBatchFailEmail(SchedulerClientJobs jobConfig, BillProcessQueue queue, string entityName)
        {
            try
            {
                FileLogger.LogInformation($"Utils: SendBatchFailEmail Started : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                var billBatchEmailConfiguration = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.BillBatchEmailConfiguration.ToString());
                var billbatchEmailSubject = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.BillBatchEmailSubject.ToString());
                var billBatchFailEmailBody = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.BillBatchFailEmailBody.ToString());
                var billBatchFailEmailBodyParameters = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.BillBatchFailEmailBodyParameters.ToString());
                var placeholders = Utils.GetDistinctPlaceholders(null, billBatchFailEmailBodyParameters);
                var eWSEmailServiceURL = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailServiceURL.ToString());
                var eWSEmailToken = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailToken.ToString());
                var eWSEmailAppId = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailAppId.ToString());
                var eWSEmailAppPassword = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailAppPassword.ToString());
                var eWSEmailAddress = await CommonRepositoryService.GetSchedulerApplicationSettings(Enum_SchedulerApplicationSettings.EWSEmailAddress.ToString());
                var billBatchFailData = new
                {
                    Entity_Name = entityName,
                    BillNumber = queue?.BillNumber ?? string.Empty,
                    BillId = queue?.Bill_Id ?? 0
                };
                var emailParameters = Utils.BuildEmailParameters(billBatchFailData, placeholders, null);
                var billBatchEmailConfigurations = new SchedulerClientEmailConfigurations
                {
                    Job_Id = jobConfig?.Job_Id ?? 0,
                    SchedulerClientJob_Id = jobConfig?.SchedulerClientJob_Id ?? 0,
                    EmailConfiguration = billBatchEmailConfiguration ?? string.Empty,
                    EmailSubject = billbatchEmailSubject ?? string.Empty,
                    EmailBody = billBatchFailEmailBody ?? string.Empty,
                };

                var schedulerEmailProviderSettings = new Dictionary<string, SchedulerClientJobEmailSettings>
                {
                    [nameof(Enum_SchedulerEmailOptions.EWS)] = new SchedulerClientJobEmailSettings
                    {
                        schedulerClientJobEWSSettings = new SchedulerClientJobEWSSettings
                        {
                            ServiceUrl = eWSEmailServiceURL ?? string.Empty,
                            Token = eWSEmailToken ?? string.Empty,
                            App_Id = eWSEmailAppId ?? string.Empty,
                            AppPassword = eWSEmailAppPassword ?? string.Empty,
                            EmailAddress = eWSEmailAddress ?? string.Empty,
                        }
                    }
                };
                var toEmail = new List<string> { queue.Email };
                var sendEmail = EmailUtility.SendSchedulerEmail(jobConfig.Client_Id, jobConfig.SchedulerClientJob_Id, jobConfig.JobName, billBatchEmailConfigurations, emailParameters, jobConfig.JobName, toEmail, null, schedulerEmailProviderSettings);
                if (!sendEmail.EmailSent)
                {
                    FileLogger.LogInformation($"Utils: BillBatchFailEmail Failed: ErrorMessage: {sendEmail.ErrorMessage} : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                }
            }
            catch (Exception exception)
            {
                FileLogger.LogError($"Utils: BillBatchFailEmail Exception: {exception} Message: {exception.Message}");
            }
        }
    }
}
