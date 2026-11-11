using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Abstract;
using Accounting.Web.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Accounting.Web.Services
{
    public class ReportService : IReportService
    {
        internal readonly IPolicyDetailsRepository _policyDetailsRepository;
        internal readonly IClientsRepository _accountRepository;
        private readonly IZip _IZip;
        private readonly IMSOfficeFileOptions _IMSOfficeFileOptions;
        private readonly ICSVOptions _ICSVOptions;
        private readonly IPDFOptions _IPDFOptions;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private readonly ViewRender view;
        IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();

        public ReportService(IPolicyDetailsRepository policyDetailsRepository, IClientsRepository accountRepository, IZip iZip, IMSOfficeFileOptions iMSOfficeFileOptions, ICSVOptions iCSVOptions, IPDFOptions pDFOptions, IWebHostEnvironment iWebHostEnvironment, ViewRender view)
        {
            _policyDetailsRepository = policyDetailsRepository;
            _accountRepository = accountRepository;
            _IZip = iZip;
            _IMSOfficeFileOptions = iMSOfficeFileOptions;
            _ICSVOptions = iCSVOptions;
            _IPDFOptions = pDFOptions;
            _IWebHostEnvironment = iWebHostEnvironment;
            this.view = view;
        }

        public string BuildReportHtmlFromTemplate(long Report_Id,  long Client_Id, List<ReportOutputTables> reportOutputTablesList, LU_ReportTemplates ReportTemplate, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null)
        {
            string FinalHTMLAlongWithTemplate = "";
            try
            {

                var Clients = _accountRepository.GetClients(Client_Id);
                string DynamicReportHTMLContent = "";
                string ClientLogo = $"<img src=\"data:image/jpg;base64,{Convert.ToBase64String(Clients.Logo)}\" alt=\"Client Logo\" style=\"height:45px;vertical-align: middle;\">";
                string ApplyFonts = "<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\" />\r\n<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin=\"anonymous\" />\r\n<link href=\"https://fonts.googleapis.com/css2?family=Poppins:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800;1,900&display=swap\" rel=\"stylesheet\" />";
                if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectBilledAccountStatement)
                {
                    double totalNetAmount = 0;
                    var tableAgency = reportOutputTablesList[0];
                    var tablePolicies = reportOutputTablesList[1].DataTable;
                    var table3 = reportOutputTablesList[2].DataTable;
                    var table4 = reportOutputTablesList[3].DataTable;

                    // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

                    var agencyTableFinal = tableAgency.DataTable.Clone();
                    var policiesTableFinal = tablePolicies.Clone();
                    var invoicesTableFinal = table3.Clone();
                    var table4Final = table4.Clone();
                    var finalInvoiceTable = table4.Clone();
                    if (IsOutstandingInvoices)
                    {
                        var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));


                        foreach (var agencyGroup in agencyGroups)
                        {
                            long agentDetailId = agencyGroup.Key;

                            var agencyTableTemp = tableAgency.DataTable.Clone();
                            foreach (var row in agencyGroup)
                                agencyTableTemp.ImportRow(row);

                            var policiesForAgent = tablePolicies.AsEnumerable()
                                .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                            var policiesTableTemp = tablePolicies.Clone();
                            foreach (var row in policiesForAgent)
                                policiesTableTemp.ImportRow(row);

                            var policyIds = policiesForAgent
                                .Where(r => !r.IsNull("Id"))
                                .Select(r => r.Field<long>("Id"))
                                .ToHashSet();

                            var invoicesForAgent = table3.AsEnumerable()
                                .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                            var invoicesTableTemp = table3.Clone();
                            foreach (var row in invoicesForAgent)
                                invoicesTableTemp.ImportRow(row);

                            var filteredTable4Rows = table4.AsEnumerable()
                                .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                            var table4Temp = table4.Clone();
                            foreach (var row in filteredTable4Rows)
                                table4Temp.ImportRow(row);

                            //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
                            var policiesToRemove = table4Temp.AsEnumerable()
                                .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
                                .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
                                .Select(g => g.Key)
                                .ToHashSet();

                            if (policiesToRemove.Any())
                            {
                                foreach (var row in policiesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        policiesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        invoicesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in table4Temp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        table4Temp.Rows.Remove(row);
                                }
                            }

                            if (table4Temp.Rows.Count > 0)
                            {
                                foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                                foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                                foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                                foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                            }
                        }

                        foreach (DataRow row in table4Final.Rows)
                        {
                            finalInvoiceTable.ImportRow(row);
                        }
                        var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
                            .Select(r => r.Field<long>("AgentDetail_Id")).Distinct().ToList();

                        var filteredAgencies = tableAgency.DataTable.AsEnumerable()
                            .Where(a => validAgencyIds.Contains(a.Field<long>("AgentDetail_Id")));

                        if (filteredAgencies.Any())
                            tableAgency.DataTable = filteredAgencies.CopyToDataTable();
                        else
                            tableAgency.DataTable = tableAgency.DataTable.Clone();

                    }
                    else
                    {
                        finalInvoiceTable = table4;

                    }

                    if (IsSimplifiedView)
                    {
                        var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

                        if (filteredRows.Any())
                            finalInvoiceTable = filteredRows.CopyToDataTable();
                    }
                    var commPaidRows = finalInvoiceTable.AsEnumerable()
                    .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
                    IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

                    var groupedRows = finalInvoiceTable.AsEnumerable()
                            .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
                            .GroupBy(r => new
                            {
                                PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
                                InvoiceId = r.Field<long>("Invoice_Id")
                            })
                            .Select(g => g.First());

                    DataTable tempGroupedRows;
                    DataTable result;

                    // if (groupedRows != null && groupedRows.Any())
                    // {
                    // finalDistinct = groupedRows.Concat(commPaidRows);
                    DataTable distinctPremiumTable = finalInvoiceTable.Clone();
                    if (groupedRows.Any())
                    {
                        distinctPremiumTable = groupedRows.CopyToDataTable();
                    }
                    ApplyMultiLobInvoiceTotals(finalInvoiceTable, distinctPremiumTable);

                    finalDistinct = distinctPremiumTable.AsEnumerable().Concat(commPaidRows);
                    if (finalDistinct != null && finalDistinct.Any())
                    {
                        result = finalDistinct.CopyToDataTable();

                        reportOutputTablesList[0].DataTable = tableAgency.DataTable;
                        reportOutputTablesList[3].DataTable = result;

                        if (reportOutputTablesList.Count > 3)
                        {
                            var invoiceDetails = result;
                            totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
                        }

                        var viewDataDictionary = new ViewDataDictionary
                                               (
                                       new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                        {
                            Model = reportOutputTablesList,
                        };
                        viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
                        //ViewBag.IsSimplifiedView = IsSimplifiedView;
                        DynamicReportHTMLContent = this.view.Render("Reports/DirectBilledAccountStatementDynamicReport", viewDataDictionary);
                        if (!IsSimplifiedView)
                        {
                            // Remove Comm. Paid to Date, Comm. Due columns + simplified-block
                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "Comm. Paid to Date", "Comm. Due" });
                        }
                        else
                        {
                            DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                            RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                            .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                           .Replace("{{clientLogo}}", ClientLogo)
                           //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
                    }
                    //}
                }
                else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.AgencyBilledAccountStatement)
                {
                    double totalNetAmount = 0;
                    var tableAgency = reportOutputTablesList[0];
                    var tablePolicies = reportOutputTablesList[1];
                    var tableInvoices = reportOutputTablesList[2];
                    var table4 = reportOutputTablesList[3];
                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));

                    var agencyTableFinal = tableAgency.DataTable.Clone();
                    var policiesTableFinal = tablePolicies.DataTable.Clone();
                    var invoicesTableFinal = tableInvoices.DataTable.Clone();
                    var table4Final = table4.DataTable.Clone();
                    var ReportsInvoiceDueDate = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.ReportsInvoiceDueDate);
                    IsSimplifiedView = true; // added
                    foreach (var agencyGroup in agencyGroups)
                    {
                        long agentDetailId = agencyGroup.Key;

                        var agencyTableTemp = tableAgency.DataTable.Clone();
                        foreach (var row in agencyGroup)
                            agencyTableTemp.ImportRow(row);

                        var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                        var policiesTableTemp = tablePolicies.DataTable.Clone();
                        foreach (var row in policiesForAgent)
                            policiesTableTemp.ImportRow(row);

                        var policyIds = policiesForAgent
                            .Where(r => !r.IsNull("Id"))
                            .Select(r => r.Field<long>("Id"))
                            .ToHashSet();

                        var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                        var invoicesTableTemp = tableInvoices.DataTable.Clone();
                        foreach (var row in invoicesForAgent)
                            invoicesTableTemp.ImportRow(row);

                        var filteredTable4Rows = table4.DataTable.AsEnumerable()
                            .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                        var table4Temp = table4.DataTable.Clone();
                        foreach (var row in filteredTable4Rows)
                            table4Temp.ImportRow(row);

                        if (table4Temp.Rows.Count > 0)
                        {
                            foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                            foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                            foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                            foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                        }
                    }
                    DataTable resultTable = table4Final.Clone();

                    var groupedNonCash = table4Final.AsEnumerable()
                        .Where(r => r.Field<string>("LineItemDescription") != "Cash Receipt")
                        .GroupBy(r => new
                        {
                            PolicyDetails_Id = r.Field<long>("PolicyDetails_Id"),
                            Invoice_Id = r.Field<long>("Invoice_Id"),
                            PolicyNumber = r.Field<string>("PolicyNumber"),
                            TransactionType = r.Field<string>("TransactionType"),
                            LineItemDescription = r.Field<string>("LineItemDescription"),
                            IdNo = r.Field<string>("IdNo")
                        });

                    foreach (var g in groupedNonCash)
                    {
                        var first = g.First();
                        string lineItem = first.Field<string>("LineItemDescription");

                        DataRow row = resultTable.NewRow();

                        // Copy common columns
                        foreach (DataColumn col in resultTable.Columns)
                        {
                            if (col.ColumnName != "InvoiceDetail_Id" &&
                                col.ColumnName != "GrossAmount" &&
                                col.ColumnName != "AgentCommission" &&
                                col.ColumnName != "AgentCommissionRemainingBalance" &&
                                col.ColumnName != "NetAmount" &&
                                col.ColumnName != "RemainingBalance")
                            {
                                row[col.ColumnName] = first[col.ColumnName];
                            }
                        }

                        if (lineItem.Contains("Premium"))
                        {
                            row["GrossAmount"] = first["GrossAmount"];
                            row["AgentCommission"] = first["AgentCommission"];
                            row["AgentCommissionRemainingBalance"] = first["AgentCommissionRemainingBalance"];
                            row["NetAmount"] = first["NetAmount"];
                            //row["RemainingBalance"] = first["RemainingBalance"];
                            row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
                        }
                        else
                        {
                            row["GrossAmount"] = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0);
                            row["AgentCommission"] = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0);
                            row["AgentCommissionRemainingBalance"] = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0);
                            row["NetAmount"] = g.Sum(x => x.Field<double?>("NetAmount") ?? 0);
                            row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
                        }

                        resultTable.Rows.Add(row);
                    }

                    var groups = table4Final.AsEnumerable()
    .Where(r => r.Field<string>("LineItemDescription") == "Cash Receipt")
    .GroupBy(r => new
    {
        InvoiceId = r.Field<long>("Invoice_Id"),
        IdNo = r.Field<string>("IdNo")
    });

                    foreach (var g in groups)
                    {
                        var first = g.First();
                        DataRow newRow = resultTable.NewRow();

                        foreach (DataColumn col in resultTable.Columns)
                        {
                            if (col.ColumnName != "NetAmount")
                                newRow[col.ColumnName] = first[col.ColumnName];
                        }

                        newRow["NetAmount"] = g.Sum(r =>
                            r.Field<double?>("NetAmount") ?? 0);

                        resultTable.Rows.Add(newRow);
                    }



                    table4Final.Clear();
                    table4Final.Merge(resultTable);


                    if (IsSimplifiedView)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("PolicyNumber", typeof(string));
                        dt.Columns.Add("TransactionType", typeof(string));
                        dt.Columns.Add("Invoice_Id", typeof(long));
                        dt.Columns.Add("IdNo", typeof(string));
                        dt.Columns.Add("IsHouseCommission", typeof(bool));
                        dt.Columns.Add("ExpirationDate", typeof(DateTime));
                        dt.Columns.Add("EffectiveDate", typeof(DateTime));
                        if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
                        {
                            dt.Columns.Add("InvoiceDate", typeof(DateTime));
                        }
                        dt.Columns.Add("DueDate", typeof(DateTime));
                        dt.Columns.Add("GrossAmount", typeof(double));
                        dt.Columns.Add("AgentCommPercentage", typeof(double));
                        dt.Columns.Add("AgentCommission", typeof(double));
                        dt.Columns.Add("RemainingBalance", typeof(double));
                        dt.Columns.Add("AgentCommissionRemainingBalance", typeof(double));
                        dt.Columns.Add("NetAmount", typeof(double));
                        dt.Columns.Add("AmountDue", typeof(double));
                        dt.Columns.Add("AmountFinanced", typeof(double));
                        dt.Columns.Add("AmountPaid", typeof(double));
                        dt.Columns.Add("EndorsementNumber", typeof(int));
                        var paymentTotalsByInvoice = table4Final.AsEnumerable().Where(r => r.Field<string>("LineItemDescription")?.Trim()
                                    .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true && r.Field<bool>("IsHouseCommission") == false)
                            .GroupBy(r => r.Field<long>("Invoice_Id"))
                            .ToDictionary(
                                g => g.Key,
                                g => g.Sum(x => Math.Abs(x.Field<double?>("NetAmount") ?? 0))
                            );


                        //var invoiceSummary = table4Final.AsEnumerable()
                        //    .Where(r =>
                        //        !(r.Field<string>("LineItemDescription")?
                        //            .Trim()
                        //            .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
                        //    )
                        //    .GroupBy(r => new
                        //    {
                        //        PolicyNumber = r.Field<string>("PolicyNumber"),
                        //        TransactionType = r.Field<string>("TransactionType"),
                        //        Invoice_Id = r.Field<long>("Invoice_Id"),
                        //        IdNo = r.Field<string>("IdNo"),
                        //        IsHouseCommission = r.Field<bool>("IsHouseCommission"),
                        //        EffectiveDate = r.Field<DateTime>("EffectiveDate"),
                        //        ExpirationDate = r.Field<DateTime>("ExpirationDate"),
                        //        DueDate = r.Field<DateTime>("DueDate"),
                        //        AmountFinanced = r.Field<double?>("AmountFinanced") ?? 0,
                        //        AmountPaid = r.Field<double?>("AmountPaid") ?? 0
                        //    })
                        //    .Select(g => new
                        //    {
                        //        g.Key.PolicyNumber,
                        //        g.Key.TransactionType,
                        //        g.Key.Invoice_Id,
                        //        g.Key.IdNo,
                        //        g.Key.IsHouseCommission,
                        //        g.Key.ExpirationDate,
                        //        g.Key.EffectiveDate,
                        //        g.Key.DueDate,
                        //        g.Key.AmountFinanced,
                        //        g.Key.AmountPaid,
                        //        GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
                        //        AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
                        //        AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
                        //        RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
                        //        AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
                        //        NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
                        //        AmountDue = g.Sum(x => (x.Field<double?>("GrossAmount") ?? 0) - (x.Field<double?>("AgentCommission") ?? 0)),                               
                        //        //AmountPaid = g.Sum(x => x.Field<double?>("AmountPaid") ?? 0)


                        //    });
                        var invoiceSummary = table4Final.AsEnumerable()
                            .Where(r =>
                                !(r.Field<string>("LineItemDescription")?
                                    .Trim()
                                    .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
                            )
                            .GroupBy(r => new
                            {
                                PolicyNumber = r.Field<string>("PolicyNumber"),
                                TransactionType = r.Field<string>("TransactionType"),
                                Invoice_Id = r.Field<long>("Invoice_Id"),
                                IdNo = r.Field<string>("IdNo"),
                                IsHouseCommission = r.Field<bool>("IsHouseCommission"),
                                EffectiveDate = r.Field<DateTime>("EffectiveDate").Date,
                                InvoiceDate = r.Field<DateTime>("InvoiceDate").Date,
                                ExpirationDate = r.Field<DateTime>("ExpirationDate").Date,
                                DueDate = r.Field<DateTime>("DueDate").Date
                            })
                            .Select(g => new
                            {
                                g.Key.PolicyNumber,
                                g.Key.TransactionType,
                                g.Key.Invoice_Id,
                                g.Key.IdNo,
                                g.Key.IsHouseCommission,
                                g.Key.ExpirationDate,
                                g.Key.EffectiveDate,
                                g.Key.InvoiceDate,
                                g.Key.DueDate,

                                AmountFinanced = g.Max(x => x.Field<double?>("AmountFinanced") ?? 0),
                                AmountPaid = g.Max(x => x.Field<double?>("AmountPaid") ?? 0),
                                EndorsementNumber = g.Max(x => x.Field<int?>("EndorsementNumber") ?? 0),
                                GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
                                AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
                                AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
                                RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
                                AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
                                NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
                                AmountDue = g.Sum(x =>
                                    (x.Field<double?>("GrossAmount") ?? 0) -
                                    (x.Field<double?>("AgentCommission") ?? 0))
                            });

                        foreach (var row in invoiceSummary)
                        {
                            //double amountPaid =
                            //    paymentTotalsByInvoice.TryGetValue(row.Invoice_Id, out var paid)
                            //        ? paid
                            //        : 0;

                            if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
                            {
                                dt.Rows.Add(
                                row.PolicyNumber,
                                row.TransactionType,
                                row.Invoice_Id,
                                row.IdNo,
                                row.IsHouseCommission,
                                row.ExpirationDate,
                                row.EffectiveDate,
                                row.InvoiceDate,
                                row.DueDate,
                                row.GrossAmount,
                                row.AgentCommPercentage,
                                row.AgentCommission,
                                row.RemainingBalance,
                                row.AgentCommissionRemainingBalance,
                                row.NetAmount,
                               row.AmountDue,
                               row.AmountFinanced,
                           // amountPaid
                           row.AmountPaid,
                           row.EndorsementNumber
                            );
                            }
                            else
                            {
                                dt.Rows.Add(
                                    row.PolicyNumber,
                                    row.TransactionType,
                                    row.Invoice_Id,
                                    row.IdNo,
                                    row.IsHouseCommission,
                                    row.ExpirationDate,
                                    row.EffectiveDate,
                                    row.DueDate,
                                    row.GrossAmount,
                                    row.AgentCommPercentage,
                                    row.AgentCommission,
                                    row.RemainingBalance,
                                    row.AgentCommissionRemainingBalance,
                                    row.NetAmount,
                                   row.AmountDue,
                                   row.AmountFinanced,
                               // amountPaid
                               row.AmountPaid,
                               row.EndorsementNumber
                                );
                            }


                        }

                        table4Final = dt;
                    }
                    table4Final = _IMSOfficeFileOptions.BalanceDueInAgencyBill(IsOutstandingInvoices, table4Final);

                    if (table4Final.Rows.Count > 0)
                    {
                        List<ReportOutputTables> agencyOutputTables = new()
                        {
                            new() { DataTable = agencyTableFinal },
                            new() { DataTable = policiesTableFinal },
                            new() { DataTable = invoicesTableFinal },
                            new() { DataTable = table4Final }
                        };

                        var policyTable = agencyOutputTables?[3]?.DataTable;
                        var policyAgentTable = agencyOutputTables?[2]?.DataTable;
                        var agentTable = agencyOutputTables?[0]?.DataTable;
                        var distinctPolicyNumbers = policyTable != null && policyTable.Columns.Contains("PolicyNumber") ? policyTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")).Select(r => r.Field<string>("PolicyNumber")).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList() : new List<string>();
                        var distinctAgents = policyAgentTable != null && policyAgentTable.Columns.Contains("PolicyNumber") && policyAgentTable.Columns.Contains("AgentDetail_Id") ? policyAgentTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")
                                                && distinctPolicyNumbers.Contains(r.Field<string>("PolicyNumber"))
                                                && !r.IsNull("AgentDetail_Id")).Select(r => Convert.ToInt64(r["AgentDetail_Id"])).Distinct().ToList() : new List<long>();
                        var filteredAgentsTable = agentTable?.Clone() ?? new DataTable();
                        if (agentTable != null && agentTable.Columns.Contains("AgentDetail_Id"))
                        {
                            var agentIds = distinctAgents.ToHashSet();
                            var rows = agentTable.AsEnumerable().Where(r => !r.IsNull("AgentDetail_Id") && agentIds.Contains(Convert.ToInt64(r["AgentDetail_Id"])));
                            if (rows.Any())
                            {
                                filteredAgentsTable = rows.CopyToDataTable();
                                agencyOutputTables[0].DataTable = filteredAgentsTable;
                            }
                        }

                        if (agencyOutputTables.Count > 3)
                        {
                            var invoiceDetails = agencyOutputTables[3].DataTable;
                            totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("NetAmount")) ?? 0;
                        }
                        var viewDataDictionary = new ViewDataDictionary
                                               (
                                       new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                        {
                            Model = agencyOutputTables,
                        };

                        viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
                        //ViewBag.IsSimplifiedView = IsSimplifiedView;
                        viewDataDictionary["ReportsInvoiceDueDate"] = ReportsInvoiceDueDate;
                        //ViewBag.ReportsInvoiceDueDate = ReportsInvoiceDueDate;
                        viewDataDictionary["Client_Id"] = Client_Id;
                        DynamicReportHTMLContent = this.view.Render("Reports/AgencyBilledAccountStatementDynamicReport", viewDataDictionary);

                        if (!IsSimplifiedView)
                        {

                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "Amount Due", "Amount Paid", "NET Due" });
                        }
                        else
                        {
                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "NET" });
                            DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                            RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                                                                               .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                                                                               .Replace("{{clientAddress}}", Clients?.Address ?? "")
                                                                               .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                                                                               .Replace("{{clientLogo}}", ClientLogo)
                                                                               //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ServerDateFormat))
                                                                               .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                                                                               .Replace("{{dynamicAgentloopingLogic}}", DynamicReportHTMLContent)
                                                                               .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString())
                                                                               ;

                    }
                }
                else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.InsuredBilledAccountStatement)
                {
                    double totalNetAmount = 0;
                    var tableAgency = reportOutputTablesList[0];
                    var tablePolicies = reportOutputTablesList[1].DataTable;
                    var table3 = reportOutputTablesList[2].DataTable;
                    var table4 = reportOutputTablesList[3].DataTable;

                    // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

                    var agencyTableFinal = tableAgency.DataTable.Clone();
                    var policiesTableFinal = tablePolicies.Clone();
                    var invoicesTableFinal = table3.Clone();
                    var table4Final = table4.Clone();
                    var finalInvoiceTable = table4.Clone();
                    if (IsOutstandingInvoices)
                    {
                        var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));


                        foreach (var agencyGroup in agencyGroups)
                        {
                            long agentDetailId = agencyGroup.Key;

                            var agencyTableTemp = tableAgency.DataTable.Clone();
                            foreach (var row in agencyGroup)
                                agencyTableTemp.ImportRow(row);

                            var policiesForAgent = tablePolicies.AsEnumerable()
                                .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                            var policiesTableTemp = tablePolicies.Clone();
                            foreach (var row in policiesForAgent)
                                policiesTableTemp.ImportRow(row);

                            var policyIds = policiesForAgent
                                .Where(r => !r.IsNull("Id"))
                                .Select(r => r.Field<long>("Id"))
                                .ToHashSet();

                            var invoicesForAgent = table3.AsEnumerable()
                                .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                            var invoicesTableTemp = table3.Clone();
                            foreach (var row in invoicesForAgent)
                                invoicesTableTemp.ImportRow(row);

                            var filteredTable4Rows = table4.AsEnumerable()
                                .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                            var table4Temp = table4.Clone();
                            foreach (var row in filteredTable4Rows)
                                table4Temp.ImportRow(row);

                            //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
                            var policiesToRemove = table4Temp.AsEnumerable()
                                .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
                                .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
                                .Select(g => g.Key)
                                .ToHashSet();

                            if (policiesToRemove.Any())
                            {
                                foreach (var row in policiesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        policiesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        invoicesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in table4Temp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        table4Temp.Rows.Remove(row);
                                }
                            }

                            if (table4Temp.Rows.Count > 0)
                            {
                                foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                                foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                                foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                                foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                            }
                        }

                        foreach (DataRow row in table4Final.Rows)
                        {
                            finalInvoiceTable.ImportRow(row);
                        }
                        var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
                            .Select(r => r.Field<long>("AgentDetail_Id")).Distinct().ToList();

                        var filteredAgencies = tableAgency.DataTable.AsEnumerable()
                            .Where(a => validAgencyIds.Contains(a.Field<long>("AgentDetail_Id")));

                        if (filteredAgencies.Any())
                            tableAgency.DataTable = filteredAgencies.CopyToDataTable();
                        else
                            tableAgency.DataTable = tableAgency.DataTable.Clone();

                    }
                    else
                    {
                        finalInvoiceTable = table4;

                    }

                    if (IsSimplifiedView)
                    {
                        var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

                        if (filteredRows.Any())
                            finalInvoiceTable = filteredRows.CopyToDataTable();
                    }
                    var commPaidRows = finalInvoiceTable.AsEnumerable()
                    .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
                    IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

                    var groupedRows = finalInvoiceTable.AsEnumerable()
                            .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
                            .GroupBy(r => new
                            {
                                PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
                                InvoiceId = r.Field<long>("Invoice_Id")
                            })
                            .Select(g => g.First());

                    DataTable tempGroupedRows;
                    DataTable result;

                    finalDistinct = groupedRows.Concat(commPaidRows);
                    if (finalDistinct != null && finalDistinct.Any())
                    {
                        result = finalDistinct.CopyToDataTable();

                        reportOutputTablesList[0].DataTable = tableAgency.DataTable;
                        reportOutputTablesList[3].DataTable = result;

                        if (reportOutputTablesList.Count > 3)
                        {
                            var invoiceDetails = result;
                            totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
                        }

                        var viewDataDictionary = new ViewDataDictionary
                                               (
                                       new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                        {
                            Model = reportOutputTablesList,
                        };
                        viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
                        //ViewBag.IsSimplifiedView = IsSimplifiedView;
                        DynamicReportHTMLContent = this.view.Render("Reports/InsuredBilledAccountStatementDynamicReport", viewDataDictionary);
                        if (!IsSimplifiedView)
                        {
                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "Comm. Paid to Date", "Comm. Due" });
                        }
                        else
                        {
                            DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                            RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                            .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                           .Replace("{{clientLogo}}", ClientLogo)
                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
                    }
                }
                else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.AgencyDetailedAccountStatement)
                {
                    double totalNetAmount = 0;
                    var tableAgency = reportOutputTablesList[0];
                    var tablePolicies = reportOutputTablesList[1];
                    var tableInvoices = reportOutputTablesList[2];
                    var table4 = reportOutputTablesList[3];
                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));

                    var agencyTableFinal = tableAgency.DataTable.Clone();
                    var policiesTableFinal = tablePolicies.DataTable.Clone();
                    var invoicesTableFinal = tableInvoices.DataTable.Clone();
                    var table4Final = table4.DataTable.Clone();
                    var ReportsInvoiceDueDate = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.ReportsInvoiceDueDate);

                    foreach (var agencyGroup in agencyGroups)
                    {
                        long agentDetailId = agencyGroup.Key;

                        var agencyTableTemp = tableAgency.DataTable.Clone();
                        foreach (var row in agencyGroup)
                            agencyTableTemp.ImportRow(row);

                        var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                        var policiesTableTemp = tablePolicies.DataTable.Clone();
                        foreach (var row in policiesForAgent)
                            policiesTableTemp.ImportRow(row);

                        var policyIds = policiesForAgent
                            .Where(r => !r.IsNull("Id"))
                            .Select(r => r.Field<long>("Id"))
                            .ToHashSet();

                        var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                        var invoicesTableTemp = tableInvoices.DataTable.Clone();
                        foreach (var row in invoicesForAgent)
                            invoicesTableTemp.ImportRow(row);

                        var filteredTable4Rows = table4.DataTable.AsEnumerable()
                            .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                        var table4Temp = table4.DataTable.Clone();
                        foreach (var row in filteredTable4Rows)
                            table4Temp.ImportRow(row);

                        if (table4Temp.Rows.Count > 0)
                        {
                            foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                            foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                            foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                            foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                        }
                    }
                    DataTable resultTable = table4Final.Clone();

                    var groupedNonCash = table4Final.AsEnumerable()
                        .Where(r => r.Field<string>("LineItemDescription") != "Cash Receipt")
                        .GroupBy(r => new
                        {
                            PolicyDetails_Id = r.Field<long>("PolicyDetails_Id"),
                            Invoice_Id = r.Field<long>("Invoice_Id"),
                            PolicyNumber = r.Field<string>("PolicyNumber"),
                            TransactionType = r.Field<string>("TransactionType"),
                            LineItemDescription = r.Field<string>("LineItemDescription"),
                            IdNo = r.Field<string>("IdNo")
                        });

                    foreach (var g in groupedNonCash)
                    {
                        var first = g.First();
                        string lineItem = first.Field<string>("LineItemDescription");

                        DataRow row = resultTable.NewRow();

                        // Copy common columns
                        foreach (DataColumn col in resultTable.Columns)
                        {
                            if (col.ColumnName != "InvoiceDetail_Id" &&
                                col.ColumnName != "GrossAmount" &&
                                col.ColumnName != "AgentCommission" &&
                                col.ColumnName != "AgentCommissionRemainingBalance" &&
                                col.ColumnName != "NetAmount" &&
                                col.ColumnName != "RemainingBalance")
                            {
                                row[col.ColumnName] = first[col.ColumnName];
                            }
                        }

                        if (lineItem.Contains("Premium"))
                        {
                            row["GrossAmount"] = first["GrossAmount"];
                            row["AgentCommission"] = first["AgentCommission"];
                            row["AgentCommissionRemainingBalance"] = first["AgentCommissionRemainingBalance"];
                            row["NetAmount"] = first["NetAmount"];
                            //row["RemainingBalance"] = first["RemainingBalance"];
                            row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
                        }
                        else
                        {
                            row["GrossAmount"] = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0);
                            row["AgentCommission"] = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0);
                            row["AgentCommissionRemainingBalance"] = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0);
                            row["NetAmount"] = g.Sum(x => x.Field<double?>("NetAmount") ?? 0);
                            row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
                        }

                        resultTable.Rows.Add(row);
                    }

                    var groups = table4Final.AsEnumerable()
    .Where(r => r.Field<string>("LineItemDescription") == "Cash Receipt")
    .GroupBy(r => new
    {
        InvoiceId = r.Field<long>("Invoice_Id"),
        IdNo = r.Field<string>("IdNo")
    });

                    foreach (var g in groups)
                    {
                        var first = g.First();
                        DataRow newRow = resultTable.NewRow();

                        foreach (DataColumn col in resultTable.Columns)
                        {
                            if (col.ColumnName != "NetAmount")
                                newRow[col.ColumnName] = first[col.ColumnName];
                        }

                        newRow["NetAmount"] = g.Sum(r =>
                            r.Field<double?>("NetAmount") ?? 0);

                        resultTable.Rows.Add(newRow);
                    }



                    table4Final.Clear();
                    table4Final.Merge(resultTable);


                    if (IsSimplifiedView)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("PolicyNumber", typeof(string));
                        dt.Columns.Add("TransactionType", typeof(string));
                        dt.Columns.Add("Invoice_Id", typeof(long));
                        dt.Columns.Add("IdNo", typeof(string));
                        dt.Columns.Add("IsHouseCommission", typeof(bool));
                        dt.Columns.Add("ExpirationDate", typeof(DateTime));
                        dt.Columns.Add("EffectiveDate", typeof(DateTime));
                        if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
                        {
                            dt.Columns.Add("InvoiceDate", typeof(DateTime));
                        }
                        dt.Columns.Add("DueDate", typeof(DateTime));
                        dt.Columns.Add("GrossAmount", typeof(double));
                        dt.Columns.Add("AgentCommPercentage", typeof(double));
                        dt.Columns.Add("AgentCommission", typeof(double));
                        dt.Columns.Add("RemainingBalance", typeof(double));
                        dt.Columns.Add("AgentCommissionRemainingBalance", typeof(double));
                        dt.Columns.Add("NetAmount", typeof(double));
                        dt.Columns.Add("AmountDue", typeof(double));
                        dt.Columns.Add("AmountFinanced", typeof(double));
                        dt.Columns.Add("AmountPaid", typeof(double));
                        dt.Columns.Add("EndorsementNumber", typeof(int));
                        var paymentTotalsByInvoice = table4Final.AsEnumerable().Where(r => r.Field<string>("LineItemDescription")?.Trim()
                                    .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true && r.Field<bool>("IsHouseCommission") == false)
                            .GroupBy(r => r.Field<long>("Invoice_Id"))
                            .ToDictionary(
                                g => g.Key,
                                g => g.Sum(x => Math.Abs(x.Field<double?>("NetAmount") ?? 0))
                            );


                        //var invoiceSummary = table4Final.AsEnumerable()
                        //    .Where(r =>
                        //        !(r.Field<string>("LineItemDescription")?
                        //            .Trim()
                        //            .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
                        //    )
                        //    .GroupBy(r => new
                        //    {
                        //        PolicyNumber = r.Field<string>("PolicyNumber"),
                        //        TransactionType = r.Field<string>("TransactionType"),
                        //        Invoice_Id = r.Field<long>("Invoice_Id"),
                        //        IdNo = r.Field<string>("IdNo"),
                        //        IsHouseCommission = r.Field<bool>("IsHouseCommission"),
                        //        EffectiveDate = r.Field<DateTime>("EffectiveDate"),
                        //        ExpirationDate = r.Field<DateTime>("ExpirationDate"),
                        //        DueDate = r.Field<DateTime>("DueDate"),
                        //        AmountFinanced = r.Field<double?>("AmountFinanced") ?? 0,
                        //        AmountPaid = r.Field<double?>("AmountPaid") ?? 0
                        //    })
                        //    .Select(g => new
                        //    {
                        //        g.Key.PolicyNumber,
                        //        g.Key.TransactionType,
                        //        g.Key.Invoice_Id,
                        //        g.Key.IdNo,
                        //        g.Key.IsHouseCommission,
                        //        g.Key.ExpirationDate,
                        //        g.Key.EffectiveDate,
                        //        g.Key.DueDate,
                        //        g.Key.AmountFinanced,
                        //        g.Key.AmountPaid,
                        //        GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
                        //        AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
                        //        AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
                        //        RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
                        //        AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
                        //        NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
                        //        AmountDue = g.Sum(x => (x.Field<double?>("GrossAmount") ?? 0) - (x.Field<double?>("AgentCommission") ?? 0)),                               
                        //        //AmountPaid = g.Sum(x => x.Field<double?>("AmountPaid") ?? 0)


                        //    });
                        var invoiceSummary = table4Final.AsEnumerable()
                            .Where(r =>
                                !(r.Field<string>("LineItemDescription")?
                                    .Trim()
                                    .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
                            )
                            .GroupBy(r => new
                            {
                                PolicyNumber = r.Field<string>("PolicyNumber"),
                                TransactionType = r.Field<string>("TransactionType"),
                                Invoice_Id = r.Field<long>("Invoice_Id"),
                                IdNo = r.Field<string>("IdNo"),
                                IsHouseCommission = r.Field<bool>("IsHouseCommission"),
                                EffectiveDate = r.Field<DateTime>("EffectiveDate").Date,
                                InvoiceDate = r.Field<DateTime>("InvoiceDate").Date,
                                ExpirationDate = r.Field<DateTime>("ExpirationDate").Date,
                                DueDate = r.Field<DateTime>("DueDate").Date
                            })
                            .Select(g => new
                            {
                                g.Key.PolicyNumber,
                                g.Key.TransactionType,
                                g.Key.Invoice_Id,
                                g.Key.IdNo,
                                g.Key.IsHouseCommission,
                                g.Key.ExpirationDate,
                                g.Key.EffectiveDate,
                                g.Key.InvoiceDate,
                                g.Key.DueDate,

                                AmountFinanced = g.Max(x => x.Field<double?>("AmountFinanced") ?? 0),
                                AmountPaid = g.Max(x => x.Field<double?>("AmountPaid") ?? 0),
                                EndorsementNumber = g.Max(x => x.Field<int?>("EndorsementNumber") ?? 0),
                                GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
                                AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
                                AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
                                RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
                                AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
                                NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
                                AmountDue = g.Sum(x =>
                                    (x.Field<double?>("GrossAmount") ?? 0) -
                                    (x.Field<double?>("AgentCommission") ?? 0))
                            });

                        foreach (var row in invoiceSummary)
                        {
                            //double amountPaid =
                            //    paymentTotalsByInvoice.TryGetValue(row.Invoice_Id, out var paid)
                            //        ? paid
                            //        : 0;

                            if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
                            {
                                dt.Rows.Add(
                                row.PolicyNumber,
                                row.TransactionType,
                                row.Invoice_Id,
                                row.IdNo,
                                row.IsHouseCommission,
                                row.ExpirationDate,
                                row.EffectiveDate,
                                row.InvoiceDate,
                                row.DueDate,
                                row.GrossAmount,
                                row.AgentCommPercentage,
                                row.AgentCommission,
                                row.RemainingBalance,
                                row.AgentCommissionRemainingBalance,
                                row.NetAmount,
                               row.AmountDue,
                               row.AmountFinanced,
                           // amountPaid
                           row.AmountPaid,
                           row.EndorsementNumber
                            );
                            }
                            else
                            {
                                dt.Rows.Add(
                                    row.PolicyNumber,
                                    row.TransactionType,
                                    row.Invoice_Id,
                                    row.IdNo,
                                    row.IsHouseCommission,
                                    row.ExpirationDate,
                                    row.EffectiveDate,
                                    row.DueDate,
                                    row.GrossAmount,
                                    row.AgentCommPercentage,
                                    row.AgentCommission,
                                    row.RemainingBalance,
                                    row.AgentCommissionRemainingBalance,
                                    row.NetAmount,
                                   row.AmountDue,
                                   row.AmountFinanced,
                               // amountPaid
                               row.AmountPaid,
                               row.EndorsementNumber
                                );
                            }


                        }

                        table4Final = dt;
                    }
                    table4Final = _IMSOfficeFileOptions.BalanceDueInAgencyBill(IsOutstandingInvoices, table4Final);

                    if (table4Final.Rows.Count > 0)
                    {
                        List<ReportOutputTables> agencyOutputTables = new()
                        {
                            new() { DataTable = agencyTableFinal },
                            new() { DataTable = policiesTableFinal },
                            new() { DataTable = invoicesTableFinal },
                            new() { DataTable = table4Final }
                        };

                        var policyTable = agencyOutputTables?[3]?.DataTable;
                        var policyAgentTable = agencyOutputTables?[2]?.DataTable;
                        var agentTable = agencyOutputTables?[0]?.DataTable;
                        var distinctPolicyNumbers = policyTable != null && policyTable.Columns.Contains("PolicyNumber") ? policyTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")).Select(r => r.Field<string>("PolicyNumber")).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList() : new List<string>();
                        var distinctAgents = policyAgentTable != null && policyAgentTable.Columns.Contains("PolicyNumber") && policyAgentTable.Columns.Contains("AgentDetail_Id") ? policyAgentTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")
                                                && distinctPolicyNumbers.Contains(r.Field<string>("PolicyNumber"))
                                                && !r.IsNull("AgentDetail_Id")).Select(r => Convert.ToInt64(r["AgentDetail_Id"])).Distinct().ToList() : new List<long>();
                        var filteredAgentsTable = agentTable?.Clone() ?? new DataTable();
                        if (agentTable != null && agentTable.Columns.Contains("AgentDetail_Id"))
                        {
                            var agentIds = distinctAgents.ToHashSet();
                            var rows = agentTable.AsEnumerable().Where(r => !r.IsNull("AgentDetail_Id") && agentIds.Contains(Convert.ToInt64(r["AgentDetail_Id"])));
                            if (rows.Any())
                            {
                                filteredAgentsTable = rows.CopyToDataTable();
                                agencyOutputTables[0].DataTable = filteredAgentsTable;
                            }
                        }

                        if (agencyOutputTables.Count > 3)
                        {
                            var invoiceDetails = agencyOutputTables[3].DataTable;
                            totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("NetAmount")) ?? 0;
                        }
                        var viewDataDictionary = new ViewDataDictionary
                                               (
                                       new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                        {
                            Model = agencyOutputTables,
                        };

                        viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
                        //ViewBag.IsSimplifiedView = IsSimplifiedView;
                        viewDataDictionary["ReportsInvoiceDueDate"] = ReportsInvoiceDueDate;
                        //ViewBag.ReportsInvoiceDueDate = ReportsInvoiceDueDate;
                        DynamicReportHTMLContent = this.view.Render("Reports/AgencyDetailedAccountStatementDynamicReport", viewDataDictionary);

                        if (!IsSimplifiedView)
                        {

                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "Amount Due", "Amount Paid", "NET Due" });
                        }
                        else
                        {
                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "NET" });
                            DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                            RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                                                                               .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                                                                               .Replace("{{clientAddress}}", Clients?.Address ?? "")
                                                                               .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                                                                               .Replace("{{clientLogo}}", ClientLogo)
                                                                               //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ServerDateFormat))
                                                                               .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                                                                               .Replace("{{dynamicAgentloopingLogic}}", DynamicReportHTMLContent)
                                                                               .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString())
                                                                               ;

                    }
                }
                else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.UnderwriterAccountStatement)
                {
                    double totalNetAmount = 0;
                    var tableAgency = reportOutputTablesList[0];
                    var tablePolicies = reportOutputTablesList[1].DataTable;
                    var table3 = reportOutputTablesList[2].DataTable;
                    var table4 = reportOutputTablesList[3].DataTable;

                    // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

                    var agencyTableFinal = tableAgency.DataTable.Clone();
                    var policiesTableFinal = tablePolicies.Clone();
                    var invoicesTableFinal = table3.Clone();
                    var table4Final = table4.Clone();
                    var finalInvoiceTable = table4.Clone();
                    if (IsOutstandingInvoices)
                    {
                        var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("UnderWriterDetail_Id"));


                        foreach (var agencyGroup in agencyGroups)
                        {
                            long agentDetailId = agencyGroup.Key;

                            var agencyTableTemp = tableAgency.DataTable.Clone();
                            foreach (var row in agencyGroup)
                                agencyTableTemp.ImportRow(row);

                            var policiesForAgent = tablePolicies.AsEnumerable()
                                .Where(r => !r.IsNull("UnderWriterDetail_Id") && r.Field<long>("UnderWriterDetail_Id") == agentDetailId);

                            var policiesTableTemp = tablePolicies.Clone();
                            foreach (var row in policiesForAgent)
                                policiesTableTemp.ImportRow(row);

                            var policyIds = policiesForAgent
                                .Where(r => !r.IsNull("Id"))
                                .Select(r => r.Field<long>("Id"))
                                .ToHashSet();

                            var invoicesForAgent = table3.AsEnumerable()
                                .Where(r => !r.IsNull("UnderWriterDetail_Id") && r.Field<long>("UnderWriterDetail_Id") == agentDetailId);

                            var invoicesTableTemp = table3.Clone();
                            foreach (var row in invoicesForAgent)
                                invoicesTableTemp.ImportRow(row);

                            var filteredTable4Rows = table4.AsEnumerable()
                                .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                            var table4Temp = table4.Clone();
                            foreach (var row in filteredTable4Rows)
                                table4Temp.ImportRow(row);

                            //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
                            var policiesToRemove = table4Temp.AsEnumerable()
                                .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
                                .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
                                .Select(g => g.Key)
                                .ToHashSet();

                            if (policiesToRemove.Any())
                            {
                                foreach (var row in policiesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        policiesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        invoicesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in table4Temp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        table4Temp.Rows.Remove(row);
                                }
                            }

                            if (table4Temp.Rows.Count > 0)
                            {
                                foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                                foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                                foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                                foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                            }
                        }

                        foreach (DataRow row in table4Final.Rows)
                        {
                            finalInvoiceTable.ImportRow(row);
                        }
                        var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
                            .Select(r => r.Field<long>("UnderWriterDetail_Id")).Distinct().ToList();

                        var filteredAgencies = tableAgency.DataTable.AsEnumerable()
                            .Where(a => validAgencyIds.Contains(a.Field<long>("UnderWriterDetail_Id")));

                        if (filteredAgencies.Any())
                            tableAgency.DataTable = filteredAgencies.CopyToDataTable();
                        else
                            tableAgency.DataTable = tableAgency.DataTable.Clone();

                    }
                    else
                    {
                        finalInvoiceTable = table4;

                    }

                    if (IsSimplifiedView)
                    {
                        var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

                        if (filteredRows.Any())
                            finalInvoiceTable = filteredRows.CopyToDataTable();
                    }
                    var commPaidRows = finalInvoiceTable.AsEnumerable()
                    .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
                    IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

                    var groupedRows = finalInvoiceTable.AsEnumerable()
                            .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
                            .GroupBy(r => new
                            {
                                PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
                                InvoiceId = r.Field<long>("Invoice_Id")
                            })
                            .Select(g => g.First());

                    DataTable tempGroupedRows;
                    DataTable result;

                    // if (groupedRows != null && groupedRows.Any())
                    // {
                    finalDistinct = groupedRows.Concat(commPaidRows);
                    if (finalDistinct != null && finalDistinct.Any())
                    {
                        result = finalDistinct.CopyToDataTable();

                        reportOutputTablesList[0].DataTable = tableAgency.DataTable;
                        reportOutputTablesList[3].DataTable = result;

                        if (reportOutputTablesList.Count > 3)
                        {
                            var invoiceDetails = result;
                            totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
                        }

                        var viewDataDictionary = new ViewDataDictionary
                                               (
                                       new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                        {
                            Model = reportOutputTablesList,
                        };
                        viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
                        //ViewBag.IsSimplifiedView = IsSimplifiedView;
                        DynamicReportHTMLContent = this.view.Render("Reports/UnderWriterAccountStatementDynamicReport", viewDataDictionary);
                        if (!IsSimplifiedView)
                        {
                            // Remove Comm. Paid to Date, Comm. Due columns + simplified-block
                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "Comm. Paid to Date", "Comm. Due" });
                        }
                        else
                        {
                            DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                            RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                            .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                           .Replace("{{clientLogo}}", ClientLogo)
                           //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
                    }
                    //}
                }
                else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectInsuredBilledAccountStatement)
                {
                    double totalNetAmount = 0;
                    var tableAgency = reportOutputTablesList[0];
                    var tablePolicies = reportOutputTablesList[1].DataTable;
                    var table3 = reportOutputTablesList[2].DataTable;
                    var table4 = reportOutputTablesList[3].DataTable;

                    // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

                    var agencyTableFinal = tableAgency.DataTable.Clone();
                    var policiesTableFinal = tablePolicies.Clone();
                    var invoicesTableFinal = table3.Clone();
                    var table4Final = table4.Clone();
                    var finalInvoiceTable = table4.Clone();
                    if (IsOutstandingInvoices)
                    {
                        var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));


                        foreach (var agencyGroup in agencyGroups)
                        {
                            long agentDetailId = agencyGroup.Key;

                            var agencyTableTemp = tableAgency.DataTable.Clone();
                            foreach (var row in agencyGroup)
                                agencyTableTemp.ImportRow(row);

                            var policiesForAgent = tablePolicies.AsEnumerable()
                                .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                            var policiesTableTemp = tablePolicies.Clone();
                            foreach (var row in policiesForAgent)
                                policiesTableTemp.ImportRow(row);

                            var policyIds = policiesForAgent
                                .Where(r => !r.IsNull("Id"))
                                .Select(r => r.Field<long>("Id"))
                                .ToHashSet();

                            var invoicesForAgent = table3.AsEnumerable()
                                .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

                            var invoicesTableTemp = table3.Clone();
                            foreach (var row in invoicesForAgent)
                                invoicesTableTemp.ImportRow(row);

                            var filteredTable4Rows = table4.AsEnumerable()
                                .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

                            var table4Temp = table4.Clone();
                            foreach (var row in filteredTable4Rows)
                                table4Temp.ImportRow(row);

                            //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
                            var policiesToRemove = table4Temp.AsEnumerable()
                                .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
                                .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
                                .Select(g => g.Key)
                                .ToHashSet();

                            if (policiesToRemove.Any())
                            {
                                foreach (var row in policiesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        policiesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        invoicesTableTemp.Rows.Remove(row);
                                }

                                foreach (var row in table4Temp.AsEnumerable().ToList())
                                {
                                    if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                                        table4Temp.Rows.Remove(row);
                                }
                            }

                            if (table4Temp.Rows.Count > 0)
                            {
                                foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
                                foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
                                foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
                                foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
                            }
                        }

                        foreach (DataRow row in table4Final.Rows)
                        {
                            finalInvoiceTable.ImportRow(row);
                        }
                        var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
                            .Select(r => r.Field<long>("AgentDetail_Id")).Distinct().ToList();

                        var filteredAgencies = tableAgency.DataTable.AsEnumerable()
                            .Where(a => validAgencyIds.Contains(a.Field<long>("AgentDetail_Id")));

                        if (filteredAgencies.Any())
                            tableAgency.DataTable = filteredAgencies.CopyToDataTable();
                        else
                            tableAgency.DataTable = tableAgency.DataTable.Clone();

                    }
                    else
                    {
                        finalInvoiceTable = table4;

                    }

                    if (IsSimplifiedView)
                    {
                        var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

                        if (filteredRows.Any())
                            finalInvoiceTable = filteredRows.CopyToDataTable();
                    }
                    var commPaidRows = finalInvoiceTable.AsEnumerable()
                    .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
                    IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

                    var groupedRows = finalInvoiceTable.AsEnumerable()
                            .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
                            .GroupBy(r => new
                            {
                                PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
                                InvoiceId = r.Field<long>("Invoice_Id")
                            })
                            .Select(g => g.First());

                    DataTable tempGroupedRows;
                    DataTable result;

                    // if (groupedRows != null && groupedRows.Any())
                    // {
                    finalDistinct = groupedRows.Concat(commPaidRows);
                    if (finalDistinct != null && finalDistinct.Any())
                    {
                        result = finalDistinct.CopyToDataTable();

                        reportOutputTablesList[0].DataTable = tableAgency.DataTable;
                        reportOutputTablesList[3].DataTable = result;

                        if (reportOutputTablesList.Count > 3)
                        {
                            var invoiceDetails = result;
                            totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
                        }

                        var viewDataDictionary = new ViewDataDictionary
                                               (
                                       new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                        {
                            Model = reportOutputTablesList,
                        };
                        viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
                        //ViewBag.IsSimplifiedView = IsSimplifiedView;
                        DynamicReportHTMLContent = this.view.Render("Reports/DirectInsuredBilledAccountStatementDynamicReport", viewDataDictionary);
                        if (!IsSimplifiedView)
                        {
                            // Remove Comm. Paid to Date, Comm. Due columns + simplified-block
                            DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
                                new[] { "Comm. Paid to Date", "Comm. Due" });
                        }
                        else
                        {
                            DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
                            RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                        FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
                            .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
                           .Replace("{{clientLogo}}", ClientLogo)
                           //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
                           .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
                    }
                    //}
                }
                else
                {
                    FinalHTMLAlongWithTemplate = "";
                }
                
            }
            catch (Exception ex)
            {
                FinalHTMLAlongWithTemplate = "";
            }
            return FinalHTMLAlongWithTemplate;
        }
       


        public DocumentFileInfo Download(byte FileType_Id, long Client_Id, List<ReportOutputTables> reportOutputTables, long Report_Id,string ActionType, LU_ReportTemplates ReportTemplate = null, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null, bool IsAgentId_Include = false)
        {
            DataSet ds = new DataSet();
            DataSet dsAgency = new DataSet();
            byte[]? content = null;
            var FileTypesList = LookupUtility.GetFileTypes();
            LU_FileTypes FileType = FileTypesList?.Where(c => c.FileType_Id == FileType_Id)?.FirstOrDefault();
            DocumentFileInfo documentFileInfo = new DocumentFileInfo();
            List<DocumentFileInfo> listDocumentFileInfo = new List<DocumentFileInfo>();
            if (FileType != null)
            {
                documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.{FileType.Extension}";
                documentFileInfo.FileType_Id = FileType.FileType_Id;
                int counter = 0;
                foreach (var reportOutputTable in reportOutputTables)
                {
                    counter++;
                    string tableName = reportOutputTable.Name;
                    int count = reportOutputTables.Where(c => c.OutputDataTable.TableName == tableName)?.Count() ?? 0;
                    if (count > 0)
                    {
                        tableName += $"_{counter}";
                    }
                    reportOutputTable.OutputDataTable.TableName = tableName;
                    ds.Tables.Add(reportOutputTable.OutputDataTable.Copy());
                }

                DataSet exportDs = ds;
                if (Report_Id == MasterLookup._Reports.InvoiceProduction)
                {
                    exportDs = ds.Copy();
                    var referralSetting = LookupUtility.GetClientSettings(Client_Id,MasterLookup._ClientSettings.ShowReferralSourceColumn);
                    bool showReferral = bool.TryParse(referralSetting, out bool referralEnabled) && referralEnabled;
                    var additionalSetting = LookupUtility.GetClientSettings(Client_Id,MasterLookup._ClientSettings.EnableAdditionalInvoiceProductionColumns);
                    bool showAdditional = bool.TryParse(additionalSetting, out bool additionalEnabled) && additionalEnabled;
                    string[] additionalColumns = {"Surplus Lines Tax","Stamping Fee","Issuing Company/Billing Company","Invoice Created/Invoice Booked"};
                    foreach (DataTable table in exportDs.Tables)
                    {
                        if (!showReferral && table.Columns.Contains("Referral Source"))
                        {
                            table.Columns.Remove("Referral Source");
                        }
                        if (!showAdditional)
                        {
                            foreach (string column in additionalColumns)
                            {
                                if (table.Columns.Contains(column))
                                {
                                    table.Columns.Remove(column);
                                }
                            }
                        }
                    }
                }
                if (FileType.FileType_Id == MasterLookup._LUFileTypes.Excel)
                {
                    if (Report_Id == MasterLookup._Reports.DirectBill || Report_Id == MasterLookup._Reports.InsuredBill || Report_Id == MasterLookup._Reports.AgencyDetailed || Report_Id == MasterLookup._Reports.DirectInsuredBill)
                    {
                        content = _IMSOfficeFileOptions.DownloadDirectBillExcel(ds, AsOfDate);
                        documentFileInfo.Bytes = content;
                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                    else if(Report_Id==MasterLookup._Reports.Underwriter)
                    {
                        content = _IMSOfficeFileOptions.DownloadUnderwriterExcel(ds, AsOfDate);
                        documentFileInfo.Bytes = content;
                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                    else if (Report_Id == MasterLookup._Reports.AgencyBill)
                    {
                        content = _IMSOfficeFileOptions.DownloadSimplifiedAgencyBillExcel(ds, IsOutstandingInvoices, Client_Id, AsOfDate);
                        documentFileInfo.Bytes = content;
                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                    else
                    {
                        content = _IMSOfficeFileOptions.DownloadExcel(exportDs, reportOutputTables);
                        documentFileInfo.Bytes = content;
                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                }
                else if (FileType.FileType_Id == MasterLookup._LUFileTypes.CSV)
                {
                    List<DocumentFileInfo> documentFileInfoList = _ICSVOptions.DownloadCSV(exportDs);
                    if (documentFileInfoList?.Count == 1)
                    {
                        content = documentFileInfoList.First().Bytes;
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                    else
                    {
                        content = _IZip.Compress(documentFileInfoList);
                        documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.zip";
                        documentFileInfo.ContentType = "application/zip";
                    }
                    documentFileInfo.Bytes = content;
                    documentFileInfo.Base64String = Convert.ToBase64String(content);
                }
                else if (FileType.FileType_Id == MasterLookup._LUFileTypes.PDF ||
                    FileType.FileType_Id == MasterLookup._LUFileTypes.AgentSpecificPDF)
                {
                    if (ds?.Tables.Count > 0)
                    {
                        if (ReportTemplate != null)
                        {
                            try
                            {
                                if (FileType.FileType_Id == MasterLookup._LUFileTypes.AgentSpecificPDF)
                                {
                                    if (ActionType == "Email")
                                    {
                                        var agentFiles = GenerateAgentStatementPDFs(Client_Id, reportOutputTables, Report_Id, ReportTemplate, IsOutstandingInvoices,
                                                        IsSimplifiedView, AsOfDate, IsAgentId_Include);

                                        if (agentFiles != null && agentFiles.Count > 0)
                                        {
                                            foreach (var file in agentFiles)
                                            {
                                                listDocumentFileInfo.Add(new DocumentFileInfo
                                                {
                                                    Name = file.FileName,
                                                    Base64String = Convert.ToBase64String(file.FileBytes),
                                                    Bytes = file.FileBytes,
                                                    ContentType = "application/pdf"
                                                });
                                            }

                                            content = _IZip.Compress(listDocumentFileInfo);
                                            documentFileInfo.Name = $"Report-{ReportTemplate.Name}-{DateTime.Now.Ticks}.zip";
                                            documentFileInfo.Bytes = content;
                                            documentFileInfo.Base64String = Convert.ToBase64String(content);
                                            documentFileInfo.ContentType = "application/zip";
                                        }
                                    }
                                    else
                                    {
                                        var tableAgency = reportOutputTables[0];

                                        var tablePolicies = reportOutputTables[1];

                                        var tableInvoices = reportOutputTables[2];

                                        var table4 = reportOutputTables[3];
                                        bool hasAgentDetailId = tableAgency.DataTable.Columns.Contains("AgentDetail_Id");
                                        bool hasUnderwriterDetailId = tableAgency.DataTable.Columns.Contains("UnderwriterDetail_Id");

                                        //var agencyGroups = tableAgency.DataTable.AsEnumerable().GroupBy(r => r.Field<long>("AgentDetail_Id"));
                                        var agencyGroups = tableAgency.DataTable.AsEnumerable().GroupBy(r => new { AgentDetailId = hasAgentDetailId && !r.IsNull("AgentDetail_Id") ? r.Field<long>("AgentDetail_Id")
                                            : (long?)null, UnderwriterDetailId = hasUnderwriterDetailId && !r.IsNull("UnderwriterDetail_Id") ? r.Field<long>("UnderwriterDetail_Id") : (long?)null});

                                        List<(string fileName, byte[] fileBytes)> pdfFiles = new List<(string, byte[])>();

                                        foreach (var agencyGroup in agencyGroups)
                                        {

                                            //long agentDetailId = agencyGroup.Key;
                                            long? agentDetailId = agencyGroup.Key.AgentDetailId;
                                            long? underwriterDetailId = agencyGroup.Key.UnderwriterDetailId;

                                            //var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
                                            //    .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId)
                                            //    .ToList();
                                            var policiesForAgent = tablePolicies.DataTable.AsEnumerable().Where(r =>(agentDetailId.HasValue && !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId.Value)
                                                                   || (underwriterDetailId.HasValue && !r.IsNull("UnderwriterDetail_Id") && r.Field<long>("UnderwriterDetail_Id") == underwriterDetailId.Value)).ToList();

                                            if (!policiesForAgent.Any())
                                                continue;

                                            var validPolicyIds = table4.DataTable.AsEnumerable()
                                                .Where(r => !r.IsNull("PolicyDetails_Id"))
                                                .Select(r => r.Field<long>("PolicyDetails_Id"))
                                                .ToHashSet();

                                            var filteredPolicies = policiesForAgent
                                                .Where(r => validPolicyIds.Contains(r.Field<long>("Id")))
                                                .ToList();

                                            if (!filteredPolicies.Any())
                                                continue;

                                            var policiesTableClone = tablePolicies.DataTable.Clone();
                                            foreach (var row in filteredPolicies)
                                                policiesTableClone.ImportRow(row);

                                            var policyNos = filteredPolicies
                                                .Select(r => r.Field<string>("PolicyNumber"))
                                                .ToHashSet();

                                            var policyIds = filteredPolicies
                                                .Select(r => r.Field<long>("Id"))
                                                .ToHashSet();

                                            //var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                                            //    .Where(r => !r.IsNull("AgentDetail_Id") &&
                                            //                r.Field<long>("AgentDetail_Id") == agentDetailId &&
                                            //                !r.IsNull("PolicyNumber") &&
                                            //                policyNos.Contains(r.Field<string>("PolicyNumber")))
                                            //    .ToList();
                                            var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                                                .Where(r =>((agentDetailId.HasValue && !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId.Value)
                                                         || (underwriterDetailId.HasValue && !r.IsNull("UnderwriterDetail_Id") && r.Field<long>("UnderwriterDetail_Id") == underwriterDetailId.Value))
                                                         && !r.IsNull("PolicyNumber") && policyNos.Contains(r.Field<string>("PolicyNumber"))).ToList();

                                            if (!invoicesForAgent.Any())
                                                continue;

                                            var invoicesTableClone = tableInvoices.DataTable.Clone();
                                            foreach (var row in invoicesForAgent)
                                                invoicesTableClone.ImportRow(row);

                                            var filteredTable4Rows = table4.DataTable.AsEnumerable()
                                                .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")))
                                                .ToList();

                                            if (!filteredTable4Rows.Any())
                                                continue;

                                            var table4Clone = table4.DataTable.Clone();
                                            foreach (var row in filteredTable4Rows)
                                                table4Clone.ImportRow(row);

                                            var agencyTableClone = tableAgency.DataTable.Clone();
                                            foreach (var row in agencyGroup)
                                                agencyTableClone.ImportRow(row);

                                            var agencyOutputTables = new List<ReportOutputTables>
                                        {
                                            new ReportOutputTables { DataTable = agencyTableClone },
                                            new ReportOutputTables { DataTable = policiesTableClone },
                                            new ReportOutputTables { DataTable = invoicesTableClone },
                                            new ReportOutputTables { DataTable = table4Clone }
                                        };

                                            if (table4Clone.Rows.Count > 0)
                                            {
                                                // Build HTML
                                                var Clients = _accountRepository.GetClients(Client_Id);
                                                string htmlContent = BuildReportHtmlFromTemplate(Report_Id, Clients.Client_Id, agencyOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
                                                if (htmlContent != "")
                                                {
                                                    string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                                                    string year = DateTime.Now.Year.ToString();

                                                    string headerHtml = $@"<table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;"">
                                    <tbody><tr><td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt;"">{month} {year}</td>
                                    <td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt;"">{ReportTemplate.Name}</td></tr></tbody></table>
                                    <style>body {{margin: 0px;padding: 0px;}}</style>";

                                                    string footerHtml = @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>";

                                                    var request = new PDFRequest
                                                    {
                                                        BodyHTMLString = htmlContent,
                                                        HeaderHTMLString = headerHtml,
                                                        FooterHTMLString = footerHtml,
                                                        IsCoverPageExcluded = false
                                                    };

                                                    string serviceUrl = MasterLookup._PDFCoverterAPIURL;
                                                    ServiceLogInfo logInfo = new ServiceLogInfo();
                                                    logInfo = new ServiceLogInfo()
                                                    {
                                                        Service_Id = MasterLookup._Services.EIConnect,
                                                        ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.PDFCoverterAPI,
                                                        Client_Id = Client_Id
                                                    };
                                                    ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(logInfo, serviceUrl, request, typeof(ServiceResponse), true);
                                                    _ServiceLogInfoRepository.InsertServiceLogInfo(logInfo);
                                                    if (response.Status && response.Data != null)
                                                    {
                                                        byte[] pdfBytes = Convert.FromBase64String(response.Data.ToString());
                                                        //string agencyName = agencyGroup.First().Field<string>("Name")?.Replace(" ", "_") ?? $"Agency_{agentDetailId}";
                                                        string agencyName = agencyGroup.First().Field<string>("Name")?.Replace(" ", "_") ?? $"Agency_{agentDetailId ?? underwriterDetailId}";
                                                        string referenceId = "";

                                                        //var Agent_ID = _policyDetailsRepository.GetAgentDetails(agentDetailId).AgentReference_Id;
                                                        string Agent_ID = "";
                                                        if (agentDetailId.HasValue)
                                                        {
                                                            Agent_ID = _policyDetailsRepository.GetAgentDetails(agentDetailId.Value).AgentReference_Id;
                                                        }
                                                        else if (underwriterDetailId.HasValue)
                                                        {
                                                            referenceId = underwriterDetailId.Value.ToString();
                                                        }
                                                        //string fileName = $"{agencyName}_{ReportTemplate.Name}_{DateTime.Now:yyyyMMddHHmmss}_{Agent_ID}.pdf";                                                  
                                                        var datePart = (AsOfDate ?? DateTime.Now).ToString("yyyyMMdd");
                                                        string fileName = "";
                                                        if (IsAgentId_Include && !string.IsNullOrEmpty(referenceId))
                                                        {
                                                            fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}_{Agent_ID}.pdf";
                                                        }
                                                        else
                                                        {
                                                            fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}.pdf";
                                                        }
                                                        pdfFiles.Add((fileName, pdfBytes));
                                                    }
                                                }
                                            }
                                        }

                                        if (pdfFiles?.Count == 1)
                                        {
                                            foreach (var file in pdfFiles)
                                            {
                                                listDocumentFileInfo.Add(new DocumentFileInfo
                                                {
                                                    Name = file.fileName,
                                                    Base64String = file.fileBytes.ToString(),
                                                    Bytes = file.fileBytes,
                                                    ContentType = "application/pdf"

                                                });
                                            }

                                            content = _IZip.Compress(listDocumentFileInfo);
                                            documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.zip";
                                            documentFileInfo.Bytes = content;
                                            documentFileInfo.Base64String = Convert.ToBase64String(content);
                                            documentFileInfo.ContentType = "application/zip";
                                        }
                                        else
                                        {
                                            foreach (var file in pdfFiles)
                                            {
                                                listDocumentFileInfo.Add(new DocumentFileInfo
                                                {
                                                    Name = file.fileName,
                                                    Base64String = file.fileBytes.ToString(),
                                                    Bytes = file.fileBytes,
                                                    ContentType = "application/pdf"

                                                });
                                            }

                                            content = _IZip.Compress(listDocumentFileInfo);
                                            documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.zip";
                                            documentFileInfo.Bytes = content;
                                            documentFileInfo.Base64String = Convert.ToBase64String(content);
                                            documentFileInfo.ContentType = "application/zip";
                                        }
                                    }
                                }
                                else
                                {
                                    //Clients clients = _accountRepository.GetClients(ReportTemplate.Client_Id);
                                    var HtmlContent = BuildReportHtmlFromTemplate(Report_Id, Client_Id, reportOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
                                    string Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                                    string Year = DateTime.Now.Year.ToString();
                                    string HeaderHTMLString = @" <table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;""><tbody><tr><td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt; "">" + Month + @" " + Year + @" </td><td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt; "">" + ReportTemplate.Name + "</td></tr></tbody></table><style>body {margin: 0px;padding: 0px;}</style>";

                                    //pDFConverterController.SetFooterForHtmlConverstion(PDFConverter.Document, 30, @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>", null);
                                    string FooterHTMLString = @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>";

                                    //byte[] PDFBytes = pDFConverterController.ConvertHtmlToPDF(PDFConverter, RenderedHtml);
                                    bool IsCoverPageExcluded = false;
                                    PDFRequest request = new PDFRequest()
                                    {
                                        BodyHTMLString = HtmlContent,
                                        HeaderHTMLString = HeaderHTMLString,
                                        FooterHTMLString = FooterHTMLString,
                                        IsCoverPageExcluded = IsCoverPageExcluded
                                    };
                                    string Service_Url = MasterLookup._PDFCoverterAPIURL;
                                    ServiceLogInfo serviceLogInfo = new ServiceLogInfo();
                                    serviceLogInfo = new ServiceLogInfo()
                                    {
                                        Service_Id = MasterLookup._Services.EIConnect,
                                        ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.PDFCoverterAPI,
                                        Client_Id = Client_Id
                                    };
                                    ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, request, typeof(ServiceResponse), true);
                                    _ServiceLogInfoRepository.InsertServiceLogInfo(serviceLogInfo);
                                    if (response.Status)
                                    {
                                        documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.pdf";
                                        documentFileInfo.Base64String = response.Data.ToString();
                                        documentFileInfo.ContentType = "application/pdf";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            List<DocumentFileInfo> documentFileInfoList = new List<DocumentFileInfo>();
                            foreach (DataTable table in ds?.Tables)
                            {
                                var viewDataDictionary = new ViewDataDictionary
                                    (
                            new EmptyModelMetadataProvider(),
                                 new ModelStateDictionary()
                                    )
                                {
                                    Model = table,
                                };
                                viewDataDictionary.Add("FontSize", 6);
                                var htmlContent = this.view.Render("Reports/ReportPDF", viewDataDictionary);
                                List<string> CSSReferenceList = new List<string> { "css/Reports/ReportPDF.css" };
                                byte[] bytes = _IPDFOptions.DownloadPDF(htmlContent, _IWebHostEnvironment.WebRootPath, CSSReferenceList);
                                if (bytes != null && bytes.Length > 0)
                                {
                                    DocumentFileInfo fileInfo = new DocumentFileInfo();
                                    fileInfo.Bytes = bytes;
                                    fileInfo.Name = $"{table.TableName}.pdf";
                                    documentFileInfoList.Add(fileInfo);
                                }
                                //content = _IPDFOptions.DownloadPDF(htmlContent, _IWebHostEnvironment.WebRootPath, CSSReferenceList);
                            }
                            if (documentFileInfoList?.Count == 1)
                            {
                                content = documentFileInfoList.First().Bytes;
                                documentFileInfo.ContentType = FileType.ContentType;
                            }
                            else
                            {
                                content = _IZip.Compress(documentFileInfoList);
                                documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.zip";
                                documentFileInfo.ContentType = "application/zip";
                            }
                            documentFileInfo.Bytes = content;
                            documentFileInfo.Base64String = Convert.ToBase64String(content);
                        }

                    }
                }
            }

            return documentFileInfo;
        }


        public static string RemoveSimplifiedColumns(string html, string[] columnHeadersToRemove)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables == null) return html;

            foreach (var table in tables)
            {
                var headerRow = table.SelectSingleNode(".//tr");
                if (headerRow == null) continue;

                var headers = headerRow.SelectNodes("./th|./td").ToList();
                try
                {

                    var indexesToRemove = headers
                        .Select((th, idx) => new { th, idx })
                        .Where(x => columnHeadersToRemove.Contains(x.th.InnerText.Trim()))
                        .Select(x => x.idx)
                        .OrderByDescending(i => i)
                        .ToList();

                    foreach (var row in table.SelectNodes(".//tr"))
                    {
                        var cells = row.SelectNodes("./th|./td");
                        if (cells == null) continue;

                        foreach (var colIndex in indexesToRemove)
                        {
                            if (colIndex < cells.Count)
                                cells[colIndex].Remove();
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
        public List<AgentStatementFile> GenerateAgentStatementPDFs(long Client_Id, List<ReportOutputTables> reportOutputTables, long Report_Id, LU_ReportTemplates ReportTemplate,
                                                                  bool IsOutstandingInvoices, bool IsSimplifiedView, DateTime? AsOfDate, bool IsAgentId_Include = false)
        {
            var agentFiles = new List<AgentStatementFile>();
            if (reportOutputTables == null || reportOutputTables.Count < 4)
                return agentFiles;

            var tableAgency = reportOutputTables[0];
            var tablePolicies = reportOutputTables[1];
            var tableInvoices = reportOutputTables[2];
            var table4 = reportOutputTables[3];
            bool hasAgentDetailId = tableAgency.DataTable.Columns.Contains("AgentDetail_Id");
            bool hasUnderwriterDetailId = tableAgency.DataTable.Columns.Contains("UnderwriterDetail_Id");

            var agencyGroups = tableAgency.DataTable
                               .AsEnumerable()
                               .GroupBy(r => new
                               {
                                   AgentDetailId = hasAgentDetailId && !r.IsNull("AgentDetail_Id") ? r.Field<long>("AgentDetail_Id") : (long?)null,
                                   UnderwriterDetailId = hasUnderwriterDetailId && !r.IsNull("UnderwriterDetail_Id") ? r.Field<long>("UnderwriterDetail_Id") : (long?)null
                               });

            foreach (var agencyGroup in agencyGroups)
            {
                long? agentDetailId = agencyGroup.Key.AgentDetailId;
                long? underwriterDetailId = agencyGroup.Key.UnderwriterDetailId;

                // 1. Get policies for this Agent/Underwriter
                var policiesForAgent = tablePolicies.DataTable
                    .AsEnumerable()
                    .Where(r => (agentDetailId.HasValue && !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId.Value)
                                ||
                                (underwriterDetailId.HasValue && !r.IsNull("UnderwriterDetail_Id") && r.Field<long>("UnderwriterDetail_Id") == underwriterDetailId.Value))
                    .ToList();

                if (!policiesForAgent.Any())
                    continue;

                // 2. Get valid PolicyDetails_Id values from table4
                var validPolicyIds = table4.DataTable.AsEnumerable().Where(r => !r.IsNull("PolicyDetails_Id")).Select(r => r.Field<long>("PolicyDetails_Id")).ToHashSet();

                // 3. Filter policies
                var filteredPolicies = policiesForAgent.Where(r => !r.IsNull("Id") && validPolicyIds.Contains(r.Field<long>("Id"))).ToList();

                if (!filteredPolicies.Any())
                    continue;

                // 4. Clone policies table
                var policiesTableClone = tablePolicies.DataTable.Clone();
                foreach (var row in filteredPolicies)
                {
                    policiesTableClone.ImportRow(row);
                }

                // 5. Get policy numbers
                var policyNos = filteredPolicies.Where(r => !r.IsNull("PolicyNumber")).Select(r => r.Field<string>("PolicyNumber")).ToHashSet();

                // 6. Get policy IDs
                var policyIds = filteredPolicies.Where(r => !r.IsNull("Id")).Select(r => r.Field<long>("Id")).ToHashSet();

                // 7. Get invoices for this Agent/Underwriter
                var invoicesForAgent = tableInvoices.DataTable
                    .AsEnumerable()
                    .Where(r =>
                        (
                            (agentDetailId.HasValue &&
                             !r.IsNull("AgentDetail_Id") &&
                             r.Field<long>("AgentDetail_Id") == agentDetailId.Value)
                            ||
                            (underwriterDetailId.HasValue &&
                             !r.IsNull("UnderwriterDetail_Id") &&
                             r.Field<long>("UnderwriterDetail_Id") == underwriterDetailId.Value)
                        )
                        &&
                        !r.IsNull("PolicyNumber")
                        &&
                        policyNos.Contains(r.Field<string>("PolicyNumber"))
                    )
                    .ToList();

                if (!invoicesForAgent.Any())
                    continue;

                // 8. Clone invoice table
                var invoicesTableClone = tableInvoices.DataTable.Clone();
                foreach (var row in invoicesForAgent)
                {
                    invoicesTableClone.ImportRow(row);
                }

                // 9. Filter table4
                var filteredTable4Rows = table4.DataTable.AsEnumerable().Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id"))).ToList();
                if (!filteredTable4Rows.Any())
                    continue;

                // 10. Clone table4
                var table4Clone = table4.DataTable.Clone();

                foreach (var row in filteredTable4Rows)
                {
                    table4Clone.ImportRow(row);
                }

                // 11. Clone agency table
                var agencyTableClone = tableAgency.DataTable.Clone();
                foreach (var row in agencyGroup)
                {
                    agencyTableClone.ImportRow(row);
                }

                // 12. Build output tables for this Agent
                var agencyOutputTables = new List<ReportOutputTables>
                {
                    new ReportOutputTables
                    {
                        DataTable = agencyTableClone
                    },
                    new ReportOutputTables
                    {
                        DataTable = policiesTableClone
                    },
                    new ReportOutputTables
                    {
                        DataTable = invoicesTableClone
                    },
                    new ReportOutputTables
                    {
                        DataTable = table4Clone
                    }
                };

                if (table4Clone.Rows.Count == 0)
                    continue;

                // 13. Build HTML
                var Clients = _accountRepository.GetClients(Client_Id);
                string htmlContent = BuildReportHtmlFromTemplate(Report_Id,Clients.Client_Id,agencyOutputTables,ReportTemplate,IsOutstandingInvoices,IsSimplifiedView,AsOfDate);

                if (string.IsNullOrEmpty(htmlContent))
                    continue;

                // 14. Header
                string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                string year = DateTime.Now.Year.ToString();
                string headerHtml = $@"
                <table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;"">
                    <tbody>
                        <tr>
                            <td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt;"">
                                {month} {year}
                            </td>
                            <td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt;"">
                                {ReportTemplate.Name}
                            </td>
                        </tr>
                    </tbody>
                </table>
                <style>
                    body {{ margin: 0px; padding: 0px; }}
                </style>";

                // 15. Footer
                string footerHtml = @"
                <div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">
                    {CrtPage}
                </div>";

                // 16. PDF request
                var request = new PDFRequest
                {
                    BodyHTMLString = htmlContent,
                    HeaderHTMLString = headerHtml,
                    FooterHTMLString = footerHtml,
                    IsCoverPageExcluded = false
                };

                // 17. Generate PDF
                string serviceUrl = MasterLookup._PDFCoverterAPIURL;
                ServiceLogInfo logInfo = new ServiceLogInfo();
                logInfo = new ServiceLogInfo()
                {
                    Service_Id = MasterLookup._Services.EIConnect,
                    ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.PDFCoverterAPI,
                    Client_Id = Client_Id
                };
                ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(logInfo, serviceUrl, request, typeof(ServiceResponse), true);
                _ServiceLogInfoRepository.InsertServiceLogInfo(logInfo);
                if (!response.Status || response.Data == null)
                    continue;

                byte[] pdfBytes = Convert.FromBase64String(response.Data.ToString());

                // 18. Agency name
                string agencyName = agencyGroup.First().Field<string>("Name")?.Replace(" ", "_") ?? $"Agency_{agentDetailId ?? underwriterDetailId}";

                // 19. Reference ID
                string referenceId = "";
                string agentReferenceId = "";

                if (agentDetailId.HasValue)
                {
                    agentReferenceId = _policyDetailsRepository.GetAgentDetails(agentDetailId.Value).AgentReference_Id;
                    referenceId = agentReferenceId;
                }
                else if (underwriterDetailId.HasValue)
                {
                    referenceId = underwriterDetailId.Value.ToString();
                }

                // 20. File name
                string datePart = (AsOfDate ?? DateTime.Now).ToString("yyyyMMdd");
                string fileName;
                if (IsAgentId_Include && !string.IsNullOrEmpty(referenceId))
                {
                    fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}_{referenceId}.pdf";
                }
                else
                {
                    fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}.pdf";
                }

                // 21. Store original PDF bytes
                agentFiles.Add(new AgentStatementFile
                {
                    AgentDetailId = agentDetailId ?? 0,
                    AgentReferenceId = Convert.ToInt64(agentReferenceId),
                    FileName = fileName,
                    FileBytes = pdfBytes,
                    AgencyName = agencyName
                });
            }

            return agentFiles;
        }
        private void ApplyMultiLobInvoiceTotals(DataTable invoiceDetails, DataTable consolidatedInvoiceDetails)
        {
            string[] financialColumns = { "GrossAmount", "NetAmount", "RemainingBalance", "InferredPremiumCollected", "Comm.", "Comm. Paid to Date", "CommsNetDue", "Comm. Due", "AgentCommissionAmt", "TotalPremium", "TotalCommissionReceived" };

            var invoiceGroups = invoiceDetails.AsEnumerable().Where(row => Convert.ToString(row["LineItemDescription"]) != "Comm. Paid")
                .GroupBy(row => new
                {
                    PolicyDetailsId = row.Field<long>("PolicyDetails_Id"),
                    InvoiceId = row.Field<long>("Invoice_Id")
                });

            foreach (var invoiceGroup in invoiceGroups)
            {
                var lineOfBusinessRows = invoiceGroup.GroupBy(row => invoiceDetails.Columns.Contains("LOB") ? Convert.ToString(row["LOB"]) : string.Empty)
                    .Select(group => group.First()).ToList();

                if (lineOfBusinessRows.Count <= 1)
                    continue;

                var consolidatedRow = consolidatedInvoiceDetails.AsEnumerable().FirstOrDefault(row => row.Field<long>("PolicyDetails_Id") == invoiceGroup.Key.PolicyDetailsId && row.Field<long>("Invoice_Id") == invoiceGroup.Key.InvoiceId);

                if (consolidatedRow == null)
                    continue;

                foreach (string columnName in financialColumns)
                {
                    if (!invoiceDetails.Columns.Contains(columnName) || !consolidatedInvoiceDetails.Columns.Contains(columnName))
                    {
                        continue;
                    }
                    decimal consolidatedAmount = lineOfBusinessRows.Sum(row => row.IsNull(columnName) ? 0m : Convert.ToDecimal(row[columnName]));
                    consolidatedRow[columnName] = Convert.ChangeType(consolidatedAmount, consolidatedInvoiceDetails.Columns[columnName].DataType);
                }

                if (consolidatedInvoiceDetails.Columns.Contains("AgentCommPercentage"))
                {
                    decimal commissionAmount = consolidatedRow.IsNull("Comm.") ? 0m : Convert.ToDecimal(consolidatedRow["Comm."]);
                    decimal premiumCollected = consolidatedRow.IsNull("InferredPremiumCollected") ? 0m : Convert.ToDecimal(consolidatedRow["InferredPremiumCollected"]);
                    decimal commissionPercentage = premiumCollected == 0m ? 0m : Math.Round(commissionAmount / premiumCollected * 100m, 2);
                    consolidatedRow["AgentCommPercentage"] = Convert.ChangeType(commissionPercentage, consolidatedInvoiceDetails.Columns["AgentCommPercentage"].DataType);
                }
            }
        }
    }
}
