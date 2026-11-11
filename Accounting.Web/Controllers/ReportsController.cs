using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.EI_Connect;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Abstract;
using Accounting.Web.Models;
using DocumentFormat.OpenXml.Presentation;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Accounting.Web.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly IReportsRepository _IReportsRepository;
        private readonly ILU_SQLQueriesRepository _ISQLQueriesRepository;
        private readonly ILU_SQLInputParametersRepository _ISQLInputParametersRepository;
        private readonly ReportsUtility reportsUtility;
        private readonly IReportFileDownloadSettingsRepository _IReportFileDownloadSettingsRepository;
        private readonly IMSOfficeFileOptions _IMSOfficeFileOptions;
        private readonly ICSVOptions _ICSVOptions;
        private readonly IZip _IZip;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private readonly IPDFOptions _IPDFOptions;
        private readonly ViewRender view;
        private readonly IReportService _IReportService;
        public ReportsController(IUsersRepository userRepository, IReportsRepository iReportsRepository, ILU_SQLQueriesRepository iSQLQueriesRepository,
            ILU_SQLInputParametersRepository iSQLInputParametersRepository, IReportFileDownloadSettingsRepository iReportFileDownloadSettingsRepository,
            IMSOfficeFileOptions IMSOfficeFileOptions, ICSVOptions ICSVOptions, IZip IZip, IWebHostEnvironment IWebHostEnvironment, ICSVOptions iCSVOptions,
            IZip iZip, IWebHostEnvironment iWebHostEnvironment, IPDFOptions iPDFOptions, ViewRender view, IUserContextRepository userContextRepository, IReportService iReportService)
        {
            _IReportsRepository = iReportsRepository;
            _ISQLQueriesRepository = iSQLQueriesRepository;
            _IMSOfficeFileOptions = IMSOfficeFileOptions;
            _ISQLInputParametersRepository = iSQLInputParametersRepository;
            reportsUtility = new ReportsUtility(userContextRepository);
            _IReportFileDownloadSettingsRepository = iReportFileDownloadSettingsRepository;
            _ICSVOptions = iCSVOptions;
            _IZip = iZip;
            _IWebHostEnvironment = iWebHostEnvironment;
            _IPDFOptions = iPDFOptions;
            this.view = view;
            _IReportService = iReportService;
        }
        public IActionResult Index(long Report_Id)
        {
            long Client_Id = userSession.Client_Id;
            ViewBag.Report_Id = Report_Id;
            var Reports = _IReportsRepository.GetReports(Client_Id);
            if (!Is_AjaxRequest)
                return View(Reports);
            return PartialView(Reports);
        }
        public IActionResult ReportForm(long Report_Id)
        {
            int PageSize = MasterLookup._DefaultPageSize;
            int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);
            long TotalRecords = 0;
            ViewBag.TotalRecords = TotalRecords;
            ViewBag.PageIndex = PageIndex;
            ViewBag.PageSize = PageSize;
            ViewBag.Report_Id = Report_Id;
            List<LU_SQLQueries> _SQLQueries = _ISQLQueriesRepository.GetSQLQueriesByReport(Report_Id);
            List<LU_SQLInputParameters> _SQLInputParametersList = new List<LU_SQLInputParameters>();
            if (_SQLQueries != null && _SQLQueries.Count > 0)
            {
                foreach (var _SQLQuery in _SQLQueries)
                {
                    var sqlInputParameters = _ISQLInputParametersRepository.GetSQLQueryInputParameters(_SQLQuery.SQLQuery_Id);
                    if (sqlInputParameters != null && sqlInputParameters.Count > 0)
                    {

                        foreach (var SIParam in sqlInputParameters.OrderBy(x => x.Sequence))
                        {
                            if (string.IsNullOrEmpty(SIParam.GroupName) || !_SQLInputParametersList.Any(c => c.GroupName == SIParam.GroupName))
                                _SQLInputParametersList.Add(SIParam);
                        }
                    }
                }
            }
            if (_SQLInputParametersList != null && _SQLInputParametersList.Count > 0)
            {
                //Skip Predefined Parameters and the parameters that are not required to take the input from UI.
                _SQLInputParametersList = _SQLInputParametersList.Where(c => c.PredefinedParameter_Id == 0 && c.ShowInputField).OrderBy(c => c.SequenceNo).ToList();
                // Client-specific Outstanding Invoices parameter
                var outstandingInvoicesSetting = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.EnableOutstandingInvoicesForARReportAllTrans);
                bool enableOutstandingInvoices = !string.IsNullOrEmpty(outstandingInvoicesSetting) && Convert.ToBoolean(outstandingInvoicesSetting);
                // Only apply this rule to the Accounts Receivable report for AllTrans.
                if (Report_Id == MasterLookup._Reports.AccountReceivable && !enableOutstandingInvoices)
                {
                    _SQLInputParametersList = _SQLInputParametersList.Where(x => x.ParameterName != "OutstandingBalances").ToList();
                }
                // Client-specific Insured Name parameter
                var insuredNameSetting = LookupUtility.GetClientSettings(userSession.Client_Id, MasterLookup._ClientSettings.EnableInsuredNameFilterinInvoiceProduction);
                bool enableinsuredNameFilter = !string.IsNullOrEmpty(insuredNameSetting) && Convert.ToBoolean(insuredNameSetting);
                // Only apply this rule to the Invoice Production report for Guardian.
                if (Report_Id == MasterLookup._Reports.InvoiceProduction && !enableinsuredNameFilter)
                {
                    _SQLInputParametersList = _SQLInputParametersList.Where(x => x.ParameterName != "InsuredName").ToList();
                }
                foreach (var sqlInputParam in _SQLInputParametersList)
                {
                    if (sqlInputParam.DataDictionaryType_Id > 0 && sqlInputParam.Dependency_SQLParameter_Id == null)
                    {
                        sqlInputParam.DataDictionayType = LookupUtility.GetDataDictionaryType((int)sqlInputParam.DataDictionaryType_Id);
                        if (sqlInputParam.DataDictionayType != null)
                        {
                            sqlInputParam.DataDictionaryList = reportsUtility.GetDataDictionary(sqlInputParam.DataDictionayType, null);
                        }
                    }
                }
            }
            ViewBag.Report_Id = Report_Id;
            return View(_SQLInputParametersList);
        }
        public IActionResult Report(long Report_Id, List<LU_SQLInputParameters> SQLInputParameters)
        {
            int PageSize = !string.IsNullOrEmpty(HttpContext.Request.Query["PageSize"]) ? Convert.ToInt32(HttpContext.Request.Query["PageSize"]) : 0;
            int PageIndex = !string.IsNullOrEmpty(HttpContext.Request.Query["PageIndex"]) ? Convert.ToInt32(HttpContext.Request.Query["PageIndex"]) : 0;
            int? TotalRecords = 0;
            string ReportHTMLWithTemplate = "";
            long Client_Id = userSession.Client_Id;
            ViewBag.ClientId = Client_Id;
            if (ReportsUtility.ValidReportWithoutTemplates.Contains(Report_Id))
            {
                SQLInputParameters.AddRange(new List<LU_SQLInputParameters>
                {
                    new() {
                        ParameterName = "PageSize",
                        InputValue = Convert.ToString(PageSize)
                    },
                    new() {
                        ParameterName = "PageIndex",
                        InputValue = Convert.ToString(PageIndex)
                    },
                    new() {
                        ParameterName = "TotalRecords",
                        InputValue = "0"
                    }
                });
            }
            List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
            LU_ReportTemplates ReportTemplate = LookupUtility.GetAllReportTemplates()?.FirstOrDefault(x => x.Report_Id == Report_Id);
            if (ReportTemplate != null)
            {
                outputTablesList = reportsUtility.GetReportOutputTablesWithoutTotalrecords(Report_Id, SQLInputParameters);
                DateTime? AsOfDate = null;

                var asOfDateValue = SQLInputParameters.Where(x => x.ParameterName == "As_Of_Date").Select(x => x.InputValue).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(asOfDateValue) &&
                    DateTime.TryParseExact(
                    asOfDateValue,
                    new[] { "MM/dd/yyyy", "M/d/yyyy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
                {
                    AsOfDate = parsedDate;
                }
                else
                {
                    AsOfDate = DateTime.Now;
                }

                bool IsOutstandingInvoices = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "OutstandingBalances").Select(x => x.InputValue).FirstOrDefault());
                bool IsSimplifiedView = false;
                if (ReportsUtility.ValidReportWithTemplates.Contains(Report_Id))
                {
                    IsSimplifiedView = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "Simplified").Select(x => x.InputValue).FirstOrDefault());
                }
                ReportHTMLWithTemplate = _IReportService.BuildReportHtmlFromTemplate(Report_Id, Client_Id, outputTablesList, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
                ViewBag.ReportHTMLWithTemplate = ReportHTMLWithTemplate;
                ViewBag.ReportTemplate = ReportTemplate;
                ViewBag.Report_Id = Report_Id;
                ViewBag.IsSimplified = IsSimplifiedView;
            }
            else
            {
                outputTablesList = reportsUtility.GetReportOutputTables(Report_Id, SQLInputParameters, out TotalRecords);
                ViewBag.TotalRecords = TotalRecords;
                ViewBag.PageIndex = PageIndex;
                ViewBag.PageSize = PageSize;
                ViewBag.Report_Id = Report_Id;
                ViewBag.ReportsData = outputTablesList;
            }
            var fileDownLoadSettings = _IReportFileDownloadSettingsRepository.GetReportFileDownloadSettings(Report_Id);
            if (fileDownLoadSettings == null || fileDownLoadSettings.Count == 0)
            {
                var FileTypesList = LookupUtility.GetFileTypes();
                if (FileTypesList != null && FileTypesList.Count > 0)
                {
                    fileDownLoadSettings = new List<ReportFileDownloadSettings>();
                    foreach (var fileType in FileTypesList)
                    {
                        fileDownLoadSettings.Add(new ReportFileDownloadSettings() { FileType_Id = fileType.FileType_Id });
                    }
                }
            }
            ViewBag.FileDownLoadSettings = fileDownLoadSettings;
            return View(outputTablesList);
        }
        //    [NonAction]
        //    public string BuildReportHtmlFromTemplate(long Report_Id, List<ReportOutputTables> reportOutputTablesList, LU_ReportTemplates ReportTemplate, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null)
        //    {
        //        string FinalHTMLAlongWithTemplate = "";
        //        try
        //        {

        //            var Clients = _accountRepository.GetClients(userSession.Client_Id);
        //            string DynamicReportHTMLContent = "";
        //            string ClientLogo = $"<img src=\"data:image/jpg;base64,{Convert.ToBase64String(Clients.Logo)}\" alt=\"Client Logo\" style=\"height:45px;vertical-align: middle;\">";
        //            string ApplyFonts = "<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\" />\r\n<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin=\"anonymous\" />\r\n<link href=\"https://fonts.googleapis.com/css2?family=Poppins:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800;1,900&display=swap\" rel=\"stylesheet\" />";
        //            if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectBilledAccountStatement)
        //            {
        //                double totalNetAmount = 0;
        //                var tableAgency = reportOutputTablesList[0];
        //                var tablePolicies = reportOutputTablesList[1].DataTable;
        //                var table3 = reportOutputTablesList[2].DataTable;
        //                var table4 = reportOutputTablesList[3].DataTable;

        //                // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

        //                var agencyTableFinal = tableAgency.DataTable.Clone();
        //                var policiesTableFinal = tablePolicies.Clone();
        //                var invoicesTableFinal = table3.Clone();
        //                var table4Final = table4.Clone();
        //                var finalInvoiceTable = table4.Clone();
        //                if (IsOutstandingInvoices)
        //                {
        //                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
        //                                    .GroupBy(r => r.Field<long>("AgentDetail_Id"));


        //                    foreach (var agencyGroup in agencyGroups)
        //                    {
        //                        long agentDetailId = agencyGroup.Key;

        //                        var agencyTableTemp = tableAgency.DataTable.Clone();
        //                        foreach (var row in agencyGroup)
        //                            agencyTableTemp.ImportRow(row);

        //                        var policiesForAgent = tablePolicies.AsEnumerable()
        //                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                        var policiesTableTemp = tablePolicies.Clone();
        //                        foreach (var row in policiesForAgent)
        //                            policiesTableTemp.ImportRow(row);

        //                        var policyIds = policiesForAgent
        //                            .Where(r => !r.IsNull("Id"))
        //                            .Select(r => r.Field<long>("Id"))
        //                            .ToHashSet();

        //                        var invoicesForAgent = table3.AsEnumerable()
        //                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                        var invoicesTableTemp = table3.Clone();
        //                        foreach (var row in invoicesForAgent)
        //                            invoicesTableTemp.ImportRow(row);

        //                        var filteredTable4Rows = table4.AsEnumerable()
        //                            .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

        //                        var table4Temp = table4.Clone();
        //                        foreach (var row in filteredTable4Rows)
        //                            table4Temp.ImportRow(row);

        //                        //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
        //                        var policiesToRemove = table4Temp.AsEnumerable()
        //                            .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
        //                            .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
        //                            .Select(g => g.Key)
        //                            .ToHashSet();

        //                        if (policiesToRemove.Any())
        //                        {
        //                            foreach (var row in policiesTableTemp.AsEnumerable().ToList())
        //                            {
        //                                if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
        //                                    policiesTableTemp.Rows.Remove(row);
        //                            }

        //                            foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
        //                            {
        //                                if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
        //                                    invoicesTableTemp.Rows.Remove(row);
        //                            }

        //                            foreach (var row in table4Temp.AsEnumerable().ToList())
        //                            {
        //                                if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
        //                                    table4Temp.Rows.Remove(row);
        //                            }
        //                        }

        //                        if (table4Temp.Rows.Count > 0)
        //                        {
        //                            foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
        //                            foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
        //                            foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
        //                            foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
        //                        }
        //                    }

        //                    foreach (DataRow row in table4Final.Rows)
        //                    {
        //                        finalInvoiceTable.ImportRow(row);
        //                    }
        //                    var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
        //                        .Select(r => r.Field<long>("AgentDetail_Id")).Distinct().ToList();

        //                    var filteredAgencies = tableAgency.DataTable.AsEnumerable()
        //                        .Where(a => validAgencyIds.Contains(a.Field<long>("AgentDetail_Id")));

        //                    if (filteredAgencies.Any())
        //                        tableAgency.DataTable = filteredAgencies.CopyToDataTable();
        //                    else
        //                        tableAgency.DataTable = tableAgency.DataTable.Clone();

        //                }
        //                else
        //                {
        //                    finalInvoiceTable = table4;

        //                }

        //                if (IsSimplifiedView)
        //                {
        //                    var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

        //                    if (filteredRows.Any())
        //                        finalInvoiceTable = filteredRows.CopyToDataTable();
        //                }
        //                var commPaidRows = finalInvoiceTable.AsEnumerable()
        //                .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
        //                IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

        //                var groupedRows = finalInvoiceTable.AsEnumerable()
        //                        .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
        //                        .GroupBy(r => new
        //                        {
        //                            PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
        //                            InvoiceId = r.Field<long>("Invoice_Id")
        //                        })
        //                        .Select(g => g.First());

        //                DataTable tempGroupedRows;
        //                DataTable result;

        //                // if (groupedRows != null && groupedRows.Any())
        //                // {
        //                finalDistinct = groupedRows.Concat(commPaidRows);
        //                if (finalDistinct != null && finalDistinct.Any())
        //                {
        //                    result = finalDistinct.CopyToDataTable();

        //                    reportOutputTablesList[0].DataTable = tableAgency.DataTable;
        //                    reportOutputTablesList[3].DataTable = result;

        //                    if (reportOutputTablesList.Count > 3)
        //                    {
        //                        var invoiceDetails = result;
        //                        totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
        //                    }

        //                    var viewDataDictionary = new ViewDataDictionary
        //                                           (
        //                                   new EmptyModelMetadataProvider(),
        //                                        new ModelStateDictionary()
        //                                           )
        //                    {
        //                        Model = reportOutputTablesList,
        //                    };
        //                    viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
        //                    ViewBag.IsSimplifiedView = IsSimplifiedView;
        //                    DynamicReportHTMLContent = this.view.Render("Reports/DirectBilledAccountStatementDynamicReport", viewDataDictionary);
        //                    if (!IsSimplifiedView)
        //                    {
        //                        // Remove Comm. Paid to Date, Comm. Due columns + simplified-block
        //                        DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
        //                            new[] { "Comm. Paid to Date", "Comm. Due" });
        //                    }
        //                    else
        //                    {
        //                        DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
        //                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
        //                    }
        //                    FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
        //                        .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
        //                       .Replace("{{clientAddress}}", Clients?.Address ?? "")
        //                       .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
        //                       .Replace("{{clientLogo}}", ClientLogo)
        //                       //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ReportDateFormat))
        //                       .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
        //                       .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
        //                       .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
        //                }
        //                //}
        //            }
        //            else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.AgencyBilledAccountStatement)
        //            {
        //                double totalNetAmount = 0;
        //                var tableAgency = reportOutputTablesList[0];
        //                var tablePolicies = reportOutputTablesList[1];
        //                var tableInvoices = reportOutputTablesList[2];
        //                var table4 = reportOutputTablesList[3];
        //                var agencyGroups = tableAgency.DataTable.AsEnumerable()
        //                                    .GroupBy(r => r.Field<long>("AgentDetail_Id"));

        //                var agencyTableFinal = tableAgency.DataTable.Clone();
        //                var policiesTableFinal = tablePolicies.DataTable.Clone();
        //                var invoicesTableFinal = tableInvoices.DataTable.Clone();
        //                var table4Final = table4.DataTable.Clone();
        //                var Client_Id = Clients.Client_Id;
        //                var ReportsInvoiceDueDate = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.ReportsInvoiceDueDate);

        //                foreach (var agencyGroup in agencyGroups)
        //                {
        //                    long agentDetailId = agencyGroup.Key;

        //                    var agencyTableTemp = tableAgency.DataTable.Clone();
        //                    foreach (var row in agencyGroup)
        //                        agencyTableTemp.ImportRow(row);

        //                    var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
        //                        .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                    var policiesTableTemp = tablePolicies.DataTable.Clone();
        //                    foreach (var row in policiesForAgent)
        //                        policiesTableTemp.ImportRow(row);

        //                    var policyIds = policiesForAgent
        //                        .Where(r => !r.IsNull("Id"))
        //                        .Select(r => r.Field<long>("Id"))
        //                        .ToHashSet();

        //                    var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
        //                        .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                    var invoicesTableTemp = tableInvoices.DataTable.Clone();
        //                    foreach (var row in invoicesForAgent)
        //                        invoicesTableTemp.ImportRow(row);

        //                    var filteredTable4Rows = table4.DataTable.AsEnumerable()
        //                        .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

        //                    var table4Temp = table4.DataTable.Clone();
        //                    foreach (var row in filteredTable4Rows)
        //                        table4Temp.ImportRow(row);

        //                    if (table4Temp.Rows.Count > 0)
        //                    {
        //                        foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
        //                        foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
        //                        foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
        //                        foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
        //                    }
        //                }
        //                DataTable resultTable = table4Final.Clone();

        //                var groupedNonCash = table4Final.AsEnumerable()
        //                    .Where(r => r.Field<string>("LineItemDescription") != "Cash Receipt")
        //                    .GroupBy(r => new
        //                    {
        //                        PolicyDetails_Id = r.Field<long>("PolicyDetails_Id"),
        //                        Invoice_Id = r.Field<long>("Invoice_Id"),
        //                        PolicyNumber = r.Field<string>("PolicyNumber"),
        //                        TransactionType = r.Field<string>("TransactionType"),
        //                        LineItemDescription = r.Field<string>("LineItemDescription"),
        //                        IdNo = r.Field<string>("IdNo")
        //                    });

        //                foreach (var g in groupedNonCash)
        //                {
        //                    var first = g.First();
        //                    string lineItem = first.Field<string>("LineItemDescription");

        //                    DataRow row = resultTable.NewRow();

        //                    // Copy common columns
        //                    foreach (DataColumn col in resultTable.Columns)
        //                    {
        //                        if (col.ColumnName != "InvoiceDetail_Id" &&
        //                            col.ColumnName != "GrossAmount" &&
        //                            col.ColumnName != "AgentCommission" &&
        //                            col.ColumnName != "AgentCommissionRemainingBalance" &&
        //                            col.ColumnName != "NetAmount" &&
        //                            col.ColumnName != "RemainingBalance")
        //                        {
        //                            row[col.ColumnName] = first[col.ColumnName];
        //                        }
        //                    }

        //                    if (lineItem.Contains("Premium"))
        //                    {
        //                        row["GrossAmount"] = first["GrossAmount"];
        //                        row["AgentCommission"] = first["AgentCommission"];
        //                        row["AgentCommissionRemainingBalance"] = first["AgentCommissionRemainingBalance"];
        //                        row["NetAmount"] = first["NetAmount"];
        //                        //row["RemainingBalance"] = first["RemainingBalance"];
        //                        row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
        //                    }
        //                    else
        //                    {
        //                        row["GrossAmount"] = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0);
        //                        row["AgentCommission"] = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0);
        //                        row["AgentCommissionRemainingBalance"] = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0);
        //                        row["NetAmount"] = g.Sum(x => x.Field<double?>("NetAmount") ?? 0);
        //                        row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
        //                    }

        //                    resultTable.Rows.Add(row);
        //                }

        //                var groups = table4Final.AsEnumerable()
        //.Where(r => r.Field<string>("LineItemDescription") == "Cash Receipt")
        //.GroupBy(r => new
        //{
        //    InvoiceId = r.Field<long>("Invoice_Id"),
        //    IdNo = r.Field<string>("IdNo")
        //});

        //                foreach (var g in groups)
        //                {
        //                    var first = g.First();
        //                    DataRow newRow = resultTable.NewRow();

        //                    foreach (DataColumn col in resultTable.Columns)
        //                    {
        //                        if (col.ColumnName != "NetAmount")
        //                            newRow[col.ColumnName] = first[col.ColumnName];
        //                    }

        //                    newRow["NetAmount"] = g.Sum(r =>
        //                        r.Field<double?>("NetAmount") ?? 0);

        //                    resultTable.Rows.Add(newRow);
        //                }



        //                table4Final.Clear();
        //                table4Final.Merge(resultTable);


        //                if (IsSimplifiedView)
        //                {
        //                    DataTable dt = new DataTable();
        //                    dt.Columns.Add("PolicyNumber", typeof(string));
        //                    dt.Columns.Add("TransactionType", typeof(string));
        //                    dt.Columns.Add("Invoice_Id", typeof(long));
        //                    dt.Columns.Add("IdNo", typeof(string));
        //                    dt.Columns.Add("IsHouseCommission", typeof(bool));
        //                    dt.Columns.Add("ExpirationDate", typeof(DateTime));
        //                    dt.Columns.Add("EffectiveDate", typeof(DateTime));
        //                    if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
        //                    {
        //                        dt.Columns.Add("InvoiceDate", typeof(DateTime));
        //                    }
        //                    dt.Columns.Add("DueDate", typeof(DateTime));
        //                    dt.Columns.Add("GrossAmount", typeof(double));
        //                    dt.Columns.Add("AgentCommPercentage", typeof(double));
        //                    dt.Columns.Add("AgentCommission", typeof(double));
        //                    dt.Columns.Add("RemainingBalance", typeof(double));
        //                    dt.Columns.Add("AgentCommissionRemainingBalance", typeof(double));
        //                    dt.Columns.Add("NetAmount", typeof(double));
        //                    dt.Columns.Add("AmountDue", typeof(double));
        //                    dt.Columns.Add("AmountFinanced", typeof(double));
        //                    dt.Columns.Add("AmountPaid", typeof(double));
        //                    dt.Columns.Add("EndorsementNumber", typeof(int));
        //                    var paymentTotalsByInvoice = table4Final.AsEnumerable().Where(r => r.Field<string>("LineItemDescription")?.Trim()
        //                                .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true && r.Field<bool>("IsHouseCommission") == false)
        //                        .GroupBy(r => r.Field<long>("Invoice_Id"))
        //                        .ToDictionary(
        //                            g => g.Key,
        //                            g => g.Sum(x => Math.Abs(x.Field<double?>("NetAmount") ?? 0))
        //                        );


        //                    //var invoiceSummary = table4Final.AsEnumerable()
        //                    //    .Where(r =>
        //                    //        !(r.Field<string>("LineItemDescription")?
        //                    //            .Trim()
        //                    //            .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
        //                    //    )
        //                    //    .GroupBy(r => new
        //                    //    {
        //                    //        PolicyNumber = r.Field<string>("PolicyNumber"),
        //                    //        TransactionType = r.Field<string>("TransactionType"),
        //                    //        Invoice_Id = r.Field<long>("Invoice_Id"),
        //                    //        IdNo = r.Field<string>("IdNo"),
        //                    //        IsHouseCommission = r.Field<bool>("IsHouseCommission"),
        //                    //        EffectiveDate = r.Field<DateTime>("EffectiveDate"),
        //                    //        ExpirationDate = r.Field<DateTime>("ExpirationDate"),
        //                    //        DueDate = r.Field<DateTime>("DueDate"),
        //                    //        AmountFinanced = r.Field<double?>("AmountFinanced") ?? 0,
        //                    //        AmountPaid = r.Field<double?>("AmountPaid") ?? 0
        //                    //    })
        //                    //    .Select(g => new
        //                    //    {
        //                    //        g.Key.PolicyNumber,
        //                    //        g.Key.TransactionType,
        //                    //        g.Key.Invoice_Id,
        //                    //        g.Key.IdNo,
        //                    //        g.Key.IsHouseCommission,
        //                    //        g.Key.ExpirationDate,
        //                    //        g.Key.EffectiveDate,
        //                    //        g.Key.DueDate,
        //                    //        g.Key.AmountFinanced,
        //                    //        g.Key.AmountPaid,
        //                    //        GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
        //                    //        AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
        //                    //        AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
        //                    //        RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
        //                    //        AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
        //                    //        NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
        //                    //        AmountDue = g.Sum(x => (x.Field<double?>("GrossAmount") ?? 0) - (x.Field<double?>("AgentCommission") ?? 0)),                               
        //                    //        //AmountPaid = g.Sum(x => x.Field<double?>("AmountPaid") ?? 0)


        //                    //    });
        //                    var invoiceSummary = table4Final.AsEnumerable()
        //                        .Where(r =>
        //                            !(r.Field<string>("LineItemDescription")?
        //                                .Trim()
        //                                .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
        //                        )
        //                        .GroupBy(r => new
        //                        {
        //                            PolicyNumber = r.Field<string>("PolicyNumber"),
        //                            TransactionType = r.Field<string>("TransactionType"),
        //                            Invoice_Id = r.Field<long>("Invoice_Id"),
        //                            IdNo = r.Field<string>("IdNo"),
        //                            IsHouseCommission = r.Field<bool>("IsHouseCommission"),
        //                            EffectiveDate = r.Field<DateTime>("EffectiveDate").Date,
        //                            InvoiceDate = r.Field<DateTime>("InvoiceDate").Date,
        //                            ExpirationDate = r.Field<DateTime>("ExpirationDate").Date,
        //                            DueDate = r.Field<DateTime>("DueDate").Date
        //                        })
        //                        .Select(g => new
        //                        {
        //                            g.Key.PolicyNumber,
        //                            g.Key.TransactionType,
        //                            g.Key.Invoice_Id,
        //                            g.Key.IdNo,
        //                            g.Key.IsHouseCommission,
        //                            g.Key.ExpirationDate,
        //                            g.Key.EffectiveDate,
        //                            g.Key.InvoiceDate,
        //                            g.Key.DueDate,

        //                            AmountFinanced = g.Max(x => x.Field<double?>("AmountFinanced") ?? 0),
        //                            AmountPaid = g.Max(x => x.Field<double?>("AmountPaid") ?? 0),
        //                            EndorsementNumber = g.Max(x => x.Field<int?>("EndorsementNumber") ?? 0),
        //                            GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
        //                            AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
        //                            AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
        //                            RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
        //                            AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
        //                            NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
        //                            AmountDue = g.Sum(x =>
        //                                (x.Field<double?>("GrossAmount") ?? 0) -
        //                                (x.Field<double?>("AgentCommission") ?? 0))
        //                        });

        //                    foreach (var row in invoiceSummary)
        //                    {
        //                        //double amountPaid =
        //                        //    paymentTotalsByInvoice.TryGetValue(row.Invoice_Id, out var paid)
        //                        //        ? paid
        //                        //        : 0;

        //                        if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
        //                        {
        //                            dt.Rows.Add(
        //                            row.PolicyNumber,
        //                            row.TransactionType,
        //                            row.Invoice_Id,
        //                            row.IdNo,
        //                            row.IsHouseCommission,
        //                            row.ExpirationDate,
        //                            row.EffectiveDate,
        //                            row.InvoiceDate,
        //                            row.DueDate,
        //                            row.GrossAmount,
        //                            row.AgentCommPercentage,
        //                            row.AgentCommission,
        //                            row.RemainingBalance,
        //                            row.AgentCommissionRemainingBalance,
        //                            row.NetAmount,
        //                           row.AmountDue,
        //                           row.AmountFinanced,
        //                       // amountPaid
        //                       row.AmountPaid,
        //                       row.EndorsementNumber
        //                        );
        //                        }
        //                        else
        //                        {
        //                            dt.Rows.Add(
        //                                row.PolicyNumber,
        //                                row.TransactionType,
        //                                row.Invoice_Id,
        //                                row.IdNo,
        //                                row.IsHouseCommission,
        //                                row.ExpirationDate,
        //                                row.EffectiveDate,
        //                                row.DueDate,
        //                                row.GrossAmount,
        //                                row.AgentCommPercentage,
        //                                row.AgentCommission,
        //                                row.RemainingBalance,
        //                                row.AgentCommissionRemainingBalance,
        //                                row.NetAmount,
        //                               row.AmountDue,
        //                               row.AmountFinanced,
        //                           // amountPaid
        //                           row.AmountPaid,
        //                           row.EndorsementNumber
        //                            );
        //                        }


        //                    }

        //                    table4Final = dt;
        //                }
        //                table4Final = _IMSOfficeFileOptions.BalanceDueInAgencyBill(IsOutstandingInvoices, table4Final);

        //                if (table4Final.Rows.Count > 0)
        //                {
        //                    List<ReportOutputTables> agencyOutputTables = new()
        //                    {
        //                        new() { DataTable = agencyTableFinal },
        //                        new() { DataTable = policiesTableFinal },
        //                        new() { DataTable = invoicesTableFinal },
        //                        new() { DataTable = table4Final }
        //                    };

        //                    var policyTable = agencyOutputTables?[3]?.DataTable;
        //                    var policyAgentTable = agencyOutputTables?[2]?.DataTable;
        //                    var agentTable = agencyOutputTables?[0]?.DataTable;
        //                    var distinctPolicyNumbers = policyTable != null && policyTable.Columns.Contains("PolicyNumber") ? policyTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")).Select(r => r.Field<string>("PolicyNumber")).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList() : new List<string>();
        //                    var distinctAgents = policyAgentTable != null && policyAgentTable.Columns.Contains("PolicyNumber") && policyAgentTable.Columns.Contains("AgentDetail_Id") ? policyAgentTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")
        //                                            && distinctPolicyNumbers.Contains(r.Field<string>("PolicyNumber"))
        //                                            && !r.IsNull("AgentDetail_Id")).Select(r => Convert.ToInt64(r["AgentDetail_Id"])).Distinct().ToList() : new List<long>();
        //                    var filteredAgentsTable = agentTable?.Clone() ?? new DataTable();
        //                    if (agentTable != null && agentTable.Columns.Contains("AgentDetail_Id"))
        //                    {
        //                        var agentIds = distinctAgents.ToHashSet();
        //                        var rows = agentTable.AsEnumerable().Where(r => !r.IsNull("AgentDetail_Id") && agentIds.Contains(Convert.ToInt64(r["AgentDetail_Id"])));
        //                        if (rows.Any())
        //                        {
        //                            filteredAgentsTable = rows.CopyToDataTable();
        //                            agencyOutputTables[0].DataTable = filteredAgentsTable;
        //                        }
        //                    }

        //                    if (agencyOutputTables.Count > 3)
        //                    {
        //                        var invoiceDetails = agencyOutputTables[3].DataTable;
        //                        totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("NetAmount")) ?? 0;
        //                    }
        //                    var viewDataDictionary = new ViewDataDictionary
        //                                           (
        //                                   new EmptyModelMetadataProvider(),
        //                                        new ModelStateDictionary()
        //                                           )
        //                    {
        //                        Model = agencyOutputTables,
        //                    };

        //                    viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
        //                    ViewBag.IsSimplifiedView = IsSimplifiedView;
        //                    viewDataDictionary["ReportsInvoiceDueDate"] = ReportsInvoiceDueDate;
        //                    ViewBag.ReportsInvoiceDueDate = ReportsInvoiceDueDate;
        //                    viewDataDictionary["Client_Id"] = userSession.Client_Id;
        //                    DynamicReportHTMLContent = this.view.Render("Reports/AgencyBilledAccountStatementDynamicReport", viewDataDictionary);

        //                    if (!IsSimplifiedView)
        //                    {

        //                        DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
        //                            new[] { "Amount Due", "Amount Paid", "NET Due" });
        //                    }
        //                    else
        //                    {
        //                        DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
        //                            new[] { "NET" });
        //                        DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
        //                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
        //                    }
        //                    FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
        //                                                                           .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
        //                                                                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
        //                                                                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
        //                                                                           .Replace("{{clientLogo}}", ClientLogo)
        //                                                                           //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ServerDateFormat))
        //                                                                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
        //                                                                           .Replace("{{dynamicAgentloopingLogic}}", DynamicReportHTMLContent)
        //                                                                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString())
        //                                                                           ;

        //                }
        //            }
        //            else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.InsuredBilledAccountStatement)
        //            {
        //                double totalNetAmount = 0;
        //                var tableAgency = reportOutputTablesList[0];
        //                var tablePolicies = reportOutputTablesList[1].DataTable;
        //                var table3 = reportOutputTablesList[2].DataTable;
        //                var table4 = reportOutputTablesList[3].DataTable;

        //                // Remove duplicates by grouping on PolicyDetails_Id + Invoice_Id

        //                var agencyTableFinal = tableAgency.DataTable.Clone();
        //                var policiesTableFinal = tablePolicies.Clone();
        //                var invoicesTableFinal = table3.Clone();
        //                var table4Final = table4.Clone();
        //                var finalInvoiceTable = table4.Clone();
        //                if (IsOutstandingInvoices)
        //                {
        //                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
        //                                    .GroupBy(r => r.Field<long>("AgentDetail_Id"));


        //                    foreach (var agencyGroup in agencyGroups)
        //                    {
        //                        long agentDetailId = agencyGroup.Key;

        //                        var agencyTableTemp = tableAgency.DataTable.Clone();
        //                        foreach (var row in agencyGroup)
        //                            agencyTableTemp.ImportRow(row);

        //                        var policiesForAgent = tablePolicies.AsEnumerable()
        //                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                        var policiesTableTemp = tablePolicies.Clone();
        //                        foreach (var row in policiesForAgent)
        //                            policiesTableTemp.ImportRow(row);

        //                        var policyIds = policiesForAgent
        //                            .Where(r => !r.IsNull("Id"))
        //                            .Select(r => r.Field<long>("Id"))
        //                            .ToHashSet();

        //                        var invoicesForAgent = table3.AsEnumerable()
        //                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                        var invoicesTableTemp = table3.Clone();
        //                        foreach (var row in invoicesForAgent)
        //                            invoicesTableTemp.ImportRow(row);

        //                        var filteredTable4Rows = table4.AsEnumerable()
        //                            .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

        //                        var table4Temp = table4.Clone();
        //                        foreach (var row in filteredTable4Rows)
        //                            table4Temp.ImportRow(row);

        //                        //  Group by PolicyNumber and remove those with SUM(CommsNetDue) == 0
        //                        var policiesToRemove = table4Temp.AsEnumerable()
        //                            .GroupBy(r => r.Field<string>("PolicyNumber")) // group by PolicyNumber
        //                            .Where(g => g.Sum(r => r.Field<double>("Comm. Due")) == 0) // Sum = 0 changed as commsNetDue is coming wrong//CommsNetDue
        //                            .Select(g => g.Key)
        //                            .ToHashSet();

        //                        if (policiesToRemove.Any())
        //                        {
        //                            foreach (var row in policiesTableTemp.AsEnumerable().ToList())
        //                            {
        //                                if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
        //                                    policiesTableTemp.Rows.Remove(row);
        //                            }

        //                            foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
        //                            {
        //                                if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
        //                                    invoicesTableTemp.Rows.Remove(row);
        //                            }

        //                            foreach (var row in table4Temp.AsEnumerable().ToList())
        //                            {
        //                                if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
        //                                    table4Temp.Rows.Remove(row);
        //                            }
        //                        }

        //                        if (table4Temp.Rows.Count > 0)
        //                        {
        //                            foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
        //                            foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
        //                            foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
        //                            foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
        //                        }
        //                    }

        //                    foreach (DataRow row in table4Final.Rows)
        //                    {
        //                        finalInvoiceTable.ImportRow(row);
        //                    }
        //                    var validAgencyIds = table4Final.AsEnumerable().Where(r => !r.IsNull("Invoice_Id"))
        //                        .Select(r => r.Field<long>("AgentDetail_Id")).Distinct().ToList();

        //                    var filteredAgencies = tableAgency.DataTable.AsEnumerable()
        //                        .Where(a => validAgencyIds.Contains(a.Field<long>("AgentDetail_Id")));

        //                    if (filteredAgencies.Any())
        //                        tableAgency.DataTable = filteredAgencies.CopyToDataTable();
        //                    else
        //                        tableAgency.DataTable = tableAgency.DataTable.Clone();

        //                }
        //                else
        //                {
        //                    finalInvoiceTable = table4;

        //                }

        //                if (IsSimplifiedView)
        //                {
        //                    var filteredRows = finalInvoiceTable.AsEnumerable().Where(dr => Convert.ToString(dr["LineItemDescription"]) != "Comm. Paid");

        //                    if (filteredRows.Any())
        //                        finalInvoiceTable = filteredRows.CopyToDataTable();
        //                }
        //                var commPaidRows = finalInvoiceTable.AsEnumerable()
        //                .Where(r => r.Field<string>("LineItemDescription") == "Comm. Paid");
        //                IEnumerable<DataRow> finalDistinct = Enumerable.Empty<DataRow>();

        //                var groupedRows = finalInvoiceTable.AsEnumerable()
        //                        .Where(r => r.Field<string>("LineItemDescription") != "Comm. Paid")
        //                        .GroupBy(r => new
        //                        {
        //                            PolicyDetailId = r.Field<long>("PolicyDetails_Id"),
        //                            InvoiceId = r.Field<long>("Invoice_Id")
        //                        })
        //                        .Select(g => g.First());

        //                DataTable tempGroupedRows;
        //                DataTable result;

        //                finalDistinct = groupedRows.Concat(commPaidRows);
        //                if (finalDistinct != null && finalDistinct.Any())
        //                {
        //                    result = finalDistinct.CopyToDataTable();

        //                    reportOutputTablesList[0].DataTable = tableAgency.DataTable;
        //                    reportOutputTablesList[3].DataTable = result;

        //                    if (reportOutputTablesList.Count > 3)
        //                    {
        //                        var invoiceDetails = result;
        //                        totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("Comm. Due")) ?? 0;
        //                    }

        //                    var viewDataDictionary = new ViewDataDictionary
        //                                           (
        //                                   new EmptyModelMetadataProvider(),
        //                                        new ModelStateDictionary()
        //                                           )
        //                    {
        //                        Model = reportOutputTablesList,
        //                    };
        //                    viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
        //                    ViewBag.IsSimplifiedView = IsSimplifiedView;
        //                    DynamicReportHTMLContent = this.view.Render("Reports/InsuredBilledAccountStatementDynamicReport", viewDataDictionary);
        //                    if (!IsSimplifiedView)
        //                    {
        //                        DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
        //                            new[] { "Comm. Paid to Date", "Comm. Due" });
        //                    }
        //                    else
        //                    {
        //                        DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
        //                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
        //                    }
        //                    FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
        //                        .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
        //                       .Replace("{{clientAddress}}", Clients?.Address ?? "")
        //                       .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
        //                       .Replace("{{clientLogo}}", ClientLogo)
        //                       .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
        //                       .Replace("{{carrierBlocks}}", DynamicReportHTMLContent)
        //                       .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString());
        //                }
        //            }
        //            else if (ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.AgencyDetailedAccountStatement)
        //            {
        //                double totalNetAmount = 0;
        //                var tableAgency = reportOutputTablesList[0];
        //                var tablePolicies = reportOutputTablesList[1];
        //                var tableInvoices = reportOutputTablesList[2];
        //                var table4 = reportOutputTablesList[3];
        //                var agencyGroups = tableAgency.DataTable.AsEnumerable()
        //                                    .GroupBy(r => r.Field<long>("AgentDetail_Id"));

        //                var agencyTableFinal = tableAgency.DataTable.Clone();
        //                var policiesTableFinal = tablePolicies.DataTable.Clone();
        //                var invoicesTableFinal = tableInvoices.DataTable.Clone();
        //                var table4Final = table4.DataTable.Clone();
        //                var Client_Id = Clients.Client_Id;
        //                var ReportsInvoiceDueDate = LookupUtility.GetClientSettings(Clients.Client_Id, MasterLookup._ClientSettings.ReportsInvoiceDueDate);

        //                foreach (var agencyGroup in agencyGroups)
        //                {
        //                    long agentDetailId = agencyGroup.Key;

        //                    var agencyTableTemp = tableAgency.DataTable.Clone();
        //                    foreach (var row in agencyGroup)
        //                        agencyTableTemp.ImportRow(row);

        //                    var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
        //                        .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                    var policiesTableTemp = tablePolicies.DataTable.Clone();
        //                    foreach (var row in policiesForAgent)
        //                        policiesTableTemp.ImportRow(row);

        //                    var policyIds = policiesForAgent
        //                        .Where(r => !r.IsNull("Id"))
        //                        .Select(r => r.Field<long>("Id"))
        //                        .ToHashSet();

        //                    var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
        //                        .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId);

        //                    var invoicesTableTemp = tableInvoices.DataTable.Clone();
        //                    foreach (var row in invoicesForAgent)
        //                        invoicesTableTemp.ImportRow(row);

        //                    var filteredTable4Rows = table4.DataTable.AsEnumerable()
        //                        .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")));

        //                    var table4Temp = table4.DataTable.Clone();
        //                    foreach (var row in filteredTable4Rows)
        //                        table4Temp.ImportRow(row);

        //                    if (table4Temp.Rows.Count > 0)
        //                    {
        //                        foreach (DataRow row in agencyTableTemp.Rows) agencyTableFinal.ImportRow(row);
        //                        foreach (DataRow row in policiesTableTemp.Rows) policiesTableFinal.ImportRow(row);
        //                        foreach (DataRow row in invoicesTableTemp.Rows) invoicesTableFinal.ImportRow(row);
        //                        foreach (DataRow row in table4Temp.Rows) table4Final.ImportRow(row);
        //                    }
        //                }
        //                DataTable resultTable = table4Final.Clone();

        //                var groupedNonCash = table4Final.AsEnumerable()
        //                    .Where(r => r.Field<string>("LineItemDescription") != "Cash Receipt")
        //                    .GroupBy(r => new
        //                    {
        //                        PolicyDetails_Id = r.Field<long>("PolicyDetails_Id"),
        //                        Invoice_Id = r.Field<long>("Invoice_Id"),
        //                        PolicyNumber = r.Field<string>("PolicyNumber"),
        //                        TransactionType = r.Field<string>("TransactionType"),
        //                        LineItemDescription = r.Field<string>("LineItemDescription"),
        //                        IdNo = r.Field<string>("IdNo")
        //                    });

        //                foreach (var g in groupedNonCash)
        //                {
        //                    var first = g.First();
        //                    string lineItem = first.Field<string>("LineItemDescription");

        //                    DataRow row = resultTable.NewRow();

        //                    // Copy common columns
        //                    foreach (DataColumn col in resultTable.Columns)
        //                    {
        //                        if (col.ColumnName != "InvoiceDetail_Id" &&
        //                            col.ColumnName != "GrossAmount" &&
        //                            col.ColumnName != "AgentCommission" &&
        //                            col.ColumnName != "AgentCommissionRemainingBalance" &&
        //                            col.ColumnName != "NetAmount" &&
        //                            col.ColumnName != "RemainingBalance")
        //                        {
        //                            row[col.ColumnName] = first[col.ColumnName];
        //                        }
        //                    }

        //                    if (lineItem.Contains("Premium"))
        //                    {
        //                        row["GrossAmount"] = first["GrossAmount"];
        //                        row["AgentCommission"] = first["AgentCommission"];
        //                        row["AgentCommissionRemainingBalance"] = first["AgentCommissionRemainingBalance"];
        //                        row["NetAmount"] = first["NetAmount"];
        //                        //row["RemainingBalance"] = first["RemainingBalance"];
        //                        row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
        //                    }
        //                    else
        //                    {
        //                        row["GrossAmount"] = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0);
        //                        row["AgentCommission"] = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0);
        //                        row["AgentCommissionRemainingBalance"] = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0);
        //                        row["NetAmount"] = g.Sum(x => x.Field<double?>("NetAmount") ?? 0);
        //                        row["RemainingBalance"] = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0);
        //                    }

        //                    resultTable.Rows.Add(row);
        //                }

        //                var groups = table4Final.AsEnumerable()
        //.Where(r => r.Field<string>("LineItemDescription") == "Cash Receipt")
        //.GroupBy(r => new
        //{
        //    InvoiceId = r.Field<long>("Invoice_Id"),
        //    IdNo = r.Field<string>("IdNo")
        //});

        //                foreach (var g in groups)
        //                {
        //                    var first = g.First();
        //                    DataRow newRow = resultTable.NewRow();

        //                    foreach (DataColumn col in resultTable.Columns)
        //                    {
        //                        if (col.ColumnName != "NetAmount")
        //                            newRow[col.ColumnName] = first[col.ColumnName];
        //                    }

        //                    newRow["NetAmount"] = g.Sum(r =>
        //                        r.Field<double?>("NetAmount") ?? 0);

        //                    resultTable.Rows.Add(newRow);
        //                }



        //                table4Final.Clear();
        //                table4Final.Merge(resultTable);


        //                if (IsSimplifiedView)
        //                {
        //                    DataTable dt = new DataTable();
        //                    dt.Columns.Add("PolicyNumber", typeof(string));
        //                    dt.Columns.Add("TransactionType", typeof(string));
        //                    dt.Columns.Add("Invoice_Id", typeof(long));
        //                    dt.Columns.Add("IdNo", typeof(string));
        //                    dt.Columns.Add("IsHouseCommission", typeof(bool));
        //                    dt.Columns.Add("ExpirationDate", typeof(DateTime));
        //                    dt.Columns.Add("EffectiveDate", typeof(DateTime));
        //                    if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
        //                    {
        //                        dt.Columns.Add("InvoiceDate", typeof(DateTime));
        //                    }
        //                    dt.Columns.Add("DueDate", typeof(DateTime));
        //                    dt.Columns.Add("GrossAmount", typeof(double));
        //                    dt.Columns.Add("AgentCommPercentage", typeof(double));
        //                    dt.Columns.Add("AgentCommission", typeof(double));
        //                    dt.Columns.Add("RemainingBalance", typeof(double));
        //                    dt.Columns.Add("AgentCommissionRemainingBalance", typeof(double));
        //                    dt.Columns.Add("NetAmount", typeof(double));
        //                    dt.Columns.Add("AmountDue", typeof(double));
        //                    dt.Columns.Add("AmountFinanced", typeof(double));
        //                    dt.Columns.Add("AmountPaid", typeof(double));
        //                    dt.Columns.Add("EndorsementNumber", typeof(int));
        //                    var paymentTotalsByInvoice = table4Final.AsEnumerable().Where(r => r.Field<string>("LineItemDescription")?.Trim()
        //                                .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true && r.Field<bool>("IsHouseCommission") == false)
        //                        .GroupBy(r => r.Field<long>("Invoice_Id"))
        //                        .ToDictionary(
        //                            g => g.Key,
        //                            g => g.Sum(x => Math.Abs(x.Field<double?>("NetAmount") ?? 0))
        //                        );


        //                    //var invoiceSummary = table4Final.AsEnumerable()
        //                    //    .Where(r =>
        //                    //        !(r.Field<string>("LineItemDescription")?
        //                    //            .Trim()
        //                    //            .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
        //                    //    )
        //                    //    .GroupBy(r => new
        //                    //    {
        //                    //        PolicyNumber = r.Field<string>("PolicyNumber"),
        //                    //        TransactionType = r.Field<string>("TransactionType"),
        //                    //        Invoice_Id = r.Field<long>("Invoice_Id"),
        //                    //        IdNo = r.Field<string>("IdNo"),
        //                    //        IsHouseCommission = r.Field<bool>("IsHouseCommission"),
        //                    //        EffectiveDate = r.Field<DateTime>("EffectiveDate"),
        //                    //        ExpirationDate = r.Field<DateTime>("ExpirationDate"),
        //                    //        DueDate = r.Field<DateTime>("DueDate"),
        //                    //        AmountFinanced = r.Field<double?>("AmountFinanced") ?? 0,
        //                    //        AmountPaid = r.Field<double?>("AmountPaid") ?? 0
        //                    //    })
        //                    //    .Select(g => new
        //                    //    {
        //                    //        g.Key.PolicyNumber,
        //                    //        g.Key.TransactionType,
        //                    //        g.Key.Invoice_Id,
        //                    //        g.Key.IdNo,
        //                    //        g.Key.IsHouseCommission,
        //                    //        g.Key.ExpirationDate,
        //                    //        g.Key.EffectiveDate,
        //                    //        g.Key.DueDate,
        //                    //        g.Key.AmountFinanced,
        //                    //        g.Key.AmountPaid,
        //                    //        GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
        //                    //        AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
        //                    //        AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
        //                    //        RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
        //                    //        AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
        //                    //        NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
        //                    //        AmountDue = g.Sum(x => (x.Field<double?>("GrossAmount") ?? 0) - (x.Field<double?>("AgentCommission") ?? 0)),                               
        //                    //        //AmountPaid = g.Sum(x => x.Field<double?>("AmountPaid") ?? 0)


        //                    //    });
        //                    var invoiceSummary = table4Final.AsEnumerable()
        //                        .Where(r =>
        //                            !(r.Field<string>("LineItemDescription")?
        //                                .Trim()
        //                                .Equals("Cash Receipt", StringComparison.OrdinalIgnoreCase) == true)
        //                        )
        //                        .GroupBy(r => new
        //                        {
        //                            PolicyNumber = r.Field<string>("PolicyNumber"),
        //                            TransactionType = r.Field<string>("TransactionType"),
        //                            Invoice_Id = r.Field<long>("Invoice_Id"),
        //                            IdNo = r.Field<string>("IdNo"),
        //                            IsHouseCommission = r.Field<bool>("IsHouseCommission"),
        //                            EffectiveDate = r.Field<DateTime>("EffectiveDate").Date,
        //                            InvoiceDate = r.Field<DateTime>("InvoiceDate").Date,
        //                            ExpirationDate = r.Field<DateTime>("ExpirationDate").Date,
        //                            DueDate = r.Field<DateTime>("DueDate").Date
        //                        })
        //                        .Select(g => new
        //                        {
        //                            g.Key.PolicyNumber,
        //                            g.Key.TransactionType,
        //                            g.Key.Invoice_Id,
        //                            g.Key.IdNo,
        //                            g.Key.IsHouseCommission,
        //                            g.Key.ExpirationDate,
        //                            g.Key.EffectiveDate,
        //                            g.Key.InvoiceDate,
        //                            g.Key.DueDate,

        //                            AmountFinanced = g.Max(x => x.Field<double?>("AmountFinanced") ?? 0),
        //                            AmountPaid = g.Max(x => x.Field<double?>("AmountPaid") ?? 0),
        //                            EndorsementNumber = g.Max(x => x.Field<int?>("EndorsementNumber") ?? 0),
        //                            GrossAmount = g.Sum(x => x.Field<double?>("GrossAmount") ?? 0),
        //                            AgentCommPercentage = g.Sum(x => x.Field<double?>("AgentCommPercentage") ?? 0),
        //                            AgentCommission = g.Sum(x => x.Field<double?>("AgentCommission") ?? 0),
        //                            RemainingBalance = g.Sum(x => x.Field<double?>("RemainingBalance") ?? 0),
        //                            AgentCommissionRemainingBalance = g.Sum(x => x.Field<double?>("AgentCommissionRemainingBalance") ?? 0),
        //                            NetAmount = g.Sum(x => x.Field<double?>("NetAmount") ?? 0),
        //                            AmountDue = g.Sum(x =>
        //                                (x.Field<double?>("GrossAmount") ?? 0) -
        //                                (x.Field<double?>("AgentCommission") ?? 0))
        //                        });

        //                    foreach (var row in invoiceSummary)
        //                    {
        //                        //double amountPaid =
        //                        //    paymentTotalsByInvoice.TryGetValue(row.Invoice_Id, out var paid)
        //                        //        ? paid
        //                        //        : 0;

        //                        if (!string.IsNullOrEmpty(ReportsInvoiceDueDate))
        //                        {
        //                            dt.Rows.Add(
        //                            row.PolicyNumber,
        //                            row.TransactionType,
        //                            row.Invoice_Id,
        //                            row.IdNo,
        //                            row.IsHouseCommission,
        //                            row.ExpirationDate,
        //                            row.EffectiveDate,
        //                            row.InvoiceDate,
        //                            row.DueDate,
        //                            row.GrossAmount,
        //                            row.AgentCommPercentage,
        //                            row.AgentCommission,
        //                            row.RemainingBalance,
        //                            row.AgentCommissionRemainingBalance,
        //                            row.NetAmount,
        //                           row.AmountDue,
        //                           row.AmountFinanced,
        //                       // amountPaid
        //                       row.AmountPaid,
        //                       row.EndorsementNumber
        //                        );
        //                        }
        //                        else
        //                        {
        //                            dt.Rows.Add(
        //                                row.PolicyNumber,
        //                                row.TransactionType,
        //                                row.Invoice_Id,
        //                                row.IdNo,
        //                                row.IsHouseCommission,
        //                                row.ExpirationDate,
        //                                row.EffectiveDate,
        //                                row.DueDate,
        //                                row.GrossAmount,
        //                                row.AgentCommPercentage,
        //                                row.AgentCommission,
        //                                row.RemainingBalance,
        //                                row.AgentCommissionRemainingBalance,
        //                                row.NetAmount,
        //                               row.AmountDue,
        //                               row.AmountFinanced,
        //                           // amountPaid
        //                           row.AmountPaid,
        //                           row.EndorsementNumber
        //                            );
        //                        }


        //                    }

        //                    table4Final = dt;
        //                }
        //                table4Final = _IMSOfficeFileOptions.BalanceDueInAgencyBill(IsOutstandingInvoices, table4Final);

        //                if (table4Final.Rows.Count > 0)
        //                {
        //                    List<ReportOutputTables> agencyOutputTables = new()
        //                    {
        //                        new() { DataTable = agencyTableFinal },
        //                        new() { DataTable = policiesTableFinal },
        //                        new() { DataTable = invoicesTableFinal },
        //                        new() { DataTable = table4Final }
        //                    };

        //                    var policyTable = agencyOutputTables?[3]?.DataTable;
        //                    var policyAgentTable = agencyOutputTables?[2]?.DataTable;
        //                    var agentTable = agencyOutputTables?[0]?.DataTable;
        //                    var distinctPolicyNumbers = policyTable != null && policyTable.Columns.Contains("PolicyNumber") ? policyTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")).Select(r => r.Field<string>("PolicyNumber")).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList() : new List<string>();
        //                    var distinctAgents = policyAgentTable != null && policyAgentTable.Columns.Contains("PolicyNumber") && policyAgentTable.Columns.Contains("AgentDetail_Id") ? policyAgentTable.AsEnumerable().Where(r => !r.IsNull("PolicyNumber")
        //                                            && distinctPolicyNumbers.Contains(r.Field<string>("PolicyNumber"))
        //                                            && !r.IsNull("AgentDetail_Id")).Select(r => Convert.ToInt64(r["AgentDetail_Id"])).Distinct().ToList() : new List<long>();
        //                    var filteredAgentsTable = agentTable?.Clone() ?? new DataTable();
        //                    if (agentTable != null && agentTable.Columns.Contains("AgentDetail_Id"))
        //                    {
        //                        var agentIds = distinctAgents.ToHashSet();
        //                        var rows = agentTable.AsEnumerable().Where(r => !r.IsNull("AgentDetail_Id") && agentIds.Contains(Convert.ToInt64(r["AgentDetail_Id"])));
        //                        if (rows.Any())
        //                        {
        //                            filteredAgentsTable = rows.CopyToDataTable();
        //                            agencyOutputTables[0].DataTable = filteredAgentsTable;
        //                        }
        //                    }

        //                    if (agencyOutputTables.Count > 3)
        //                    {
        //                        var invoiceDetails = agencyOutputTables[3].DataTable;
        //                        totalNetAmount = invoiceDetails?.AsEnumerable()?.Sum(r => r.Field<double>("NetAmount")) ?? 0;
        //                    }
        //                    var viewDataDictionary = new ViewDataDictionary
        //                                           (
        //                                   new EmptyModelMetadataProvider(),
        //                                        new ModelStateDictionary()
        //                                           )
        //                    {
        //                        Model = agencyOutputTables,
        //                    };

        //                    viewDataDictionary["IsSimplifiedView"] = IsSimplifiedView;
        //                    ViewBag.IsSimplifiedView = IsSimplifiedView;
        //                    viewDataDictionary["ReportsInvoiceDueDate"] = ReportsInvoiceDueDate;
        //                    ViewBag.ReportsInvoiceDueDate = ReportsInvoiceDueDate;
        //                    DynamicReportHTMLContent = this.view.Render("Reports/AgencyDetailedAccountStatementDynamicReport", viewDataDictionary);

        //                    if (!IsSimplifiedView)
        //                    {

        //                        DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
        //                            new[] { "Amount Due", "Amount Paid", "NET Due" });
        //                    }
        //                    else
        //                    {
        //                        DynamicReportHTMLContent = RemoveSimplifiedColumns(DynamicReportHTMLContent,
        //                            new[] { "NET" });
        //                        DynamicReportHTMLContent = Regex.Replace(DynamicReportHTMLContent, @"<tr[^>]*class\s*=\s*[""']simplified-block[""'][^>]*>.*?</tr>", "",
        //                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
        //                    }
        //                    FinalHTMLAlongWithTemplate = ReportTemplate.Template.Replace("{{fonts}}", ApplyFonts)
        //                                                                           .Replace("{{clientCompanyName}}", Clients?.CompanyName ?? "")
        //                                                                           .Replace("{{clientAddress}}", Clients?.Address ?? "")
        //                                                                           .Replace("{{clientCityStateZip}}", $"{Clients?.City}, {Clients?.State}, {Clients?.ZipCode}")
        //                                                                           .Replace("{{clientLogo}}", ClientLogo)
        //                                                                           //.Replace("{{statementDate}}", DateTime.Now.ToString(MasterLookup._ServerDateFormat))
        //                                                                           .Replace("{{statementDate}}", AsOfDate.HasValue ? AsOfDate.Value.ToString(MasterLookup._ReportDateFormat) : DateTime.Now.ToString(MasterLookup._ReportDateFormat))
        //                                                                           .Replace("{{dynamicAgentloopingLogic}}", DynamicReportHTMLContent)
        //                                                                           .Replace("{{TotalNetDue}}", CommonUtility.ConvertToCurrencyFormat(totalNetAmount, false, 2).ToString())
        //                                                                           ;

        //                }
        //            }
        //            else
        //            {
        //                FinalHTMLAlongWithTemplate = "";
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            FinalHTMLAlongWithTemplate = "";
        //        }
        //        return FinalHTMLAlongWithTemplate;
        //    }

        public IActionResult DownloadReport(long Report_Id, string ActionType, byte FileType_Id, List<LU_SQLInputParameters> SQLInputParameters, bool IsAgentId_Include)
        {
            int? totalRecords;

            int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
            int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);
            int? TotalRecords = 0;
            long Client_Id = userSession.Client_Id;
            if (ReportsUtility.ValidReportWithoutTemplates.Contains(Report_Id))
            {
                SQLInputParameters.AddRange(new List<LU_SQLInputParameters>
            {
                new() {
                    ParameterName = "PageSize",
                    InputValue = Convert.ToString(PageSize)
                },
                new() {
                    ParameterName = "PageIndex",
                    InputValue = Convert.ToString(PageIndex)
                },
                new() {
                    ParameterName = "TotalRecords",
                    InputValue = "0"
                }
            });
            }

            List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
            LU_ReportTemplates ReportTemplate = LookupUtility.GetAllReportTemplates().FirstOrDefault(x => x.Report_Id == Report_Id);
            if (ReportTemplate != null)
            {
                outputTablesList = reportsUtility.GetReportOutputTablesWithoutTotalrecords(Report_Id, SQLInputParameters);
            }
            else
            {
                outputTablesList = reportsUtility.GetReportOutputTables(Report_Id, SQLInputParameters, out totalRecords, FileType_Id);
            }
            DateTime? AsOfDate = null;

            var asOfDateValue = SQLInputParameters.Where(x => x.ParameterName == "As_Of_Date").Select(x => x.InputValue).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(asOfDateValue))
            {
                if (DateTime.TryParseExact(
                        asOfDateValue,
                        new[] { "MM/dd/yyyy", "M/d/yyyy" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedDate))
                {
                    AsOfDate = parsedDate;
                }
                else
                {
                    AsOfDate = DateTime.Now;
                }
            }
            bool IsOutstandingInvoices = false;
            bool IsSimplifiedView = false;
            if (ReportTemplate != null)
            {
                if (SQLInputParameters.Count > 0)
                    IsOutstandingInvoices = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "OutstandingBalances").Select(x => x.InputValue).FirstOrDefault());

                if (SQLInputParameters.Count > 0 && ReportsUtility.ValidReportWithTemplates.Contains(Report_Id))
                {
                    IsSimplifiedView = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "Simplified").Select(x => x.InputValue).FirstOrDefault());
                }
            }
            DocumentFileInfo documentFileInfo = _IReportService.Download(FileType_Id, userSession.Client_Id, outputTablesList, Report_Id, ActionType, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate, IsAgentId_Include);
            if (!string.IsNullOrEmpty(documentFileInfo?.Base64String))
            {
                serviceResponse.Status = true;
                serviceResponse.Data = documentFileInfo;
            }
            else
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "There was an error while processing your request.";
            }
            return Ok(serviceResponse);
        }

        public IActionResult StatementView(long Report_Id, byte FileType_Id,string ActionType, List<LU_SQLInputParameters> SQLInputParameters)
        {
            int? totalRecords;
            string ReportHTMLWithTemplate = "";
            ViewBag.FileType_Id = FileType_Id;
            DateTime? AsOfDate = null;
            LU_ReportTemplates ReportTemplate = LookupUtility.GetAllReportTemplates().FirstOrDefault(x => x.Report_Id == Report_Id);

            var asOfDateValue = SQLInputParameters.Where(x => x.ParameterName == "As_Of_Date").Select(x => x.InputValue).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(asOfDateValue))
            {
                if (DateTime.TryParseExact(asOfDateValue, new[] { "MM/dd/yyyy", "M/d/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    AsOfDate = parsedDate;
                }
                else
                {
                    AsOfDate = DateTime.Now;
                }
            }
            bool IsOutstandingInvoices = false;
            if (SQLInputParameters.Count > 0)
                IsOutstandingInvoices = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "OutstandingBalances").Select(x => x.InputValue).First());

            bool IsSimplifiedView = false;
            if (SQLInputParameters.Count > 0 && ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectBilledAccountStatement)
            {
                IsSimplifiedView = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "Simplified").Select(x => x.InputValue).First());
            }
            List<ReportOutputTables> outputTablesList = new List<ReportOutputTables>();
            if (ReportTemplate != null)
            {
                outputTablesList = reportsUtility.GetReportOutputTablesWithoutTotalrecords(Report_Id, SQLInputParameters);
                ReportHTMLWithTemplate = _IReportService.BuildReportHtmlFromTemplate(Report_Id, userSession.Client_Id,  outputTablesList, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
                if (ActionType == "Email")
                {
                    DocumentFileInfo documentFileInfo = _IReportService.Download(MasterLookup._LUFileTypes.AgentSpecificPDF, userSession.Client_Id, outputTablesList, Report_Id, ActionType, ReportTemplate, true, true, DateTime.Now, true);
                    if (documentFileInfo != null && documentFileInfo.Bytes != null && documentFileInfo.Bytes.Length > 0)
                    {
                        ViewBag.StatementZipBase64 = Convert.ToBase64String(documentFileInfo.Bytes);
                        ViewBag.StatementZipName = documentFileInfo.Name;
                        ViewBag.StatementZipContentType = documentFileInfo.ContentType;
                    }
                }
                ViewBag.ReportHTMLWithTemplate = ReportHTMLWithTemplate;
                ViewBag.ReportTemplate = ReportTemplate;
                ViewBag.Report_Id = Report_Id;
                ViewBag.ActionType = ActionType;
                ViewBag.SQLInputParameters = JsonConvert.SerializeObject(SQLInputParameters);
            }

            return View(outputTablesList);
        }

        [HttpPost]
        public ServiceResponse EIBrokerStatementUpload(long Report_Id, string ActionType, byte FileType_Id, List<LU_SQLInputParameters> SQLInputParameters)
        {
            DocumentFileInfo commissionDocs = new DocumentFileInfo();
            if (Report_Id > 0 && FileType_Id > 0 && SQLInputParameters != null)
            {
                bool IsAgentId_Include = true;
                IActionResult serviceRes = DownloadReport(Report_Id, ActionType, FileType_Id, SQLInputParameters, IsAgentId_Include);

                if (serviceRes != null && serviceRes is OkObjectResult okResult && okResult.Value is ServiceResponse serviceRes2)
                {
                    if (serviceRes2.Data != null)
                    {
                        commissionDocs = serviceRes2.Data as DocumentFileInfo;
                    }
                }

            }

            ServiceResponse serviceResponse = new ServiceResponse();
            if (commissionDocs != null)
            {
                List<DocumentFileInfo> docs = ExtractDocuments(commissionDocs);
                if (docs != null && docs.Count > 0 && userSession?.Client_Id != null)
                {
                    EI_ConnectUtility eiUtility = new EI_ConnectUtility();
                    serviceResponse = eiUtility.CommissionDocumentsUpload(userSession.Client_Id, docs);
                }
                else
                {
                    serviceResponse.Message = "No Document Available";
                }
            }
            return serviceResponse;
        }
        [HttpPost]
        public async Task<JsonResult> EIAgentStatementEmail(string AgentDetailIds, long Report_Id, string SQLInputParameters, string EmailSubject, string EmailBody)
        {
            try
            {
                // Validate AgentDetailIds
                if (string.IsNullOrEmpty(AgentDetailIds))
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "No Agent Details found."
                    });
                }

                var agentDetailIdList = JsonConvert.DeserializeObject<List<long>>(AgentDetailIds);
                if (agentDetailIdList == null || !agentDetailIdList.Any())
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "No Agent Details found."
                    });
                }

                // Deserialize SQL parameters
                var sqlInputParameterList = JsonConvert.DeserializeObject<List<LU_SQLInputParameters>>(SQLInputParameters);
                if (sqlInputParameterList == null)
                {
                    sqlInputParameterList = new List<LU_SQLInputParameters>();
                }

                // Get Report Template
                LU_ReportTemplates ReportTemplate = LookupUtility.GetAllReportTemplates().FirstOrDefault(x => x.Report_Id == Report_Id);
                if (ReportTemplate == null)
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "Report template not found."
                    });
                }

                // Get Report Output Tables
                List<ReportOutputTables> outputTablesList = reportsUtility.GetReportOutputTablesWithoutTotalrecords(Report_Id, sqlInputParameterList);
                if (outputTablesList == null || outputTablesList.Count == 0)
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "No report data found."
                    });
                }

                // Your required values
                DateTime asofDate = DateTime.Now;
                bool IsOutstandingInvoices = true;
                bool IsSimplifiedView = true;

                // Get Agent Email Data
                EI_ConnectUtility eiUtility = new EI_ConnectUtility();
                serviceResponse = eiUtility.AgentEmails(userSession.Client_Id, agentDetailIdList);

                if (!serviceResponse.Status)
                {
                    return Json(new
                    {
                        Status = false,
                        Message = serviceResponse.Message
                    });
                }

                var responseObject = JsonConvert.DeserializeObject<dynamic>(serviceResponse.Message);
                List<AgentEmailData> agentEmailData = responseObject.ToObject<List<AgentEmailData>>();
                if (agentEmailData == null || !agentEmailData.Any())
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "No agent email data found."
                    });
                }

                // Generate individual Agent PDFs
                List<AgentStatementFile> agentFiles = _IReportService.GenerateAgentStatementPDFs(userSession.Client_Id, outputTablesList, Report_Id, ReportTemplate, IsOutstandingInvoices,
                                                    IsSimplifiedView, asofDate, true);

                if (agentFiles == null || !agentFiles.Any())
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "No agent statement PDFs were generated."
                    });
                }

                var agentEmailMappings = (from emailData in agentEmailData join file in agentFiles on emailData.AgentDetail_Id equals file.AgentReferenceId
                                          select new
                                          {
                                              AgentDetailId = emailData.AgentDetail_Id,
                                              AgentReferenceId = file.AgentReferenceId,
                                              Email = emailData.AgentContactEmail,
                                              FileName = file.FileName,
                                              FileBytes = file.FileBytes,
                                              AgencyName = file.AgencyName,
                                          }).ToList();

                var noAgentEmailExists = !agentEmailMappings.Any(x => !string.IsNullOrWhiteSpace(x.Email));
                if(noAgentEmailExists)
                {
                    return Json(new
                    {
                        Status = false,
                        Message = "No agent email address is available. The agent statement cannot be sent by email."
                    });
                }
                // Email utility
                EWS_EmailUtility emailUtility = new EWS_EmailUtility();
                string clientName = userSession.CompanyName;

                // Send each agent their corresponding PDF
                foreach (var agent in agentEmailMappings)
                {
                    string toEmail = string.Empty;
                    if (string.IsNullOrWhiteSpace(agent.Email))
                    {
                        continue;
                    }

                    var agentFile = agentFiles.FirstOrDefault(x => x.AgentReferenceId == agent.AgentDetailId);
                    if (agentFile == null)
                    {
                        continue;
                    }

                    if (agentFile.FileBytes == null || agentFile.FileBytes.Length == 0)
                    {
                        continue;
                    }

                    var attachments = new List<InLineAttachmentItem>();
                    attachments.Add(new InLineAttachmentItem
                    {
                        IsInline = false,
                        BinaryImage = agentFile.FileBytes,
                        Name = agentFile.FileName,
                        ContentID = null,
                    });

                    toEmail = agent.Email;
                    string subject = string.Empty;
                    if(!string.IsNullOrEmpty(EmailSubject))
                    {
                        subject = EmailSubject;
                    }
                    else
                    {
                        subject = $"Agent Statement - {agent?.AgencyName ?? string.Empty} - {DateTime.Now:MM/dd/yyyy}";
                    }
                    string message = AgentStatmentEmailBody(agent?.AgencyName ?? string.Empty, EmailBody);
                    string ccAddress = string.Empty;
                    string enableAgentStatementCCEmail = LookupUtility.GetApplicationSettings(MasterLookup._ApplicationSettings.EnableAgentStatementCCEmail);
                    if (!string.IsNullOrEmpty(enableAgentStatementCCEmail))
                    {
                        string agentStatementCCEmail = LookupUtility.GetApplicationSettings(MasterLookup._ApplicationSettings.AgentStatementCCEmail);
                        if (!string.IsNullOrEmpty(agentStatementCCEmail))
                        {
                            ccAddress = agentStatementCCEmail;
                        }
                    }

                    string bccAddress = string.Empty;
                    string enableAgentStatementBCCEmail = LookupUtility.GetApplicationSettings(MasterLookup._ApplicationSettings.EnableAgentStatementBCCEmail);
                    if (!string.IsNullOrEmpty(enableAgentStatementBCCEmail))
                    {
                        string agentStatementBCCEmail = LookupUtility.GetApplicationSettings(MasterLookup._ApplicationSettings.AgentStatementBCCEmail);
                        if (!string.IsNullOrEmpty(agentStatementBCCEmail))
                        {
                            bccAddress = agentStatementBCCEmail;
                        }
                    }

                    string redirectAgentStatementToEmails = LookupUtility.GetApplicationSettings(MasterLookup._ApplicationSettings.RedirectAgentStatementToEmails);
                    if (!string.IsNullOrEmpty(redirectAgentStatementToEmails))
                    {
                        string agentStatementToEmail = LookupUtility.GetApplicationSettings(MasterLookup._ApplicationSettings.AgentStatementToEmail);
                        if (!string.IsNullOrEmpty(agentStatementToEmail))
                        {
                            toEmail = agentStatementToEmail;
                        }
                    }

                    try
                    {
                        emailUtility.SendExceptionReport(subject, message, clientName, ccAddress, toEmail, bccAddress, attachments);
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            Status = false,
                            Message = $"Email failed for AgentDetail_Id {agent.AgentDetailId}. " + $"Email: {agent.Email}. " + $"File: {agent.FileName}. Error: {ex.Message}"
                        });
                    }

                }

                return Json(new
                {
                    Status = true,
                    Message = "Email sent successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        private string AgentStatmentEmailBody(string agencyName, string EmailBody)
        {
            try
            {
                string bodyContent = !string.IsNullOrEmpty(EmailBody) ? EmailBody : $@"Please find attached the Agent Statement for <strong>{agencyName}</strong> for your review.";
                var Clients = _accountRepository.GetClients(userSession.Client_Id);
                string ClientLogo = Convert.ToBase64String(Clients.Logo);
                string imgSrc = !string.IsNullOrEmpty(ClientLogo) ? $"data:image/jpg;base64,{ClientLogo}" : "";
                string message = $@"<html> 
                            <head> 
                                <title>Agent Statement</title> 
                            </head> 
                            <body style=""font-family:Arial, Helvetica, sans-serif; font-size:14px; color:#333333; line-height:1.6; margin:0; padding:20px;""> 
                                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""> 
                                    <tr> 
                                        <td align=""center""> 
                                            <table width=""800"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#ffffff; border:1px solid #dddddd; border-radius:6px;"">
                                            <table width=""800"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#ffffff; border:1px solid #dddddd; border-radius:6px; overflow:hidden;""> 
                                                <!-- Banner --> 
                                                <tr> 
                                                    <td style=""background:#F3F4F6; padding:18px 25px; border-bottom:3px solid #D1D5DB;""> 

                                                        <table width=""100%"" cellpadding=""0"" cellspacing=""0""> 
                                                            <tr> 
                                                                <!-- Logo --> 
                                                                <td width=""150"" align=""left"" valign=""middle""> 
                                                                    <img src=""{imgSrc}"" 
                                                                         alt=""Expert Insured"" 
                                                                         width=""150"" 
                                                                         style=""display:block; width:150px; height:auto;"" /> 
                                                                </td> 
                                                                <!-- Centered Heading --> 
                                                                <td align=""center"" valign=""middle"" style=""font-size:20px; font-weight:bold; color:#333333;""> 
                                                                    Agent Statement 
                                                                </td> 
                                                                <!-- Spacer --> 
                                                                <td width=""150""> 
                                                                    &nbsp; 
                                                                </td> 
                                                            </tr> 
                                                        </table> 
                                                    </td> 
                                                </tr> 
                                                <tr> 
                                                    <td style=""padding:25px; background:#ffffff;""> 

                                                        <p style=""margin-top:0;""> 
                                                            Hello, 
                                                        </p> 

                                                        <p> 
                                                            {bodyContent} 
                                                        </p> 

                                                    </td> 
                                                </tr> 
                                                <tr> 
                                                    <td style=""background:#E5E7EB; border-top:1px solid #D1D5DB; padding:15px; text-align:center; font-size:12px; color:#666666;""> 
                                                        Expert Insured Accounting - Notification 
                                                    </td> 
                                                </tr> 

                                            </table> 
                                        </td> 
                                    </tr> 
                                </table> 
                            </body> 
                            </html>";
                return message;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //[NonAction]
        //public DocumentFileInfo Download(byte FileType_Id, List<ReportOutputTables> reportOutputTables, long Report_Id, LU_ReportTemplates ReportTemplate = null, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null, bool IsAgentId_Include = false)
        //{
        //    DataSet ds = new DataSet();
        //    DataSet dsAgency = new DataSet();
        //    byte[]? content = null;
        //    var FileTypesList = LookupUtility.GetFileTypes();
        //    LU_FileTypes FileType = FileTypesList?.Where(c => c.FileType_Id == FileType_Id)?.FirstOrDefault();
        //    DocumentFileInfo documentFileInfo = new DocumentFileInfo();
        //    List<DocumentFileInfo> listDocumentFileInfo = new List<DocumentFileInfo>();
        //    if (FileType != null)
        //    {
        //        documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.{FileType.Extension}";
        //        documentFileInfo.FileType_Id = FileType.FileType_Id;
        //        int counter = 0;
        //        foreach (var reportOutputTable in reportOutputTables)
        //        {
        //            counter++;
        //            string tableName = reportOutputTable.Name;
        //            int count = reportOutputTables.Where(c => c.OutputDataTable.TableName == tableName)?.Count() ?? 0;
        //            if (count > 0)
        //            {
        //                tableName += $"_{counter}";
        //            }
        //            reportOutputTable.OutputDataTable.TableName = tableName;
        //            ds.Tables.Add(reportOutputTable.OutputDataTable.Copy());
        //        }
        //        if (FileType.FileType_Id == MasterLookup._LUFileTypes.Excel)
        //        {
        //            if (Report_Id == MasterLookup._Reports.DirectBill || Report_Id == MasterLookup._Reports.InsuredBill)
        //            {
        //                content = _IMSOfficeFileOptions.DownloadDirectBillExcel(ds, AsOfDate);
        //                documentFileInfo.Bytes = content;
        //                documentFileInfo.Base64String = Convert.ToBase64String(content);
        //                documentFileInfo.ContentType = FileType.ContentType;
        //            }
        //            else if (Report_Id == MasterLookup._Reports.AgencyBill || Report_Id == MasterLookup._Reports.AgencyDetailed)
        //            {
        //                content = _IMSOfficeFileOptions.DownloadSimplifiedAgencyBillExcel(ds, IsOutstandingInvoices, userSession.Client_Id, AsOfDate);
        //                documentFileInfo.Bytes = content;
        //                documentFileInfo.Base64String = Convert.ToBase64String(content);
        //                documentFileInfo.ContentType = FileType.ContentType;
        //            }
        //            else
        //            {
        //                content = _IMSOfficeFileOptions.DownloadExcel(ds, reportOutputTables);
        //                documentFileInfo.Bytes = content;
        //                documentFileInfo.Base64String = Convert.ToBase64String(content);
        //                documentFileInfo.ContentType = FileType.ContentType;
        //            }
        //        }
        //        else if (FileType.FileType_Id == MasterLookup._LUFileTypes.CSV)
        //        {
        //            List<DocumentFileInfo> documentFileInfoList = _ICSVOptions.DownloadCSV(ds);
        //            if (documentFileInfoList?.Count == 1)
        //            {
        //                content = documentFileInfoList.First().Bytes;
        //                documentFileInfo.ContentType = FileType.ContentType;
        //            }
        //            else
        //            {
        //                content = _IZip.Compress(documentFileInfoList);
        //                documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.zip";
        //                documentFileInfo.ContentType = "application/zip";
        //            }
        //            documentFileInfo.Bytes = content;
        //            documentFileInfo.Base64String = Convert.ToBase64String(content);
        //        }
        //        else if (FileType.FileType_Id == MasterLookup._LUFileTypes.PDF ||
        //            FileType.FileType_Id == MasterLookup._LUFileTypes.AgentSpecificPDF)
        //        {
        //            if (ds?.Tables.Count > 0)
        //            {
        //                if (ReportTemplate != null)
        //                {
        //                    try
        //                    {
        //                        if (FileType.FileType_Id == MasterLookup._LUFileTypes.AgentSpecificPDF)
        //                        {
        //                            var tableAgency = reportOutputTables[0];

        //                            var tablePolicies = reportOutputTables[1];

        //                            var tableInvoices = reportOutputTables[2];

        //                            var table4 = reportOutputTables[3];

        //                            var agencyGroups = tableAgency.DataTable.AsEnumerable()
        //                                .GroupBy(r => r.Field<long>("AgentDetail_Id"));

        //                            List<(string fileName, byte[] fileBytes)> pdfFiles = new List<(string, byte[])>();

        //                            foreach (var agencyGroup in agencyGroups)
        //                            {

        //                                long agentDetailId = agencyGroup.Key;

        //                                var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
        //                                    .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId)
        //                                    .ToList();

        //                                if (!policiesForAgent.Any())
        //                                    continue;

        //                                var validPolicyIds = table4.DataTable.AsEnumerable()
        //                                    .Where(r => !r.IsNull("PolicyDetails_Id"))
        //                                    .Select(r => r.Field<long>("PolicyDetails_Id"))
        //                                    .ToHashSet();

        //                                var filteredPolicies = policiesForAgent
        //                                    .Where(r => validPolicyIds.Contains(r.Field<long>("Id")))
        //                                    .ToList();

        //                                if (!filteredPolicies.Any())
        //                                    continue;

        //                                var policiesTableClone = tablePolicies.DataTable.Clone();
        //                                foreach (var row in filteredPolicies)
        //                                    policiesTableClone.ImportRow(row);

        //                                var policyNos = filteredPolicies
        //                                    .Select(r => r.Field<string>("PolicyNumber"))
        //                                    .ToHashSet();

        //                                var policyIds = filteredPolicies
        //                                    .Select(r => r.Field<long>("Id"))
        //                                    .ToHashSet();

        //                                var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
        //                                    .Where(r => !r.IsNull("AgentDetail_Id") &&
        //                                                r.Field<long>("AgentDetail_Id") == agentDetailId &&
        //                                                !r.IsNull("PolicyNumber") &&
        //                                                policyNos.Contains(r.Field<string>("PolicyNumber")))
        //                                    .ToList();

        //                                if (!invoicesForAgent.Any())
        //                                    continue;

        //                                var invoicesTableClone = tableInvoices.DataTable.Clone();
        //                                foreach (var row in invoicesForAgent)
        //                                    invoicesTableClone.ImportRow(row);

        //                                var filteredTable4Rows = table4.DataTable.AsEnumerable()
        //                                    .Where(r => !r.IsNull("PolicyDetails_Id") && policyIds.Contains(r.Field<long>("PolicyDetails_Id")))
        //                                    .ToList();

        //                                if (!filteredTable4Rows.Any())
        //                                    continue;

        //                                var table4Clone = table4.DataTable.Clone();
        //                                foreach (var row in filteredTable4Rows)
        //                                    table4Clone.ImportRow(row);

        //                                var agencyTableClone = tableAgency.DataTable.Clone();
        //                                foreach (var row in agencyGroup)
        //                                    agencyTableClone.ImportRow(row);

        //                                var agencyOutputTables = new List<ReportOutputTables>
        //                                {
        //                                    new ReportOutputTables { DataTable = agencyTableClone },
        //                                    new ReportOutputTables { DataTable = policiesTableClone },
        //                                    new ReportOutputTables { DataTable = invoicesTableClone },
        //                                    new ReportOutputTables { DataTable = table4Clone }
        //                                };

        //                                if (table4Clone.Rows.Count > 0)
        //                                {
        //                                    // Build HTML
        //                                    Clients clients = _accountRepository.GetClients(ReportTemplate.Client_Id);
        //                                    string htmlContent = BuildReportHtmlFromTemplate(Report_Id, agencyOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
        //                                    if (htmlContent != "")
        //                                    {
        //                                        string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
        //                                        string year = DateTime.Now.Year.ToString();

        //                                        string headerHtml = $@"<table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;"">
        //                            <tbody><tr><td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt;"">{month} {year}</td>
        //                            <td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt;"">{ReportTemplate.Name}</td></tr></tbody></table>
        //                            <style>body {{margin: 0px;padding: 0px;}}</style>";

        //                                        string footerHtml = @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>";

        //                                        var request = new PDFRequest
        //                                        {
        //                                            BodyHTMLString = htmlContent,
        //                                            HeaderHTMLString = headerHtml,
        //                                            FooterHTMLString = footerHtml,
        //                                            IsCoverPageExcluded = false
        //                                        };

        //                                        string serviceUrl = MasterLookup._PDFCoverterAPIURL;
        //                                        ServiceLogInfo logInfo = new ServiceLogInfo();
        //                                        ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(logInfo, serviceUrl, request, typeof(ServiceResponse), true);

        //                                        if (response.Status && response.Data != null)
        //                                        {
        //                                            byte[] pdfBytes = Convert.FromBase64String(response.Data.ToString());
        //                                            string agencyName = agencyGroup.First().Field<string>("Name")?.Replace(" ", "_") ?? $"Agency_{agentDetailId}";
        //                                            var Agent_ID = _policyDetailsRepository.GetAgentDetails(agentDetailId).AgentReference_Id;
        //                                            //string fileName = $"{agencyName}_{ReportTemplate.Name}_{DateTime.Now:yyyyMMddHHmmss}_{Agent_ID}.pdf";                                                  
        //                                            var datePart = (AsOfDate ?? DateTime.Now).ToString("yyyyMMdd");
        //                                            string fileName = "";
        //                                            if (IsAgentId_Include)
        //                                            {
        //                                                fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}_{Agent_ID}.pdf";
        //                                            }
        //                                            else
        //                                            {
        //                                                fileName = $"{ReportTemplate.Name}_{datePart}_{agencyName}.pdf";
        //                                            }
        //                                            pdfFiles.Add((fileName, pdfBytes));
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            if (pdfFiles?.Count == 1)
        //                            {
        //                                foreach (var file in pdfFiles)
        //                                {
        //                                    listDocumentFileInfo.Add(new DocumentFileInfo
        //                                    {
        //                                        Name = file.fileName,
        //                                        Base64String = file.fileBytes.ToString(),
        //                                        Bytes = file.fileBytes,
        //                                        ContentType = "application/pdf"

        //                                    });
        //                                }

        //                                content = _IZip.Compress(listDocumentFileInfo);
        //                                documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.zip";
        //                                documentFileInfo.Bytes = content;
        //                                documentFileInfo.Base64String = Convert.ToBase64String(content);
        //                                documentFileInfo.ContentType = "application/zip";
        //                            }
        //                            else
        //                            {
        //                                foreach (var file in pdfFiles)
        //                                {
        //                                    listDocumentFileInfo.Add(new DocumentFileInfo
        //                                    {
        //                                        Name = file.fileName,
        //                                        Base64String = file.fileBytes.ToString(),
        //                                        Bytes = file.fileBytes,
        //                                        ContentType = "application/pdf"

        //                                    });
        //                                }

        //                                content = _IZip.Compress(listDocumentFileInfo);
        //                                documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.zip";
        //                                documentFileInfo.Bytes = content;
        //                                documentFileInfo.Base64String = Convert.ToBase64String(content);
        //                                documentFileInfo.ContentType = "application/zip";
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //Clients clients = _accountRepository.GetClients(ReportTemplate.Client_Id);
        //                            var HtmlContent = BuildReportHtmlFromTemplate(Report_Id, reportOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
        //                            string Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
        //                            string Year = DateTime.Now.Year.ToString();
        //                            string HeaderHTMLString = @" <table style=""width: 100%; background: #085284; font-family: 'Poppins', sans-serif;""><tbody><tr><td style=""padding: 10pt 0pt; padding-left: 20pt; color: #fff; font-size: 18pt; "">" + Month + @" " + Year + @" </td><td style=""padding: 10pt 0pt; padding-right: 20pt; text-align: right; color: #fff; font-size: 18pt; "">" + ReportTemplate.Name + "</td></tr></tbody></table><style>body {margin: 0px;padding: 0px;}</style>";

        //                            //pDFConverterController.SetFooterForHtmlConverstion(PDFConverter.Document, 30, @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>", null);
        //                            string FooterHTMLString = @"<div style=""text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;"">{CrtPage}</div>";

        //                            //byte[] PDFBytes = pDFConverterController.ConvertHtmlToPDF(PDFConverter, RenderedHtml);
        //                            bool IsCoverPageExcluded = false;
        //                            PDFRequest request = new PDFRequest()
        //                            {
        //                                BodyHTMLString = HtmlContent,
        //                                HeaderHTMLString = HeaderHTMLString,
        //                                FooterHTMLString = FooterHTMLString,
        //                                IsCoverPageExcluded = IsCoverPageExcluded
        //                            };
        //                            string Service_Url = MasterLookup._PDFCoverterAPIURL;
        //                            ServiceLogInfo serviceLogInfo = new ServiceLogInfo();
        //                            ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, request, typeof(ServiceResponse), true);
        //                            if (response.Status)
        //                            {
        //                                documentFileInfo.Name = $"Report-{ReportTemplate.Name} -{DateTime.Now.Ticks}.pdf";
        //                                documentFileInfo.Base64String = response.Data.ToString();
        //                                documentFileInfo.ContentType = "application/pdf";
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                    }
        //                }
        //                else
        //                {
        //                    List<DocumentFileInfo> documentFileInfoList = new List<DocumentFileInfo>();
        //                    foreach (DataTable table in ds?.Tables)
        //                    {
        //                        var viewDataDictionary = new ViewDataDictionary
        //                            (
        //                    new EmptyModelMetadataProvider(),
        //                         new ModelStateDictionary()
        //                            )
        //                        {
        //                            Model = table,
        //                        };
        //                        viewDataDictionary.Add("FontSize", 6);
        //                        var htmlContent = this.view.Render("Reports/ReportPDF", viewDataDictionary);
        //                        List<string> CSSReferenceList = new List<string> { "css/Reports/ReportPDF.css" };
        //                        byte[] bytes = _IPDFOptions.DownloadPDF(htmlContent, _IWebHostEnvironment.WebRootPath, CSSReferenceList);
        //                        if (bytes != null && bytes.Length > 0)
        //                        {
        //                            DocumentFileInfo fileInfo = new DocumentFileInfo();
        //                            fileInfo.Bytes = bytes;
        //                            fileInfo.Name = $"{table.TableName}.pdf";
        //                            documentFileInfoList.Add(fileInfo);
        //                        }
        //                        //content = _IPDFOptions.DownloadPDF(htmlContent, _IWebHostEnvironment.WebRootPath, CSSReferenceList);
        //                    }
        //                    if (documentFileInfoList?.Count == 1)
        //                    {
        //                        content = documentFileInfoList.First().Bytes;
        //                        documentFileInfo.ContentType = FileType.ContentType;
        //                    }
        //                    else
        //                    {
        //                        content = _IZip.Compress(documentFileInfoList);
        //                        documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.zip";
        //                        documentFileInfo.ContentType = "application/zip";
        //                    }
        //                    documentFileInfo.Bytes = content;
        //                    documentFileInfo.Base64String = Convert.ToBase64String(content);
        //                }

        //            }
        //        }
        //    }

        //    return documentFileInfo;
        //}

        [NonAction]
        public List<DocumentFileInfo> ExtractDocuments(DocumentFileInfo fileInfo)
        {
            List<DocumentFileInfo> documents = new List<DocumentFileInfo>();

            if (fileInfo == null || fileInfo.Bytes == null || fileInfo.Bytes.Length == 0)
                return documents;

            if (fileInfo.ContentType == "application/zip" || fileInfo.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using (MemoryStream zipStream = new MemoryStream(fileInfo.Bytes))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        using (var entryStream = entry.Open())
                        using (var ms = new MemoryStream())
                        {
                            entryStream.CopyTo(ms);
                            byte[] fileBytes = ms.ToArray();
                            string[] parts = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                            long agentId = 0;
                            if (parts.Length > 0 && long.TryParse(parts[^1], out var parsedId))
                                agentId = parsedId;
                            documents.Add(new DocumentFileInfo
                            {
                                Name = entry.Name,
                                Bytes = fileBytes,
                                Base64String = Convert.ToBase64String(fileBytes),
                                ContentType = CommonUtility.GetMimeType(entry.Name),
                                Agent_Id = agentId
                            });
                        }
                    }
                }
            }
            else
            {
                documents.Add(fileInfo);
            }

            return documents;
        }
    }
}
