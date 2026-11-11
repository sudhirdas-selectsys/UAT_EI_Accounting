using Accounting.Entities.DB.Transactions;
using Accounting.Utilities.Internal.Interfaces;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using System.Data;

namespace Accounting.Utilities.Internal.Implementations
{
    public class ClosedXMLUtility : IMSOfficeFileOptions
    {
        /* public byte[]? DownloadExcel(DataSet dataSet, List<ReportOutputTables> reportOutputTables)
         {
             byte[]? bytes = null;
             string sheetName = string.Empty;
             try
             {
                 if (dataSet?.Tables?.Count > 0)
                 {
                     using var workbook = new XLWorkbook();
                     foreach (DataTable table in dataSet.Tables)
                     {
                         sheetName = table.TableName;
                         var OutputParameters = reportOutputTables.Where(c => c.OutputDataTable.TableName == sheetName).FirstOrDefault().OutputParameters;
                         sheetName = sheetName.Replace("/", "");
                         var worksheet = workbook.Worksheets.Add(sheetName);
                         var currentRow = 1;
                         foreach (DataColumn column in table.Columns)
                         {
                             int rowIndex = currentRow;
                             int cell = column.Ordinal + 1;
                             worksheet.Cell(rowIndex, cell).Value = column.ColumnName;
                             worksheet.Cell(rowIndex, cell).Style.Fill.BackgroundColor = XLColor.SkyBlue;
                             worksheet.Cell(rowIndex, cell).Style.Font.Bold = true;
                             worksheet.Cell(rowIndex, cell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                             worksheet.Cell(rowIndex, cell).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                             worksheet.Cell(rowIndex, cell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                             worksheet.Cell(rowIndex, cell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                             worksheet.Cell(rowIndex, cell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                         }
                         currentRow++;
                         foreach (DataRow row in table.Rows)
                         {
                             int rowIndex = currentRow;
                             foreach (DataColumn column in table.Columns)
                             {
                                 string OutPutValue = row[column.ColumnName].ToString();
                                 int cell = column.Ordinal + 1;
                                 var OP = OutputParameters.Where(c => c.ParameterName.ToLower() == column.ColumnName.ToLower()).FirstOrDefault();
                                 if (OP != null && !string.IsNullOrWhiteSpace(OP.Excel_FormatCode))
                                 {
                                     decimal value = 0;
                                     DateTime DateTimeValue;
                                     if (decimal.TryParse(OutPutValue, out value))
                                     {
                                         worksheet.Cell(currentRow, cell).SetValue(Convert.ToDecimal(value));
                                     }
                                     // Not skipping format for DateTime in ReportUtility FormatValue method.
                                     //else if (DateTime.TryParse(OutPutValue, out DateTimeValue))
                                     //{
                                     //    worksheet.Cell(currentRow, cell).SetValue(DateTimeValue);
                                     //}
                                     else
                                     {
                                         worksheet.Cell(currentRow, cell).SetValue(OutPutValue);
                                     }
                                     worksheet.Cell(currentRow, cell).Style.NumberFormat.Format = OP.Excel_FormatCode;
                                 }
                                 else
                                 {
                                     worksheet.Cell(currentRow, cell).SetValue(OutPutValue);
                                 }
                                 //worksheet.Cell(currentRow, cell).SetValue(row[column.ColumnName].ToString());
                                 worksheet.Cell(currentRow, cell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                 worksheet.Cell(currentRow, cell).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                 worksheet.Cell(currentRow, cell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                 worksheet.Cell(currentRow, cell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                 worksheet.Cell(currentRow, cell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                             }
                             currentRow++;
                         }
                     }
                     using var stream = new MemoryStream();
                     workbook.SaveAs(stream);
                     bytes = stream.ToArray();
                 }
             }
             catch (Exception ex)
             {
                 throw;
             }
             return bytes;
         }*/
        public byte[]? DownloadExcel(DataSet dataSet, List<ReportOutputTables> reportOutputTables)
        {
            byte[]? bytes = null;

            try
            {
                if (dataSet?.Tables?.Count > 0)
                {
                    using var workbook = new XLWorkbook();

                    foreach (DataTable table in dataSet.Tables)
                    {
                        string sheetName = table.TableName?.Replace("/", "") ?? "Sheet1";

                        var outputParameters = reportOutputTables
                            .FirstOrDefault(c => c.OutputDataTable.TableName == table.TableName)
                            ?.OutputParameters;

                        var worksheet = workbook.Worksheets.Add(sheetName);

                        int currentRow = 1;

                        // ---------------- HEADER ----------------
                        foreach (DataColumn column in table.Columns)
                        {
                            int cell = column.Ordinal + 1;

                            worksheet.Cell(currentRow, cell).Value = column.ColumnName;
                            worksheet.Cell(currentRow, cell).Style.Fill.BackgroundColor = XLColor.SkyBlue;
                            worksheet.Cell(currentRow, cell).Style.Font.Bold = true;

                            worksheet.Cell(currentRow, cell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        // ---------------- DATA ----------------
                        currentRow++;

                        foreach (DataRow row in table.Rows)
                        {
                            foreach (DataColumn column in table.Columns)
                            {
                                int cell = column.Ordinal + 1;
                                var cellObj = worksheet.Cell(currentRow, cell);

                                object value = row[column];

                                var op = outputParameters?
                                    .FirstOrDefault(c =>
                                        c.ParameterName.Equals(column.ColumnName,
                                            StringComparison.OrdinalIgnoreCase));

                                // ---------------- TYPE HANDLING ----------------
                                if (value == DBNull.Value || value == null)
                                {
                                    cellObj.SetValue("");
                                }
                                else if (value is int || value is long || value is float || value is double)
                                {
                                    cellObj.SetValue(Convert.ToDouble(value));
                                }
                                else if (value is decimal dec)
                                {
                                    cellObj.SetValue(Convert.ToDouble(dec));
                                }
                                else if (value is DateTime dt)
                                {
                                    cellObj.SetValue(dt);
                                }
                                else
                                {
                                    string str = value.ToString();

                                    //  convert numeric strings 
                                    if (decimal.TryParse(str, out decimal parsedDecimal))
                                    {
                                        cellObj.SetValue(Convert.ToDouble(parsedDecimal));
                                    }
                                    else
                                    {
                                        cellObj.SetValue(str);
                                    }
                                }

                                // ---------------- FORMAT ----------------
                                if (op != null && !string.IsNullOrWhiteSpace(op.Excel_FormatCode))
                                {
                                    cellObj.Style.NumberFormat.Format = op.Excel_FormatCode;
                                }

                                // ---------------- BORDER ----------------
                                cellObj.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            currentRow++;
                        }
                    }

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    bytes = stream.ToArray();
                }
            }
            catch
            {
                throw;
            }

            return bytes;
        }
        public Dictionary<string, object> ReadExcelByCelssReference(Stream stream, List<string> MappedFieldsList)
        {
            if (MappedFieldsList == null || MappedFieldsList.Count == 0)
                return null;
            Dictionary<string, object> CellkeyValueList = new Dictionary<string, object>();
            using (var workbook = new XLWorkbook(stream))
            {
                foreach (var cellItem in MappedFieldsList)
                {
                    string cellReference = cellItem;
                    IXLWorksheet worksheet = null;
                    string sheetName = ""; object cellValue = null;
                    if (string.IsNullOrEmpty(cellItem))
                        continue;
                    if (cellItem.Contains("!"))
                    {
                        sheetName = cellItem.Substring(0, cellItem.IndexOf("!"));
                        cellReference = cellItem.Substring(cellItem.IndexOf("!") + 1);
                    }
                    if (!string.IsNullOrEmpty(sheetName))
                        worksheet = workbook.Worksheet(sheetName);
                    else
                        worksheet = workbook.Worksheet(1);
                    if (worksheet != null)
                    {
                        var xlcellItem = worksheet.Cell(cellReference);
                        if (xlcellItem != null)
                        {
                            if (xlcellItem.HasFormula)
                                cellValue = xlcellItem.CachedValue.ToString();
                            else
                                cellValue = xlcellItem.GetString();
                            if (!CellkeyValueList.ContainsKey(cellItem))
                            {
                                CellkeyValueList.Add(cellItem, cellValue);
                            }
                        }
                    }
                }
            }
            return CellkeyValueList;
        }
        public byte[] ExportToExcel(DataSet ds)
        {
            byte[] bytes = null;
            string sheetName = string.Empty;
            try
            {
                if (ds?.Tables?.Count > 0)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        foreach (DataTable table in ds.Tables)
                        {
                            sheetName = table.TableName;
                            var worksheet = workbook.Worksheets.Add(table, sheetName);
                            worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        }
                        var stream = new MemoryStream();
                        workbook.SaveAs(stream);
                        bytes = stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bytes;
        }


        public DataTable BalanceDueInAgencyBill(bool IsOutstandingInvoices, DataTable table4Final)
        {
            if (!IsOutstandingInvoices)
                return table4Final;

            DataTable filteredTable = table4Final.Clone();

            foreach (DataRow row in table4Final.Rows)
            {
                double balanceDue =
                    (row["NetAmount"] == DBNull.Value ? 0 : Convert.ToDouble(row["NetAmount"]))
                  - (row["AmountFinanced"] == DBNull.Value ? 0 : Convert.ToDouble(row["AmountFinanced"]))
                  - (row["AmountPaid"] == DBNull.Value ? 0 : Convert.ToDouble(row["AmountPaid"]));
                balanceDue = Math.Round(balanceDue, 2);
                if (balanceDue == 0)
                {
                    continue;
                }
                else if (balanceDue != 0)
                {
                    filteredTable.ImportRow(row);
                }
            }

            return filteredTable;
        }


        public byte[] DownloadDirectBillExcel(DataSet ds, DateTime? AsOfDate = null)
        {
            if (ds?.Tables?.Count > 4 && ds.Tables?[4].Rows.Count > 0)
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Commission Statement");

                DateTime date = AsOfDate ?? DateTime.Now;

                string formattedDate = date.ToString("MMMM d, yyyy");

                var table = ds.Tables[3]; // Main data table

                int row = 1;

                // Title
                ws.Cell(row, 4).Value = $"Commission Statement As Of {formattedDate}";
                ws.Range(row, 4, row, 8).Merge().Style
                    .Font.SetBold().Font.SetFontSize(14)
                    .Font.SetFontColor(XLColor.FromHtml("#289FD7"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                row += 2;

                // Headers
                var headers = new[]
                {
        "Agency Name", "Agency Address", "Insured's Name", "Policy Number", "Carrier", "LOB", "Transaction Type",
        "Invoice Number", "Effective Date", "Comm. Premium", "Premium Collected", "Comm. %",
        "Comm.", "Comm. Paid to Date", "Comm. Due"
    };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(row, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold();
                    cell.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                row++;

                int startRow = row;

                // Iterate through all rows (no grouping)
                foreach (DataRow dr in table.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid"))
                {
                    ws.Cell(row, 1).Value = Convert.ToString(dr["Agency Name"]);
                    ws.Cell(row, 2).Value = Convert.ToString(dr["Agency Address"]);
                    ws.Cell(row, 3).Value = Convert.ToString(dr["Insured Name"]);
                    ws.Cell(row, 4).Value = Convert.ToString(dr["Policy Number"]);
                    ws.Cell(row, 5).Value = Convert.ToString(dr["Carrier"]);
                    ws.Cell(row, 6).Value = Convert.ToString(dr["LOB"]);
                    ws.Cell(row, 7).Value = Convert.ToString(dr["Transaction Type"]);
                    ws.Cell(row, 8).Value = Convert.ToString(dr["IdNo"]);
                    ws.Cell(row, 9).Value = Convert.ToDateTime(dr["EffectiveDate"]).ToString("M/d/yyyy");
                    ws.Cell(row, 10).Value = Convert.ToDecimal(dr["TotalPremium"]);
                    ws.Cell(row, 11).Value = Convert.ToDecimal(dr["InferredPremiumCollected"]);

                    ws.Cell(row, 12).Value = Convert.ToDecimal(dr["AgentCommPercentage"]) / 100;
                    ws.Cell(row, 13).Value = Convert.ToDecimal(dr["Comm."]);
                    ws.Cell(row, 14).Value = Convert.ToDecimal(dr["Comm. Paid to Date"]);
                    //ws.Cell(row, 15).Value = Convert.ToDecimal(dr["Comm. Due"]);
                    decimal comission = Convert.ToDecimal(dr["Comm."]);
                    decimal paidToDate = Convert.ToDecimal(dr["Comm. Paid to Date"]);
                    ws.Cell(row, 15).Value = comission - paidToDate;

                    // Format currency
                    ws.Range(row, 10, row, 11).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, 12).Style.NumberFormat.Format = "0.00%";
                    ws.Range(row, 13, row, 15).Style.NumberFormat.Format = "$ #,##0.00";

                    row++;
                }

                int endRow = row - 1;

                // Footer total
                ws.Cell(row, 14).Value = "Total Comm Due";
                ws.Cell(row, 14).Style.Font.SetBold();
                ws.Cell(row, 15).FormulaA1 = $"=SUM(O{startRow}:O{endRow})";
                ws.Cell(row, 15).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Cell(row, 15).Style.Font.SetBold();

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }

            return null;

        }
        public byte[] DownloadUnderwriterExcel(DataSet ds, DateTime? AsOfDate = null)
        {
            if (ds?.Tables?.Count > 4 && ds.Tables?[4].Rows.Count > 0)
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Commission Statement");

                DateTime date = AsOfDate ?? DateTime.Now;

                string formattedDate = date.ToString("MMMM d, yyyy");

                var table = ds.Tables[3]; // Main data table

                int row = 1;

                // Title
                ws.Cell(row, 4).Value = $"Commission Statement As Of {formattedDate}";
                ws.Range(row, 4, row, 8).Merge().Style
                    .Font.SetBold().Font.SetFontSize(14)
                    .Font.SetFontColor(XLColor.FromHtml("#289FD7"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                row += 2;

                // Headers
                var headers = new[]
                {
        "Underwriter Name", "Underwriter Address", "Insured's Name", "Policy Number", "Carrier", "LOB", "Transaction Type",
        "Invoice Number", "Effective Date", "Comm. Premium", "Premium Collected", "Comm. %",
        "Comm.", "Comm. Paid to Date", "Comm. Due"
    };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(row, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold();
                    cell.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                row++;

                int startRow = row;

                // Iterate through all rows (no grouping)
                foreach (DataRow dr in table.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid"))
                {
                    ws.Cell(row, 1).Value = Convert.ToString(dr["Underwriter Name"]);
                    ws.Cell(row, 2).Value = Convert.ToString(dr["Underwriter Address"]);
                    ws.Cell(row, 3).Value = Convert.ToString(dr["Insured Name"]);
                    ws.Cell(row, 4).Value = Convert.ToString(dr["Policy Number"]);
                    ws.Cell(row, 5).Value = Convert.ToString(dr["Carrier"]);
                    ws.Cell(row, 6).Value = Convert.ToString(dr["LOB"]);
                    ws.Cell(row, 7).Value = Convert.ToString(dr["Transaction Type"]);
                    ws.Cell(row, 8).Value = Convert.ToString(dr["IdNo"]);
                    ws.Cell(row, 9).Value = Convert.ToDateTime(dr["EffectiveDate"]).ToString("M/d/yyyy");
                    ws.Cell(row, 10).Value = Convert.ToDecimal(dr["TotalPremium"]);
                    ws.Cell(row, 11).Value = Convert.ToDecimal(dr["InferredPremiumCollected"]);

                    ws.Cell(row, 12).Value = Convert.ToDecimal(dr["UWCommPercentage"]) / 100;
                    ws.Cell(row, 13).Value = Convert.ToDecimal(dr["Comm."]);
                    ws.Cell(row, 14).Value = Convert.ToDecimal(dr["Comm. Paid to Date"]);
                    //ws.Cell(row, 15).Value = Convert.ToDecimal(dr["Comm. Due"]);
                    decimal comission = Convert.ToDecimal(dr["Comm."]);
                    decimal paidToDate = Convert.ToDecimal(dr["Comm. Paid to Date"]);
                    ws.Cell(row, 15).Value = comission - paidToDate;

                    // Format currency
                    ws.Range(row, 10, row, 11).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, 12).Style.NumberFormat.Format = "0.00%";
                    ws.Range(row, 13, row, 15).Style.NumberFormat.Format = "$ #,##0.00";

                    row++;
                }

                int endRow = row - 1;

                // Footer total
                ws.Cell(row, 14).Value = "Total Comm Due";
                ws.Cell(row, 14).Style.Font.SetBold();
                ws.Cell(row, 15).FormulaA1 = $"=SUM(O{startRow}:O{endRow})";
                ws.Cell(row, 15).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Cell(row, 15).Style.Font.SetBold();

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }

            return null;

        }
        public byte[] DownloadSimplifiedAgencyBillExcel(DataSet ds, bool IsOutstandingInvoices, long Client_Id, DateTime? AsOfDate = null)
        {

            var ReportsInvoiceDueDate = LookupUtility.GetClientSettings(Client_Id, MasterLookup._ClientSettings.ReportsInvoiceDueDate);
            bool ShowInvoiceDate = !string.IsNullOrEmpty(ReportsInvoiceDueDate);
            if (ds?.Tables?.Count > 3 && ds.Tables?[3].Rows.Count > 0)
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Account Statement");

                DateTime date = AsOfDate ?? DateTime.Now;
                string formattedDate = date.ToString("MMMM d, yyyy");

                var table = ds.Tables[3];
                int row = 1;

                // Title
                ws.Cell(row, 1).Value = $"Account Statement As Of {formattedDate}";
                ws.Range(row, 1, row, 10).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(14)
                    .Font.SetFontColor(XLColor.FromHtml("#289FD7"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                row += 2;


                var headers = ShowInvoiceDate ? new[]
                {
                        "Agency Name", "Agency Address", "Insured Name", "Policy Number", "Carrier Name", "LOB",
            "Transaction Type", "Invoice No", "Effective Date", "InvoiceDate", "Gross Amount",
            "Commission Amount", "Net Amount Due", "Financed Paid", "Amount Paid", "Balance Due", "Due Date"
                } : new[]
                {
                   "Agency Name", "Agency Address", "Insured Name", "Policy Number", "Carrier Name", "LOB",
            "Transaction Type", "Invoice No", "Effective Date",  "Gross Amount",
            "Commission Amount", "Net Amount Due", "Financed Paid", "Amount Paid", "Balance Due", "Due Date"
                };



                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(row, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold();
                    cell.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                row++;
                int startRow = row;



                //       var groupedData = table.AsEnumerable()
                //          .Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Cash Receipt")
                //          .GroupBy(r => new
                //          {
                //              PolicyNumber = Convert.ToString(r["PolicyNumber"]),
                //              TransactionType = Convert.ToString(r["TransactionType"]),
                //              Invoice_Id = Convert.ToInt64(r["Invoice_Id"]),
                //              IdNo = Convert.ToString(r["IdNo"]),
                //              IsHouseCommission = Convert.ToBoolean(r["IsHouseCommission"]),
                //              EffectiveDate = Convert.ToDateTime(r["EffectiveDate"]).Date,
                //              InvoiceDate = ShowInvoiceDate ? Convert.ToDateTime(r["InvoiceDate"]).Date : (DateTime?)null,
                //              DueDate = Convert.ToDateTime(r["DueDate"]).Date
                //          })
                //          .Select(g =>
                //          {

                //              var premiumRows = g.Where(x => Convert.ToString(x["LineItemDescription"]).Contains("Premium"));

                //              var nonPremiumRows = g.Where(x => !Convert.ToString(x["LineItemDescription"]).Contains("Premium"));

                //              decimal grossFromPremium = premiumRows != null && premiumRows?.Count() > 0
                //                  ? premiumRows.Sum(x => Convert.ToDecimal(x["GrossAmount"]))
                //                  : 0;

                //              decimal grossFromOthers = nonPremiumRows.Sum(x =>
                //                  Convert.ToDecimal(x["GrossAmount"]));

                //              decimal commissionFromPremium = premiumRows != null && premiumRows?.Count() > 0
                //                   ? premiumRows.Sum(x => Convert.ToDecimal(x["AgentCommission"]))
                //                  : 0;

                //              decimal commissionFromOthers = nonPremiumRows.Sum(x =>
                //                  Convert.ToDecimal(x["AgentCommission"]));

                //              var endorsementNumber = g
                //                  .Where(x => x["EndorsementNumber"] != DBNull.Value)
                //                  .Select(x => Convert.ToString(x["EndorsementNumber"]))
                //                  .FirstOrDefault();


                //              string transactionType = g.Key.TransactionType;

                //              if (g.Key.TransactionType == "END" && !string.IsNullOrEmpty(endorsementNumber))
                //              {
                //                  transactionType = transactionType + " #" + endorsementNumber;
                //              }

                //              return new
                //              {
                //                  AgencyName = Convert.ToString(g.First()["Agency Name"]),
                //                  AgencyAddress = Convert.ToString(g.First()["Agency Address"]),
                //                  InsuredName = Convert.ToString(g.First()["InsuredName"]),
                //                  PolicyNumber = g.Key.PolicyNumber,
                //                  CarrierName = Convert.ToString(g.First()["CarrierName"]),
                //                  LOB = string.Join(", ",
                //g.Where(x => x["LOBNames"] != DBNull.Value)?
                //.Select(x => Convert.ToString(x["LOBNames"]))?
                //.Where(x => !string.IsNullOrWhiteSpace(x))?
                //.Distinct()),


                //                  TransactionType = transactionType,

                //                  InvoiceNo = g.Key.IdNo,
                //                  EffectiveDate = g.Key.EffectiveDate,
                //                  InvoiceDate = ShowInvoiceDate ? g.Key.InvoiceDate : null,
                //                  DueDate = g.Key.DueDate,


                //                  GrossAmount = grossFromPremium + grossFromOthers,
                //                  AgentCommissionAmount = commissionFromPremium + commissionFromOthers,

                //                  NetAmount = g.Sum(x => Convert.ToDecimal(x["NetAmount"])),
                //                  RemainingBalance = g.Sum(x => Convert.ToDecimal(x["RemainingBalance"])),

                //                  AmountFinanced = g.Max(x => Convert.ToDecimal(x["AmountFinanced"])),
                //                  AmountPaid = g.Max(x => Convert.ToDecimal(x["AmountPaid"]))
                //              };
                //          })
                //          .ToList();

                var groupedData = table.AsEnumerable()
                .Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Cash Receipt")
                .GroupBy(r => new
                {
                    PolicyNumber = Convert.ToString(r["PolicyNumber"]),
                    TransactionType = Convert.ToString(r["TransactionType"]),
                    Invoice_Id = Convert.ToInt64(r["Invoice_Id"]),
                    IdNo = Convert.ToString(r["IdNo"]),
                    IsHouseCommission = Convert.ToBoolean(r["IsHouseCommission"]),
                    EffectiveDate = Convert.ToDateTime(r["EffectiveDate"]).Date,
                    InvoiceDate = ShowInvoiceDate ? Convert.ToDateTime(r["InvoiceDate"]).Date : (DateTime?)null,
                    DueDate = Convert.ToDateTime(r["DueDate"]).Date
                })
                .Select(g =>
                {
                    var premiumRows = g.Where(x => Convert.ToString(x["LineItemDescription"]).Contains("Premium"));
                    var firstPremiumRow = premiumRows.FirstOrDefault();
                    var premiumTaxRows = g.Where(x => Convert.ToString(x["LineItemDescription"]).Contains("Premium Tax"));
                    bool hasPremiumTax = premiumTaxRows.Any();
                    var nonPremiumRows = g.Where(x => !Convert.ToString(x["LineItemDescription"]).Contains("Premium"));
                    decimal grossFromPremium = hasPremiumTax ? premiumRows.Sum(x => Convert.ToDecimal(Convert.ToDouble(x["GrossAmount"]))) : firstPremiumRow != null ? Convert.ToDecimal(Convert.ToDouble(firstPremiumRow["GrossAmount"])) : 0m;
                    decimal grossFromOthers = nonPremiumRows.Sum(x => Convert.ToDecimal(x["GrossAmount"]));
                    decimal commissionFromPremium = hasPremiumTax ? premiumRows.Sum(x => (decimal)Convert.ToDouble(x["AgentCommission"])) : firstPremiumRow != null ? (decimal)Convert.ToDouble(firstPremiumRow["AgentCommission"]) : 0m;
                    decimal commissionFromOthers = nonPremiumRows.Sum(x => Convert.ToDecimal(x["AgentCommission"]));
                    var endorsementNumber = g.Where(x => x["EndorsementNumber"] != DBNull.Value).Select(x => Convert.ToString(x["EndorsementNumber"])).FirstOrDefault();
                    string transactionType = g.Key.TransactionType;
                    if (g.Key.TransactionType == "END" && !string.IsNullOrEmpty(endorsementNumber))
                    {
                        transactionType = transactionType + " #" + endorsementNumber;
                    }

                    return new
                    {
                        AgencyName = Convert.ToString(g.First()["Agency Name"]),
                        AgencyAddress = Convert.ToString(g.First()["Agency Address"]),
                        InsuredName = Convert.ToString(g.First()["InsuredName"]),
                        PolicyNumber = g.Key.PolicyNumber,
                        CarrierName = Convert.ToString(g.First()["CarrierName"]),
                        LOB = string.Join(", ", g.Where(x => x["LOBNames"] != DBNull.Value) ? .Select(x => Convert.ToString(x["LOBNames"]))? .Where(x => !string.IsNullOrWhiteSpace(x))?.Distinct()),
                        TransactionType = transactionType,
                        InvoiceNo = g.Key.IdNo,
                        EffectiveDate = g.Key.EffectiveDate,
                        InvoiceDate = ShowInvoiceDate ? g.Key.InvoiceDate : null,
                        DueDate = g.Key.DueDate,
                        GrossAmount = grossFromPremium + grossFromOthers,
                        AgentCommissionAmount = commissionFromPremium + commissionFromOthers,
                        NetAmount = g.Sum(x => Convert.ToDecimal(x["NetAmount"])),
                        RemainingBalance = g.Sum(x => Convert.ToDecimal(x["RemainingBalance"])),
                        AmountFinanced = g.Max(x => Convert.ToDecimal(x["AmountFinanced"])),
                        AmountPaid = g.Max(x => Convert.ToDecimal(x["AmountPaid"]))
                    };
                })
               .ToList();



                //// Iterate through all rows (excluding Cash Receipt)
                //var groupedData = table.AsEnumerable()
                //   .Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Cash Receipt")
                //   .GroupBy(dr => Convert.ToString(dr["IdNo"]))
                //   .Select(g => new
                //   {
                //       AgencyName = Convert.ToString(g.First()["Agency Name"]),
                //       AgencyAddress = Convert.ToString(g.First()["Agency Address"]),
                //       InsuredName = Convert.ToString(g.First()["InsuredName"]),
                //       PolicyNumber = Convert.ToString(g.First()["PolicyNumber"]),
                //       CarrierName = Convert.ToString(g.First()["CarrierName"]),
                //       LOB = Convert.ToString(g.First()["LOBNames"]),
                //       TransactionType = Convert.ToString(g.First()["TransactionType"]),
                //       InvoiceNo = g.Key,
                //       EffectiveDate = g.First()["EffectiveDate"],
                //       DueDate = g.First()["DueDate"],

                //       GrossAmount = g.Sum(x => Convert.ToDecimal(x["GrossAmount"])),
                //       AgentCommissionAmount = g.Sum(x => Convert.ToDecimal(x["AgentCommission"])),
                //       //NetAmountDue = g.Sum(x => Convert.ToDecimal(x["AgentCommissionRemainingBalance"])),
                //       AmountFinanced = g.Max(x => Convert.ToDecimal(x["AmountFinanced"])),
                //       AmountPaid = g.Max(x => Convert.ToDecimal(x["AmountPaid"]))
                //   })
                //   //.OrderBy(x => x.InvoiceNo)
                //   .ToList();


                foreach (var dr in groupedData)
                {

                    decimal netAmountDue = dr.GrossAmount - dr.AgentCommissionAmount;
                    decimal balanceDue = netAmountDue - dr.AmountFinanced - dr.AmountPaid;

                    if (IsOutstandingInvoices && balanceDue == 0)
                    {
                        continue;
                    }

                    int col = 1;

                    ws.Cell(row, col++).Value = dr.AgencyName;
                    ws.Cell(row, col++).Value = dr.AgencyAddress;
                    ws.Cell(row, col++).Value = dr.InsuredName;
                    ws.Cell(row, col++).Value = dr.PolicyNumber;
                    ws.Cell(row, col++).Value = dr.CarrierName;
                    ws.Cell(row, col++).Value = dr.LOB;
                    ws.Cell(row, col++).Value = dr.TransactionType;
                    ws.Cell(row, col++).Value = dr.InvoiceNo;
                    ws.Cell(row, col++).Value = Convert.ToDateTime(dr.EffectiveDate).ToString("M/d/yyyy");

                    if (ShowInvoiceDate)
                    {
                        ws.Cell(row, col++).Value =
                            Convert.ToDateTime(dr.InvoiceDate).ToString("M/d/yyyy");
                    }

                    int grossAmountCol = col;

                    ws.Cell(row, col++).Value = dr.GrossAmount;
                    ws.Cell(row, col++).Value = dr.AgentCommissionAmount;

                    netAmountDue = dr.GrossAmount - dr.AgentCommissionAmount;
                    ws.Cell(row, col++).Value = netAmountDue;

                    ws.Cell(row, col++).Value = dr.AmountFinanced;
                    ws.Cell(row, col++).Value = dr.AmountPaid;

                    balanceDue = netAmountDue - dr.AmountFinanced - dr.AmountPaid;
                    ws.Cell(row, col++).Value = balanceDue;



                    ws.Cell(row, col++).Value =
                        Convert.ToDateTime(dr.DueDate).ToString("M/d/yyyy");

                    // Currency Formatting
                    ws.Cell(row, grossAmountCol).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, grossAmountCol + 1).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, grossAmountCol + 2).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, grossAmountCol + 3).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, grossAmountCol + 4).Style.NumberFormat.Format = "$ #,##0.00";
                    ws.Cell(row, grossAmountCol + 5).Style.NumberFormat.Format = "$ #,##0.00";

                    row++;
                }

                int endRow = row - 1;

                int balanceDueCol = Array.IndexOf(headers, "Balance Due") + 1;
                int totalLabelCol = balanceDueCol - 1;
                int totalValueCol = balanceDueCol;
                string balanceDueColumn = XLHelper.GetColumnLetterFromNumber(balanceDueCol);

                ws.Cell(row, totalLabelCol).Value = "Total Net Due";
                ws.Cell(row, totalLabelCol).Style.Font.SetBold();

                ws.Cell(row, totalValueCol).FormulaA1 =
                    $"=SUM({balanceDueColumn}{startRow}:{balanceDueColumn}{endRow})";

                ws.Cell(row, totalValueCol).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Cell(row, totalValueCol).Style.Font.SetBold();

                ws.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            return null;

            //making it dynamically based on showinvoicedate

            //    foreach (var dr in groupedData)
            //    {
            //        ws.Cell(row, 1).Value = dr.AgencyName;
            //        ws.Cell(row, 2).Value = dr.AgencyAddress;
            //        ws.Cell(row, 3).Value = dr.InsuredName;
            //        ws.Cell(row, 4).Value = dr.PolicyNumber;
            //        ws.Cell(row, 5).Value = dr.CarrierName;
            //        ws.Cell(row, 6).Value = dr.LOB;
            //        ws.Cell(row, 7).Value = dr.TransactionType;
            //        ws.Cell(row, 8).Value = dr.InvoiceNo;
            //        ws.Cell(row, 9).Value = Convert.ToDateTime(dr.EffectiveDate).ToString("M/d/yyyy");
            //        ws.Cell(row, 10).Value = Convert.ToDateTime(dr.InvoiceDate).ToString("M/d/yyyy");
            //        ws.Cell(row, 11).Value = dr.GrossAmount;
            //        ws.Cell(row, 12).Value = dr.AgentCommissionAmount;
            //        decimal NetAmountDue = dr.GrossAmount - dr.AgentCommissionAmount;
            //        ws.Cell(row, 13).Value = NetAmountDue;
            //        ws.Cell(row, 14).Value = dr.AmountFinanced;
            //        ws.Cell(row, 15).Value = dr.AmountPaid;

            //        //decimal balanceDue = dr.AgentCommissionAmount - dr.AmountPaid;
            //        decimal balanceDue = NetAmountDue - dr.AmountFinanced - dr.AmountPaid;
            //        ws.Cell(row, 16).Value = balanceDue;

            //        ws.Cell(row, 17).Value = Convert.ToDateTime(dr.DueDate).ToString("M/d/yyyy");
            //        // Formatting
            //        ws.Cell(row, 11).Style.NumberFormat.Format = "$ #,##0.00"; // Gross
            //        ws.Range(row, 12, row, 13).Style.NumberFormat.Format = "$ #,##0.00"; // Commission + Remaining
            //        ws.Range(row, 14, row, 15).Style.NumberFormat.Format = "$ #,##0.00"; // Financed + Paid
            //        ws.Cell(row, 16).Style.NumberFormat.Format = "$ #,##0.00"; // Net Due

            //        row++;
            //    }

            //    int endRow = row - 1;

            //    // Footer total
            //    ws.Cell(row, 14).Value = "Total Net Due";
            //    ws.Cell(row, 14).Style.Font.SetBold();
            //    ws.Cell(row, 15).FormulaA1 = $"=SUM(O{startRow}:O{endRow})";
            //    ws.Cell(row, 15).Style.NumberFormat.Format = "$ #,##0.00";
            //    ws.Cell(row, 15).Style.Font.SetBold();


            //    ws.Columns().AdjustToContents();

            //    using var stream = new MemoryStream();
            //    workbook.SaveAs(stream);
            //    return stream.ToArray();
            //}


        }


    }
}
