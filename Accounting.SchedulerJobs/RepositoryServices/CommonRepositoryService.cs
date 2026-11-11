using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Accounting.SchedulerJobs.RepositoryServices
{
    public class CommonRepositoryService : ICommonRepositoryService
    {
        public async Task<List<T>> ExecuteDyanmicGetObjectListAsync<T>(SchedulerClientJobProcedure procedure, object sourceObject) where T : class, new()
        {
            try
            {
                FileLogger.LogInformation($"CommonRepositoryService: ExecuteDyanmicGetObjectListAsync Started");
                List<SqlParameter> sqlParameters = new();
                foreach (var parameter in procedure.Parameters)
                {
                    SqlParameter sqlParameter;
                    if (parameter.IsOutputParameter)
                    {
                        sqlParameter = new SqlParameter(parameter.ParameterName, DBNull.Value)
                        {
                            Direction = ParameterDirection.Output,
                        };

                        if (!string.IsNullOrEmpty(parameter.ParameterDataType))
                        {
                            sqlParameter.SqlDbType = System.Enum.Parse<SqlDbType>(parameter.ParameterDataType, true);
                        }
                    }
                    else
                    {
                        object value = GetParameterValue(parameter, sourceObject);
                        sqlParameter = new SqlParameter(parameter.ParameterName, value ?? DBNull.Value);
                    }

                    sqlParameters.Add(sqlParameter);
                }

                // var temp = BuildSqlCommandText(procedure.CommandText, sqlParameters.ToArray());
                // var excuted = await SQLHelper.GetObjectList(new T(), procedure.CommandText, sqlParameters.ToArray());
                return await SQLHelper.GetObjectList(new T(), procedure.CommandText, sqlParameters.ToArray());
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonRepositoryService: ExecuteDyanmicGetObjectListAsync Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<T> GetObjectAsync<T>(SchedulerClientJobProcedure procedure, object sourceObject) where T : class, new()
        {
            try
            {
                FileLogger.LogInformation($"CommonRepositoryService: GetObjectAsync Started");
                List<SqlParameter> sqlParameters = new();
                foreach (var parameter in procedure.Parameters)
                {
                    SqlParameter sqlParameter;
                    if (parameter.IsOutputParameter)
                    {
                        sqlParameter = new SqlParameter(parameter.ParameterName, DBNull.Value)
                        {
                            Direction = ParameterDirection.Output,
                        };

                        if (!string.IsNullOrEmpty(parameter.ParameterDataType))
                        {
                            sqlParameter.SqlDbType = System.Enum.Parse<SqlDbType>(parameter.ParameterDataType, true);
                        }
                    }
                    else
                    {
                        object value = GetParameterValue(parameter, sourceObject);
                        sqlParameter = new SqlParameter(parameter.ParameterName, value ?? DBNull.Value);
                    }

                    sqlParameters.Add(sqlParameter);
                }

                // var temp = BuildSqlCommandText(procedure.CommandText, sqlParameters.ToArray());
                // var excuted = await SQLHelper.GetObject(new T(), procedure.CommandText, sqlParameters.ToArray());
                return await SQLHelper.GetObject(new T(), procedure.CommandText, sqlParameters.ToArray());
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonRepositoryService: GetObjectAsync Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<DataSet> ExecuteDyanmicGetDataSetAsync(SchedulerClientJobProcedure procedure, object sourceObject)
        {
            try
            {
                FileLogger.LogInformation($"CommonRepositoryService: ExecuteDyanmicGetDataSetAsync Started");
                List<SqlParameter> sqlParameters = new();
                foreach (var parameter in procedure.Parameters)
                {
                    SqlParameter sqlParameter;
                    if (parameter.IsOutputParameter)
                    {
                        sqlParameter = new SqlParameter(parameter.ParameterName, DBNull.Value)
                        {
                            Direction = ParameterDirection.Output,
                        };

                        if (!string.IsNullOrEmpty(parameter.ParameterDataType))
                        {
                            sqlParameter.SqlDbType = System.Enum.Parse<SqlDbType>(parameter.ParameterDataType, true);
                        }
                    }
                    else
                    {
                        object value = GetParameterValue(parameter, sourceObject);
                        sqlParameter = new SqlParameter(parameter.ParameterName, value ?? DBNull.Value);
                    }

                    sqlParameters.Add(sqlParameter);
                }

                // var temp = BuildSqlCommandText(procedure.CommandText, sqlParameters.ToArray());
                return await SQLHelper.ExecuteDataset(procedure.CommandText, sqlParameters.ToArray());
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonRepositoryService: ExecuteDyanmicGetDataSetAsync Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> GetSchedulerClientSettings(long client_Id, string settingKey)
        {
            string result = string.Empty;

            try
            {
                FileLogger.LogInformation($"CommonRepositoryService: GetSchedulerClientSettings Started");
                string query = "USP_GetSchedulerClientSettings";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", client_Id),
                    new SqlParameter("@SettingKey", settingKey),
                };

                var dataResult = await SQLHelper.ExecuteScalar(CommandType.StoredProcedure, query, parameters);
                if (dataResult != null)
                {
                    result = Convert.ToString(dataResult) ?? string.Empty;
                }

                return result;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonRepositoryService: GetSchedulerClientSettings Exception: {ex} Message: {ex.Message} Client_Id: {client_Id} SettingKey: {settingKey}");
                throw;
            }
        }

        public static async Task<string> GetSchedulerApplicationSettings(string settingKey)
        {
            string result = string.Empty;

            try
            {
                FileLogger.LogInformation($"CommonRepositoryService: GetSchedulerApplicationSettings Started");
                string query = "USP_GetSchedulerApplicationSettings";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SettingKey", settingKey),
                };

                var dataResult = await SQLHelper.ExecuteScalar(CommandType.StoredProcedure, query, parameters);
                if (dataResult != null)
                {
                    result = Convert.ToString(dataResult) ?? string.Empty;
                }

                return result;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonRepositoryService: GetSchedulerApplicationSettings Exception: {ex} Message: {ex.Message} SettingKey: {settingKey}");
                throw;
            }
        }

        private object GetParameterValue(SchedulerClientJobProcedureParameter parameter, object sourceObject)
        {
            try
            {
                object value = null;
                if (sourceObject is IDictionary<string, object> dictionary)
                {
                    dictionary.TryGetValue(parameter.DynamicSource, out value);
                }
                else
                {
                    value = sourceObject.GetType().GetProperty(parameter.DynamicSource)?.GetValue(sourceObject);
                }

                if (value == null && !string.IsNullOrEmpty(parameter.ParameterValue) && parameter.IsDefaultValue == true)
                {
                    value = parameter.ParameterValue;
                }

                return value ?? (parameter.IsDefaultValue ? parameter.ParameterValue : DBNull.Value);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"CommonRepositoryService: GetParameterValue Exception: {ex} Message: {ex.Message}");
                return DBNull.Value;
            }
        }

        private static string BuildSqlCommandText(string procedureName, SqlParameter[] parameters)
        {
            var sql = new StringBuilder();
            try
            {
                sql.Append($"EXEC {procedureName} ");

                var parameterTexts = new List<string>();

                foreach (var param in parameters)
                {
                    if (param.Direction == ParameterDirection.Output)
                    {
                        parameterTexts.Add($"{param.ParameterName} = {param.ParameterName} OUTPUT");
                    }
                    else
                    {
                        string value = param.Value switch
                        {
                            null or DBNull => "NULL",
                            string s => $"'{s.Replace("'", "''")}'",
                            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                            bool b => b ? "1" : "0",
                            _ => param.Value.ToString(),
                        };

                        parameterTexts.Add($"{param.ParameterName} = {value}");
                    }
                }

                sql.Append(string.Join(", ", parameterTexts));

                return sql.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}