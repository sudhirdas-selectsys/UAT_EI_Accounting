using Accounting.DBUtilities;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transaction;
using Accounting.Entities.DB.Transactions;
using Accounting.Utilities.Internal.Interfaces;
using IMSOnline.DBUtilities.DB.Interfaces;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Pqc.Crypto.Hqc;
using System.Data;

namespace Accounting.Utilities.Internal
{
    public class ReportsUtility
    {
        private readonly ILU_SQLQueriesRepository _ILU_SQLQueriesRepository = new LU_SQLQueriesRepository();
        private readonly ILU_SQLInputParametersRepository _ILU_SQLInputParametersRepository = new LU_SQLInputParametersRepository();
        private readonly IReportOutputTablesRepository _IReportOutputTablesRepository = new ReportOutputTablesRepository();
        private readonly IReportOutputTableParametersRepository _IReportOutputTableParametersRepository = new ReportOutputTableParametersRepository();
        private readonly IDynamicFilterSQLExpressionsRepository _IDynamicFilterSQLExpressionsRepository = new DynamicFilterSQLExpressionsRepository();

        private readonly CommonUtility _commonUtility;
        private readonly IUserContextRepository _UserContextRepository;
        public ReportsUtility(IUserContextRepository userContextRepository)
        {
            _UserContextRepository = userContextRepository;
        }
        public List<ReportOutputTables> GetReportOutputTables(long Report_Id, List<LU_SQLInputParameters> FormSQLInputParameters, out int? TotalRecords, byte? FileType_Id = null)
        {
            TotalRecords = null;
            List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
            List<LU_SQLQueries> _SQLQueries = _ILU_SQLQueriesRepository.GetSQLQueriesByReport(Report_Id);
            if (_SQLQueries != null && _SQLQueries.Count > 0)
            {
                foreach (var _SQLQuery in _SQLQueries)
                {
                    var ds = ExecuteDB(_SQLQuery, FormSQLInputParameters, out TotalRecords, true);
                    var outputTables = _IReportOutputTablesRepository.GetReportOutputTables(_SQLQuery.SQLQuery_Id);

                    #region Prepare Output Table
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        for (int t = 0; t < ds.Tables.Count; t++)
                        {
                            var outputTable = outputTables?.Where(c => c.TableIndex == t).FirstOrDefault();
                            if (outputTable == null)
                            {
                                outputTable = new ReportOutputTables() { TableIndex = t, Name = "Result " + t, DataTable = ds.Tables[t] };
                                if (outputTables == null)
                                    outputTables = new List<ReportOutputTables>();
                                outputTables.Add(outputTable);
                            }
                            else
                            {
                                outputTable.DataTable = ds.Tables[t];
                            }
                            //Find matching output table
                        }

                    }
                    #endregion

                    #region Prepare Output Parameters
                    if (outputTables != null)
                    {
                        foreach (var outputTable in outputTables)
                        {
                            outputTable.OutputParameters = _IReportOutputTableParametersRepository.GetReportOutputTableParameters(outputTable.ReportOutputTable_Id);
                            if (outputTable.DataTable != null && outputTable.DataTable.Columns.Count > 0)
                            {
                                if (outputTable.OutputParameters == null)
                                    outputTable.OutputParameters = new List<ReportOutputTableParameters>();
                                for (int c = 0; c < outputTable.DataTable.Columns.Count; c++)
                                {
                                    var column = outputTable.DataTable.Columns[c];
                                    var matchingOP = outputTable.OutputParameters.Where(c => c.ParameterName.ToLower() == column.ColumnName.ToLower()).FirstOrDefault();
                                    if (matchingOP == null)
                                    {
                                        var reportOutputTableParameter = new ReportOutputTableParameters()
                                        {
                                            DisplaySequence = c,
                                            ParameterName = column.ColumnName,
                                        };
                                        outputTable.OutputParameters.Add(reportOutputTableParameter);
                                    }
                                    else
                                    {
                                        if (matchingOP.DisplaySequence == null)
                                            matchingOP.DisplaySequence = c;
                                        if (string.IsNullOrEmpty(matchingOP.DisplayName))
                                            matchingOP.DisplayName = column.ColumnName;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    outputTablesList.AddRange(outputTables);
                }
            }
            //Skip tables based on configuration status Hide
            outputTablesList = outputTablesList.Where(c => !c.Hide).ToList();
            //Customize the tables as per the output parameters configurations
            #region
            foreach (var outputTable in outputTablesList)
            {
                outputTable.OutputDataTable = BuildFinalOutPutTable(outputTable, FileType_Id);
            }
            #endregion
            return outputTablesList;
        }
        public List<ReportOutputTables> GetReportOutputTablesWithoutTotalrecords(long Report_Id, List<LU_SQLInputParameters> FormSQLInputParameters, byte? FileType_Id = null, long? client_Id = null)
        {
            List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
            List<LU_SQLQueries> _SQLQueries = _ILU_SQLQueriesRepository.GetSQLQueriesByReport(Report_Id);
            if (_SQLQueries != null && _SQLQueries.Count > 0)
            {
                foreach (var _SQLQuery in _SQLQueries)
                {
                    var ds = ExecuteDBWithoutTotalrecords(_SQLQuery, FormSQLInputParameters, client_Id);
                    var outputTables = _IReportOutputTablesRepository.GetReportOutputTables(_SQLQuery.SQLQuery_Id);

                    #region Prepare Output Table
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        for (int t = 0; t < ds.Tables.Count; t++)
                        {
                            var outputTable = outputTables?.Where(c => c.TableIndex == t).FirstOrDefault();
                            if (outputTable == null)
                            {
                                outputTable = new ReportOutputTables() { TableIndex = t, Name = "Result " + t, DataTable = ds.Tables[t] };
                                if (outputTables == null)
                                    outputTables = new List<ReportOutputTables>();
                                outputTables.Add(outputTable);
                            }
                            else
                            {
                                outputTable.DataTable = ds.Tables[t];
                            }
                            //Find matching output table
                        }

                    }
                    #endregion

                    #region Prepare Output Parameters
                    if (outputTables != null)
                    {
                        foreach (var outputTable in outputTables)
                        {
                            outputTable.OutputParameters = _IReportOutputTableParametersRepository.GetReportOutputTableParameters(outputTable.ReportOutputTable_Id);
                            if (outputTable.DataTable != null && outputTable.DataTable.Columns.Count > 0)
                            {
                                if (outputTable.OutputParameters == null)
                                    outputTable.OutputParameters = new List<ReportOutputTableParameters>();
                                for (int c = 0; c < outputTable.DataTable.Columns.Count; c++)
                                {
                                    var column = outputTable.DataTable.Columns[c];
                                    var matchingOP = outputTable.OutputParameters.Where(c => c.ParameterName.ToLower() == column.ColumnName.ToLower()).FirstOrDefault();
                                    if (matchingOP == null)
                                    {
                                        var reportOutputTableParameter = new ReportOutputTableParameters()
                                        {
                                            DisplaySequence = c,
                                            ParameterName = column.ColumnName,
                                        };
                                        outputTable.OutputParameters.Add(reportOutputTableParameter);
                                    }
                                    else
                                    {
                                        if (matchingOP.DisplaySequence == null)
                                            matchingOP.DisplaySequence = c;
                                        if (string.IsNullOrEmpty(matchingOP.DisplayName))
                                            matchingOP.DisplayName = column.ColumnName;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    outputTablesList.AddRange(outputTables);
                }
            }
            //Skip tables based on configuration status Hide
            outputTablesList = outputTablesList.Where(c => !c.Hide).ToList();
            //Customize the tables as per the output parameters configurations
            #region
            foreach (var outputTable in outputTablesList)
            {
                outputTable.OutputDataTable = BuildFinalOutPutTable(outputTable, FileType_Id);
            }
            #endregion
            return outputTablesList;
        }
        public DataSet ExecuteDBWithoutTotalrecords(LU_SQLQueries _SQLQuery, List<LU_SQLInputParameters> InputtedSQLInputParameters, long? client_Id = null)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            var sqlInputParameters = _ILU_SQLInputParametersRepository.GetSQLQueryInputParameters(_SQLQuery.SQLQuery_Id);
            if (sqlInputParameters != null && sqlInputParameters.Count > 0)
            {
                foreach (var inputParameter in sqlInputParameters)
                {
                    //Fill the predefined parameters data
                    if (inputParameter.PredefinedParameter_Id > 0)
                    {
                        inputParameter.InputValue = _UserContextRepository.GetPredefinedParameterValue(inputParameter.PredefinedParameter_Id);
                        if (string.IsNullOrEmpty(inputParameter?.InputValue))
                        {
                            inputParameter.InputValue = Convert.ToString(client_Id);
                        }
                        sqlParameters.Add(new SqlParameter($"@{inputParameter.ParameterName}", inputParameter.InputValue));
                        continue;
                    }
                    if (InputtedSQLInputParameters != null && InputtedSQLInputParameters.Count > 0)
                    {
                        var formInputParameter = InputtedSQLInputParameters.Where(c => c.SQLParameter_Id == inputParameter.SQLParameter_Id).FirstOrDefault();
                        if (formInputParameter != null)
                        {
                            if (inputParameter.SQLQuery_Id == _SQLQuery.SQLQuery_Id && !sqlParameters.Any(c => c.ParameterName == $"@{inputParameter.ParameterName}"))
                            {
                                //If there are more than one input with same parameter name take the value from the last child that has the value.
                                var sqlInputsListWithSameParameterName = sqlInputParameters.Where(c => c.SQLQuery_Id == inputParameter.SQLQuery_Id && c.ParameterName == inputParameter.ParameterName).ToList();
                                if (sqlInputsListWithSameParameterName.Count > 1)
                                {
                                    var matchingFormInputParameterList = InputtedSQLInputParameters.Where(c => sqlInputsListWithSameParameterName.Any(x => x.SQLParameter_Id == c.SQLParameter_Id)).ToList();
                                    if (matchingFormInputParameterList != null)
                                    {
                                        foreach (var matchingFormInput in matchingFormInputParameterList)
                                        {
                                            matchingFormInput.SequenceNo = sqlInputParameters.Where(c => c.SQLParameter_Id == matchingFormInput.SQLParameter_Id).Select(c => c.SequenceNo).FirstOrDefault();
                                        }
                                    }
                                    formInputParameter = matchingFormInputParameterList.Where(c => !string.IsNullOrEmpty(c.InputValue)).OrderByDescending(c => c.SequenceNo).FirstOrDefault();
                                }
                                if (formInputParameter != null)
                                    sqlParameters.Add(new SqlParameter($"@{inputParameter.ParameterName}", formInputParameter.InputValue));
                            }
                        }
                    }
                }

            }
            if (!_SQLQuery.Is_StoredProcedure && sqlInputParameters != null && sqlInputParameters.Any(s => !sqlParameters.Any(c => c.ParameterName == $"@{s.ParameterName}")))
            {
                var missedSqlInputParameters = sqlInputParameters.Where(s => !sqlParameters.Any(c => c.ParameterName == $"@{s.ParameterName}")).ToList();
                if (missedSqlInputParameters != null && missedSqlInputParameters.Count > 0)
                {
                    foreach (var missedInputParameter in missedSqlInputParameters)
                    {
                        if (missedInputParameter.SQLQuery_Id == _SQLQuery.SQLQuery_Id && !sqlParameters.Any(c => c.ParameterName == $"@{missedInputParameter.ParameterName}"))
                            sqlParameters.Add(new SqlParameter($"@{missedInputParameter.ParameterName}", missedInputParameter.DefaultValue));
                    }
                }

            }
            var DynamicFilters = _IDynamicFilterSQLExpressionsRepository.GetDynamicFilters(_SQLQuery.SQLQuery_Id);
            if (DynamicFilters != null && DynamicFilters.Count > 0)

            {
                var DynamicFilterSQLExpressions = _IDynamicFilterSQLExpressionsRepository.GetDynamicFilterSQLExpressions(_SQLQuery.SQLQuery_Id);
                if (DynamicFilters != null && DynamicFilters.Count > 0)
                {
                    foreach (var DF in DynamicFilters)
                    {
                        string FinalDE = "";
                        string DynamicExpression = "";
                        var DFExpsXByDynamicFilter_Id = DynamicFilterSQLExpressions.Where(c => c.DynamicFilter_Id == DF.DynamicFilter_Id).ToList();
                        if (DFExpsXByDynamicFilter_Id != null && DFExpsXByDynamicFilter_Id.Count > 0)
                        {
                            //List<LU_DynamicFilterSQLExpressions> _ParentGroupDFSExpressions = new List<LU_DynamicFilterSQLExpressions>();

                            foreach (var DFSE in DFExpsXByDynamicFilter_Id)
                            {
                                if (!string.IsNullOrEmpty(DFSE.Dependency_SQLParameters))
                                {
                                    bool IsValidParamsToAddExpression = true;
                                    var DependencyParameters = DFSE.Dependency_SQLParameters.Split(',');
                                    foreach (var DP in DependencyParameters)
                                    {
                                        if (DF.SQLQuery_Id == _SQLQuery.SQLQuery_Id && sqlParameters.Any(c => c.ParameterName.ToLower() == $"@{DP.ToLower()}"))
                                        {
                                            var IP = sqlParameters.Where(c => c.ParameterName.ToLower() == $"@{DP.ToLower()}").FirstOrDefault();
                                            if (IP.Value == null || string.IsNullOrEmpty(IP.Value.ToString()))
                                            {
                                                IsValidParamsToAddExpression = false;
                                            }
                                        }
                                        else
                                            IsValidParamsToAddExpression = false;
                                        if (!IsValidParamsToAddExpression)
                                            break;
                                    }
                                    if (IsValidParamsToAddExpression)
                                    {
                                        if ((!DF.Add_WhereClause || !DF.Add_HavingClause) && string.IsNullOrEmpty(DynamicExpression))
                                            DynamicExpression = (DFSE.UseORForConcatenate ? "or" : "and");
                                        DynamicExpression += " " + DFSE.SQLExpression + " " + (DFSE.UseORForConcatenate ? "or" : "and");
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(DynamicExpression))
                            {
                                if (DynamicExpression.EndsWith("or"))
                                    DynamicExpression = DynamicExpression.Substring(0, DynamicExpression.Length - 3);
                                else if (DynamicExpression.EndsWith("and"))
                                    DynamicExpression = DynamicExpression.Substring(0, DynamicExpression.Length - 4);
                                else if (DynamicExpression.EndsWith(" "))
                                    DynamicExpression = DynamicExpression.Substring(0, DynamicExpression.Length - 1);

                                FinalDE = (DF.Add_WhereClause ? " Where" : DF.Add_HavingClause ? " having" : "") + DynamicExpression;
                            }
                        }
                        _SQLQuery.SQLQuery = _SQLQuery.SQLQuery.Replace($"{DF.ExpressionIdentifier}", FinalDE);

                    }
                }

            }

            return SQLHelper.ExecuteDataset(_SQLQuery.Is_StoredProcedure ? CommandType.StoredProcedure : CommandType.Text, _SQLQuery.SQLQuery, sqlParameters.ToArray());
        }

        private DataTable BuildFinalOutPutTable(ReportOutputTables outputTable, byte? FileType_Id)
        {
            if (outputTable == null || outputTable.DataTable == null)
                return null;
            DataTable newTable = new DataTable();
            var OutputParamsList = outputTable.OutputParameters.Where(c => !c.Hide).ToList();
            var filteredROTParameters = OutputParamsList.Where(c => string.IsNullOrEmpty(c.GroupName)).ToList();
            foreach (var outputParam in OutputParamsList.Where(c => !string.IsNullOrEmpty(c.GroupName)))
            {
                if (!filteredROTParameters.Any(c => c.GroupName == outputParam.GroupName))
                {
                    filteredROTParameters.Add(outputParam);
                }
            }
            //Headers
            outputTable.OutputParameters = filteredROTParameters.OrderBy(c => c.DisplaySequence).ToList();
            outputTable.OutputParameters.RemoveAll(p => p.ParameterName == "TotalRecords");

            foreach (var RP in outputTable.OutputParameters)
            {
                newTable.Columns.Add(RP.ParameterName, typeof(string));
            }
            foreach (DataRow dr in outputTable.DataTable.Rows)
            {
                DataRow newDR = newTable.NewRow();
                foreach (var RP in outputTable.OutputParameters)
                {
                    var columnDataType = outputTable.DataTable.Columns[RP.ParameterName].DataType;
                    var columnValue = dr[RP.ParameterName];
                    if (!string.IsNullOrEmpty(RP.GroupName))
                    {
                        columnValue = null;
                        var groupingOTParametersList = OutputParamsList.Where(c => c.GroupName == RP.GroupName).OrderBy(c => c.DisplaySequence).ToList();
                        for (int O = 0; O < groupingOTParametersList.Count; O++)
                        {
                            var OP = groupingOTParametersList[O];
                            if (O == groupingOTParametersList.Count - 1)
                                columnValue += $"{dr[OP.ParameterName]}";
                            else
                                columnValue += $"{dr[OP.ParameterName]}{(!string.IsNullOrEmpty(OP.GroupConcatenate) ? OP.GroupConcatenate : (!string.IsNullOrEmpty(RP.GroupConcatenate) ? RP.GroupConcatenate : " "))}";
                        }
                    }
                    if (columnValue != null && columnValue != DBNull.Value && Convert.ToString(columnValue) != "")
                    {
                        columnValue = FormatValue(RP.FormatType_Id, RP.FormatCode, columnValue, columnDataType, FileType_Id);
                    }
                    newDR[RP.ParameterName] = columnValue;
                }
                newTable.Rows.Add(newDR);
            }
            if (outputTable.OutputParameters.Any(c => c.Enable_Total))
            {
                DataRow newDR = newTable.NewRow();
                foreach (var RP in outputTable.OutputParameters)
                {
                    var columnDataType = outputTable.DataTable.Columns[RP.ParameterName].DataType;
                    if (RP.Enable_Total)
                    {
                        double total = 0;
                        try
                        {
                            foreach (DataRow dr in outputTable.DataTable.Rows)
                            {
                                var value = dr[RP.ParameterName];
                                if (value != null && value != DBNull.Value && Convert.ToString(value) != "")
                                {
                                    total += Convert.ToDouble(Convert.ToString(value).Replace("$", "").Replace(",", ""));
                                }

                            }
                        }
                        catch (Exception ex) { }
                        var totalValue = Convert.ChangeType(total, columnDataType);
                        newDR[RP.ParameterName] = FormatValue(RP.FormatType_Id, RP.FormatCode, totalValue, columnDataType, FileType_Id);
                    }
                }
                newTable.Rows.Add(newDR);
            }
            return newTable;
        }

        private object? FormatValue(byte? FormatType_Id, string FormatCode, object columnValue, Type ColumnDataType, byte? FileType_Id)
        {
            if (!string.IsNullOrEmpty(FormatCode))
            {
                if (FormatType_Id == MasterLookup._FormatTypes.Date)
                {
                    columnValue = Convert.ToDateTime(columnValue).ToString(FormatCode);
                }
                else if (FileType_Id != MasterLookup._LUFileTypes.Excel && (FormatType_Id == MasterLookup._FormatTypes.Currency || FormatType_Id == MasterLookup._FormatTypes.CurrencyWithoutDecimalDigits))
                {
                    decimal value = 0;
                    if (ColumnDataType == typeof(string))
                        if (decimal.TryParse(Convert.ToString(columnValue), out value))
                            columnValue = value;
                    columnValue = string.Format(FormatCode, columnValue);
                }
                else if (FileType_Id != MasterLookup._LUFileTypes.Excel && FormatType_Id == MasterLookup._FormatTypes.Percent || FormatType_Id == MasterLookup._FormatTypes.ApendPercent)
                {
                    if (ColumnDataType != typeof(string))
                        columnValue = string.Format(FormatCode, Convert.ChangeType(columnValue, ColumnDataType));
                    else
                        columnValue = string.Format(FormatCode, Convert.ToDouble(columnValue));
                }
            }
            return columnValue;
        }
        public DataSet ExecuteDB(LU_SQLQueries _SQLQuery, List<LU_SQLInputParameters> InputtedSQLInputParameters, out int? TotalRecords,
            bool includeTotalRecords = false)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            var sqlInputParameters = _ILU_SQLInputParametersRepository.GetSQLQueryInputParameters(_SQLQuery.SQLQuery_Id);
            if (sqlInputParameters != null)
            {
                var hiddenParameters = sqlInputParameters
            .Where(x => x.SQLQuery_Id == _SQLQuery.SQLQuery_Id && x.ShowInputField == false)
            .ToList();
                if (hiddenParameters != null && hiddenParameters.Any())
                {
                    foreach (var parameter in hiddenParameters)
                    {
                        if (InputtedSQLInputParameters != null)
                        {
                            var existingParam = InputtedSQLInputParameters
                .FirstOrDefault(x => x.ParameterName == parameter.ParameterName);

                            if (existingParam != null)
                            {
                                existingParam.SQLParameter_Id = parameter.SQLParameter_Id;
                                existingParam.SQLQuery_Id = parameter.SQLQuery_Id;
                                existingParam.ShowInputField = parameter.ShowInputField;
                                existingParam.AllowFilterWith_Empty = parameter.AllowFilterWith_Empty;
                                existingParam.AllowFilterWith_Null = parameter.AllowFilterWith_Null;
                                existingParam.Client_Id = parameter.Client_Id;
                                existingParam.Code = parameter.Code;
                                existingParam.CreatedBy = parameter.CreatedBy;
                                existingParam.CreatedOn = parameter.CreatedOn;
                                existingParam.DataDictionaryList = parameter.DataDictionaryList;
                                existingParam.DataDictionaryType_Id = parameter.DataDictionaryType_Id;
                                existingParam.DataDictionayType = parameter.DataDictionayType;
                                existingParam.DataType_Id = parameter.DataType_Id;
                                existingParam.DefaultValue = parameter.DefaultValue;
                                existingParam.Dependency_SQLParameter_Id = parameter.Dependency_SQLParameter_Id;
                                existingParam.Dependency_SQLParameter_IdForRequired = parameter.Dependency_SQLParameter_IdForRequired;
                                existingParam.DisplayName = parameter.DisplayName;
                                existingParam.DisplayText = parameter.DisplayText;
                                existingParam.EffectiveDate = parameter.EffectiveDate;
                                existingParam.ExpirationDate = parameter.ExpirationDate;
                                existingParam.FormatCode = parameter.FormatCode;
                                existingParam.FormatType_Id = parameter.FormatType_Id;
                                existingParam.GroupName = parameter.GroupName;
                                existingParam.HTMLControlType_Id = parameter.HTMLControlType_Id;
                                existingParam.InputValue = existingParam.InputValue;
                                existingParam.IsRequired = parameter.IsRequired;
                                existingParam.Is_OutputParameter = parameter.Is_OutputParameter;
                                existingParam.MaxTextLength = parameter.MaxTextLength;
                                existingParam.MaxValue = parameter.MaxValue;
                                existingParam.MinTextLength = parameter.MinTextLength;
                                existingParam.MinValue = parameter.MinValue;
                                existingParam.MultiInputSeparator = parameter.MultiInputSeparator;
                                existingParam.Name = parameter.Name;
                                existingParam.NoOfDecimalDigits = parameter.NoOfDecimalDigits;
                                existingParam.PredefinedParameter_Id = parameter.PredefinedParameter_Id;
                                existingParam.RangeGroupName = parameter.RangeGroupName;
                                existingParam.SequenceNo = parameter.SequenceNo;
                                existingParam.Service_Id = parameter.Service_Id;
                                existingParam.Status = parameter.Status;
                                existingParam.Text = parameter.Text;
                                existingParam.Value = parameter.Value;
                            }
                        }
                    }
                }

                if (sqlInputParameters != null && sqlInputParameters.Count > 0)
                {
                    if (includeTotalRecords)
                    {
                        sqlParameters.Add(new SqlParameter
                        {
                            ParameterName = "@TotalRecords",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output
                        });
                    }
                    foreach (var inputParameter in sqlInputParameters)
                    {
                        //Fill the predefined parameters data
                        if (inputParameter.PredefinedParameter_Id > 0)
                        {
                            inputParameter.InputValue = _UserContextRepository.GetPredefinedParameterValue(inputParameter.PredefinedParameter_Id);
                            sqlParameters.Add(new SqlParameter($"@{inputParameter.ParameterName}", inputParameter.InputValue));
                            continue;
                        }
                        if (InputtedSQLInputParameters != null && InputtedSQLInputParameters.Count > 0)
                        {
                            var formInputParameter = InputtedSQLInputParameters.Where(c => c.SQLParameter_Id == inputParameter.SQLParameter_Id).FirstOrDefault();
                            if (formInputParameter != null)
                            {
                                if (inputParameter.SQLQuery_Id == _SQLQuery.SQLQuery_Id && !sqlParameters.Any(c => c.ParameterName == $"@{inputParameter.ParameterName}"))
                                {
                                    //If there are more than one input with same parameter name take the value from the last child that has the value.
                                    var sqlInputsListWithSameParameterName = sqlInputParameters.Where(c => c.SQLQuery_Id == inputParameter.SQLQuery_Id && c.ParameterName == inputParameter.ParameterName).ToList();
                                    if (sqlInputsListWithSameParameterName.Count > 1)
                                    {
                                        var matchingFormInputParameterList = InputtedSQLInputParameters.Where(c => sqlInputsListWithSameParameterName.Any(x => x.SQLParameter_Id == c.SQLParameter_Id)).ToList();
                                        if (matchingFormInputParameterList != null)
                                        {
                                            foreach (var matchingFormInput in matchingFormInputParameterList)
                                            {
                                                matchingFormInput.SequenceNo = sqlInputParameters.Where(c => c.SQLParameter_Id == matchingFormInput.SQLParameter_Id).Select(c => c.SequenceNo).FirstOrDefault();
                                            }
                                        }
                                        formInputParameter = matchingFormInputParameterList.Where(c => !string.IsNullOrEmpty(c.InputValue)).OrderByDescending(c => c.SequenceNo).FirstOrDefault();
                                    }
                                    if (formInputParameter != null)
                                        sqlParameters.Add(new SqlParameter($"@{inputParameter.ParameterName}", formInputParameter.InputValue));
                                }
                            }
                        }
                    }

                }
            }
            if (!_SQLQuery.Is_StoredProcedure && sqlInputParameters != null && sqlInputParameters.Any(s => !sqlParameters.Any(c => c.ParameterName == $"@{s.ParameterName}")))
            {
                var missedSqlInputParameters = sqlInputParameters.Where(s => !sqlParameters.Any(c => c.ParameterName == $"@{s.ParameterName}")).ToList();
                if (missedSqlInputParameters != null && missedSqlInputParameters.Count > 0)
                {
                    foreach (var missedInputParameter in missedSqlInputParameters)
                    {
                        if (missedInputParameter.SQLQuery_Id == _SQLQuery.SQLQuery_Id && !sqlParameters.Any(c => c.ParameterName == $"@{missedInputParameter.ParameterName}"))
                            sqlParameters.Add(new SqlParameter($"@{missedInputParameter.ParameterName}", missedInputParameter.DefaultValue));
                    }
                }

            }
            var DynamicFilters = _IDynamicFilterSQLExpressionsRepository.GetDynamicFilters(_SQLQuery.SQLQuery_Id);
            if (DynamicFilters != null && DynamicFilters.Count > 0)

            {
                var DynamicFilterSQLExpressions = _IDynamicFilterSQLExpressionsRepository.GetDynamicFilterSQLExpressions(_SQLQuery.SQLQuery_Id);
                if (DynamicFilters != null && DynamicFilters.Count > 0)
                {
                    foreach (var DF in DynamicFilters)
                    {
                        string FinalDE = "";
                        string DynamicExpression = "";
                        var DFExpsXByDynamicFilter_Id = DynamicFilterSQLExpressions.Where(c => c.DynamicFilter_Id == DF.DynamicFilter_Id).ToList();
                        if (DFExpsXByDynamicFilter_Id != null && DFExpsXByDynamicFilter_Id.Count > 0)
                        {
                            //List<LU_DynamicFilterSQLExpressions> _ParentGroupDFSExpressions = new List<LU_DynamicFilterSQLExpressions>();

                            foreach (var DFSE in DFExpsXByDynamicFilter_Id)
                            {
                                if (!string.IsNullOrEmpty(DFSE.Dependency_SQLParameters))
                                {
                                    bool IsValidParamsToAddExpression = true;
                                    var DependencyParameters = DFSE.Dependency_SQLParameters.Split(',');
                                    foreach (var DP in DependencyParameters)
                                    {
                                        if (DF.SQLQuery_Id == _SQLQuery.SQLQuery_Id && sqlParameters.Any(c => c.ParameterName.ToLower() == $"@{DP.ToLower()}"))
                                        {
                                            var IP = sqlParameters.Where(c => c.ParameterName.ToLower() == $"@{DP.ToLower()}").FirstOrDefault();
                                            if (IP.Value == null || string.IsNullOrEmpty(IP.Value.ToString()))
                                            {
                                                IsValidParamsToAddExpression = false;
                                            }
                                        }
                                        else
                                            IsValidParamsToAddExpression = false;
                                        if (!IsValidParamsToAddExpression)
                                            break;
                                    }
                                    if (IsValidParamsToAddExpression)
                                    {
                                        if ((!DF.Add_WhereClause || !DF.Add_HavingClause) && string.IsNullOrEmpty(DynamicExpression))
                                            DynamicExpression = (DFSE.UseORForConcatenate ? "or" : "and");
                                        DynamicExpression += " " + DFSE.SQLExpression + " " + (DFSE.UseORForConcatenate ? "or" : "and");
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(DynamicExpression))
                            {
                                if (DynamicExpression.EndsWith("or"))
                                    DynamicExpression = DynamicExpression.Substring(0, DynamicExpression.Length - 3);
                                else if (DynamicExpression.EndsWith("and"))
                                    DynamicExpression = DynamicExpression.Substring(0, DynamicExpression.Length - 4);
                                else if (DynamicExpression.EndsWith(" "))
                                    DynamicExpression = DynamicExpression.Substring(0, DynamicExpression.Length - 1);

                                FinalDE = (DF.Add_WhereClause ? " Where" : DF.Add_HavingClause ? " having" : "") + DynamicExpression;
                            }
                        }
                        _SQLQuery.SQLQuery = _SQLQuery.SQLQuery.Replace($"{DF.ExpressionIdentifier}", FinalDE);

                    }
                }

            }
            DataSet ds = new DataSet();
            try
            {
                ds = SQLHelper.ExecuteDataset(_SQLQuery.Is_StoredProcedure ? CommandType.StoredProcedure : CommandType.Text, _SQLQuery.SQLQuery, sqlParameters.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            var totalRecordsParam = sqlParameters.FirstOrDefault(p => p.ParameterName == "@TotalRecords");
            TotalRecords = totalRecordsParam != null && totalRecordsParam.Value != DBNull.Value
                ? Convert.ToInt32(totalRecordsParam.Value)
                : 0;
            return ds;
        }
        public List<LU_DataDictionary> GetDataDictionary(LU_DataDictionaryTypes DataDictionayType, List<LU_SQLInputParameters> inputtedParameters)
        {
            if (DataDictionayType != null)
            {
                int? TotalRecords;
                if (DataDictionayType.SQLQuery_Id > 0)
                {
                    LU_SQLQueries SQLQuery = _ILU_SQLQueriesRepository.GetSQLQuery(DataDictionayType.SQLQuery_Id);
                    var ds = ExecuteDB(SQLQuery, inputtedParameters, out TotalRecords, false);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        try
                        {
                            var DataDictionaryList = new List<LU_DataDictionary>();
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                var dataDictionary = new LU_DataDictionary()
                                {
                                    Name = Convert.ToString(dr[DataDictionayType.Display_ParameterName]),
                                    Value = Convert.ToString(dr[DataDictionayType.Value_ParameterName])
                                };
                                DataDictionaryList.Add(dataDictionary);
                            }
                            return DataDictionaryList;
                        }
                        catch (Exception ex) { }
                    }
                }
                else
                {
                    return LookupUtility.GetDataDictionaries((int)DataDictionayType.DataDictionaryType_Id);
                }
            }
            return null;
        }

        public static readonly HashSet<long> ValidReportWithoutTemplates = new HashSet<long>
        {
            MasterLookup._Reports.InvoiceProduction,
            MasterLookup._Reports.Payment,
            MasterLookup._Reports.Transaction,
            MasterLookup._Reports.AccountReceivable,
            MasterLookup._Reports.AccountPayable,
            MasterLookup._Reports.TaxFiling,
            MasterLookup._Reports.JournalEntry,
            MasterLookup._Reports.Report1099,
            MasterLookup._Reports.BDXReport,
            MasterLookup._Reports.EarnedUnearnedPremiumCommission,
            MasterLookup._Reports.PaidBDXReport
        };

        public static readonly HashSet<long> ValidReportWithTemplates = new HashSet<long>
        {
            MasterLookup._Reports.DirectBill,
            MasterLookup._Reports.AgencyBill,
            MasterLookup._Reports.InsuredBill,
            MasterLookup._Reports.AgencyDetailed,
            MasterLookup._Reports.Underwriter,
            MasterLookup._Reports.DirectInsuredBill
        };
    }
}
