using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Enum;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Accounting.SchedulerJobs.RepositoryServices
{
    public class SchedulerRepositoryService : ISchedulerRepositoryService
    {
        public async Task<List<SchedulerClientJobs>> GetSchedulerClientJobs()
        {
            try
            {
                FileLogger.LogInformation($"SchedulerRepositoryService: GetSchedulerClientJobs Started");
                return await SQLHelper.GetObjectList(new SchedulerClientJobs(), "USP_GetSchedulerClientJobs", null);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerRepositoryService: GetSchedulerClientJobs Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task InsertExecutionLog(SchedulerJobExecutionLog schedulerJobExecutionLog)
        {
            try
            {
                FileLogger.LogInformation($"SchedulerRepositoryService: InsertExecutionLog Started");
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CreatedOrUpdatedBy", schedulerJobExecutionLog.CreatedOrUpdatedBy),
                    new SqlParameter("@SchedulerClientJob_Id", schedulerJobExecutionLog.SchedulerClientJob_Id),
                    new SqlParameter("@Job_Id", schedulerJobExecutionLog.Job_Id),
                    new SqlParameter("@Client_Id", schedulerJobExecutionLog.Client_Id),
                    new SqlParameter("@JobName", schedulerJobExecutionLog.JobName),
                    new SqlParameter("@ExecutionStartTime", schedulerJobExecutionLog.ExecutionStartTime),
                    new SqlParameter("@ExecutionEndTime", schedulerJobExecutionLog.ExecutionEndTime),
                    new SqlParameter("@ExecutionDurationInSeconds", schedulerJobExecutionLog.ExecutionDurationInSeconds),
                    new SqlParameter("@ExecutionStatus", schedulerJobExecutionLog.ExecutionStatus),
                    new SqlParameter("@ErrorMessage", (object?)schedulerJobExecutionLog.ErrorMessage ?? DBNull.Value)
                };
                string query = "USP_InsertSchedulerJobExecutionLog";
                await SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, query, parameters);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerRepositoryService: InsertExecutionLog Exception: {ex.ToString()}  Message: {ex.Message} SchedulerJobExecutionLog: {JsonConvert.SerializeObject(schedulerJobExecutionLog)}");
                throw;
            }
        }

        public async Task<SchedulerClientJobProcedure> GetClientSchedulerJobProcedures(long SchedulerClientJob_Id)
        {
            var clientSchedulerJobProcedures = new SchedulerClientJobProcedure();

            try
            {
                FileLogger.LogInformation($"SchedulerRepositoryService: GetClientSchedulerJobProcedures Started");
                string query = "USP_GetClientSchedulerJobProcedures";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SchedulerClientJob_Id", SchedulerClientJob_Id),
                };

                using (SqlDataReader reader = await SQLHelper.ExecuteReader(CommandType.StoredProcedure, query, parameters))
                {
                    // First Result Set  Procedures
                    if (reader.Read())
                    {
                        clientSchedulerJobProcedures = new SchedulerClientJobProcedure
                        {
                            SchedulerClientJobProcedure_Id = Convert.ToInt64(reader["SchedulerClientJobProcedure_Id"]),
                            SchedulerClientJob_Id = Convert.ToInt64(reader["SchedulerClientJob_Id"]),
                            CommandText = Convert.ToString(reader["CommandText"]) ?? string.Empty,
                            Is_Procedure = Convert.ToBoolean(reader["Is_Procedure"]),
                            ExecutionOrder = Convert.ToInt32(reader["ExecutionOrder"]),
                            Parameters = new List<SchedulerClientJobProcedureParameter>()
                        };
                    }

                    // Second Result Set Parameters
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            long SchedulerClientJobProcedure_Id = Convert.ToInt64(reader["SchedulerClientJobProcedure_Id"]);
                            if (clientSchedulerJobProcedures != null)
                            {
                                clientSchedulerJobProcedures.Parameters.Add(new SchedulerClientJobProcedureParameter
                                {
                                    ClientSchedulerJobProcedureParameter_Id = Convert.ToInt64(reader["ClientSchedulerJobProcedureParameter_Id"]),
                                    SchedulerClientJobProcedure_Id = SchedulerClientJobProcedure_Id,
                                    ParameterDataType = Convert.ToString(reader["ParameterDataType"]) ?? string.Empty,
									ParameterName = Convert.ToString(reader["ParameterName"]) ?? string.Empty,
                                    ParameterValue = Convert.ToString(reader["ParameterValue"]) ?? string.Empty,
                                    IsOutputParameter = Convert.ToBoolean(reader["IsOutputParameter"]),
                                    IsDynamic = Convert.ToBoolean(reader["IsDynamic"]),
									IsDefaultValue = Convert.ToBoolean(reader["IsDefaultValue"]),
									DynamicSource = Convert.ToString(reader["DynamicSource"]) ?? string.Empty
                                });
                            }
                        }
                    }
                }

                return clientSchedulerJobProcedures;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerRepositoryService: GetClientSchedulerJobProcedures Exception: {ex} Message: {ex.Message} SchedulerClientJob_Id: {SchedulerClientJob_Id}");
                throw;
            }
        }

        public async Task<SchedulerClientEmailConfigurations> GetSchedulerClientEmailConfigurations(long SchedulerClientJob_Id)
        {
            var schedulerClientEmailConfigurations = new SchedulerClientEmailConfigurations();

            try
            {
                FileLogger.LogInformation($"SchedulerRepositoryService: GetSchedulerClientEmailConfigurations Started");
                string query = "USP_GetSchedulerClientEmailConfigurations";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SchedulerClientJob_Id", SchedulerClientJob_Id),
                };

                using (SqlDataReader reader = await SQLHelper.ExecuteReader(CommandType.StoredProcedure, query, parameters))
                {
                    // First Result Set  Procedures
                    if (reader.Read())
                    {
                        schedulerClientEmailConfigurations = new SchedulerClientEmailConfigurations
                        {
                            Job_Id = Convert.ToInt64(reader["Job_Id"]),
                            SchedulerClientJob_Id = Convert.ToInt64(reader["SchedulerClientJob_Id"]),
                            ClientSchedulerJobEmailTemplateId = Convert.ToInt64(reader["ClientSchedulerJobEmailTemplate_Id"]),
                            TemplateName = Convert.ToString(reader["TemplateName"]) ?? string.Empty,
                            EmailSubject = Convert.ToString(reader["EmailSubject"]) ?? string.Empty,
                            EmailBody = Convert.ToString(reader["EmailBody"]) ?? string.Empty,
                            EmailSubjectParameters = Convert.ToString(reader["EmailSubjectParameters"]) ?? string.Empty,
                            EmailBodyParameters = Convert.ToString(reader["EmailBodyParameters"]) ?? string.Empty,
                            EmailConfiguration = Convert.ToString(reader["EmailConfiguration"]) ?? string.Empty,
                            AttachmentTypes = Convert.ToString(reader["AttachmentTypes"]) ?? string.Empty,
                            SchedulerJobEmailAttachments = new List<SchedulerJobEmailAttachments>()
                        };
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            if (schedulerClientEmailConfigurations != null)
                            {
                                schedulerClientEmailConfigurations.SchedulerJobEmailAttachments.Add(new SchedulerJobEmailAttachments
                                {
                                    AttachmentType = Convert.ToString(reader["AttachmentType"]) ?? string.Empty,
                                    AttachmentName = Convert.ToString(reader["AttachmentName"]) ?? string.Empty,
                                    AttachmentBody = Convert.ToString(reader["AttachmentBody"]) ?? string.Empty,
                                    AttachmentBodyParameters = Convert.ToString(reader["AttachmentBodyParameters"]) ?? string.Empty
                                });
                            }
                        }
                    }
                }

                return schedulerClientEmailConfigurations;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerRepositoryService: GetSchedulerClientEmailConfigurations Exception: {ex} Message: {ex.Message} SchedulerClientJob_Id: {SchedulerClientJob_Id}");
                throw;
            }
        }

        public async Task InsertSchedulerJobQueue(SchedulerClientJobQueue schedulerClientJobQueue)
        {
            try
            {
                FileLogger.LogInformation($"SchedulerRepositoryService: InsertSchedulerJobQueue Started");
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CreatedOrUpdatedBy", schedulerClientJobQueue.CreatedOrUpdatedBy),
                    new SqlParameter("@Job_Id", schedulerClientJobQueue.Job_Id),
                    new SqlParameter("@SchedulerClientJob_Id", schedulerClientJobQueue.SchedulerClientJob_Id),
                    new SqlParameter("@Client_Id", schedulerClientJobQueue.Client_Id),
                    new SqlParameter("@QueueReferenceType", schedulerClientJobQueue.QueueReferenceType),
                    new SqlParameter("@QueueReferenceValue", schedulerClientJobQueue.QueueReferenceValue),
                    new SqlParameter("@QueueStatus_Id", schedulerClientJobQueue.QueueStatus_Id),
                    new SqlParameter("@ExecutionStartTime", schedulerClientJobQueue.ExecutionStartTime),
                    new SqlParameter("@ExecutionEndTime", schedulerClientJobQueue.ExecutionEndTime),
                    new SqlParameter("@ErrorMessage", (object?)schedulerClientJobQueue.ErrorMessage ?? DBNull.Value)
                };
                string query = "USP_InsertSchedulerJobQueue";
                await SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, query, parameters);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerRepositoryService: InsertSchedulerJobQueue Exception: {ex.ToString()}  Message: {ex.Message} InsertSchedulerJobQueue: {JsonConvert.SerializeObject(schedulerClientJobQueue)}");
                throw;
            }
        }

        public async Task<Dictionary<string, SchedulerClientJobEmailSettings>> GetAccountingSchedulerEmailProviderSettings(long client_Id, long schedulerClientJob_Id)
        {
            try
            {
                FileLogger.LogInformation("SchedulerRepositoryService: GetAccountingSchedulerEmailProviderSettings Started");
                var result = new Dictionary<string, SchedulerClientJobEmailSettings>();
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", client_Id),
                    new SqlParameter("@SchedulerClientJob_Id", schedulerClientJob_Id)
                 };

                string query = "USP_GetAccountingSchedulerEmailProviderSettings";
                using (SqlDataReader reader = await SQLHelper.ExecuteReader(CommandType.StoredProcedure, query, parameters))
                {
                    if (!reader.Read())
                    {
                        return result;
                    }

                    var provider = (Enum_SchedulerEmailOptions)Convert.ToInt32(reader["EmailProvider_Id"]);
                    var settings = new SchedulerClientJobEmailSettings();
                    if (!string.IsNullOrEmpty(Convert.ToString(provider)))
                    {
                        if (reader.NextResult() && reader.Read())
                        {
                            switch (provider)
                            {
                                case Enum_SchedulerEmailOptions.SMTP:
                                    settings.schedulerClientJobSMTPSettings = new SchedulerClientJobSMTPSettings
                                    {
                                        SMTPHost = Convert.ToString(reader["SMTPHost"]),
                                        EmailAddress = Convert.ToString(reader["EmailAddress"]),
                                        SMTPPort = Convert.ToInt32(reader["SMTPPort"]),
                                        EnableSSL = Convert.ToBoolean(reader["EnableSSL"]),
                                        UserName = Convert.ToString(reader["UserName"]),
                                        Password = Convert.ToString(reader["Password"]),
                                        DisplayName = Convert.ToString(reader["DisplayName"]),
                                    };

                                    result.Add(nameof(Enum_SchedulerEmailOptions.SMTP), settings);
                                    break;

                                case Enum_SchedulerEmailOptions.EWS:

                                    settings.schedulerClientJobEWSSettings = new SchedulerClientJobEWSSettings
                                    {
                                        ServiceUrl = Convert.ToString(reader["ServiceUrl"]),
                                        Token = Convert.ToString(reader["Token"]),
                                        App_Id = Convert.ToString(reader["App_Id"]),
                                        AppPassword = Convert.ToString(reader["AppPassword"]),
                                        EmailAddress = Convert.ToString(reader["EmailAddress"])
                                    };

                                    result.Add(nameof(Enum_SchedulerEmailOptions.EWS), settings);
                                    break;
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerRepositoryService: GetAccountingSchedulerEmailProviderSettings Exception: {ex} Message: {ex.Message} ClientId: {client_Id} SchedulerClientJob_Id: {schedulerClientJob_Id}");
                throw;
            }
        }
    }
}
