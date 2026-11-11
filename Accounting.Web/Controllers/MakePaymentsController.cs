using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.External.QBO.Vendor;
using Accounting.Entities.External.Sage.Journal;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.EI_Connect;
using Accounting.Utilities.External.QBO;
using Accounting.Utilities.External.Sage;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Accounting.Web.Controllers
{
    public class MakePaymentsController : BaseController
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
        private readonly IClientsRepository _ClientsRepository;
        public MakePaymentsController(IUsersRepository userRepository, IReportsRepository iReportsRepository, ILU_SQLQueriesRepository iSQLQueriesRepository,
            ILU_SQLInputParametersRepository iSQLInputParametersRepository, IReportFileDownloadSettingsRepository iReportFileDownloadSettingsRepository,
            IMSOfficeFileOptions IMSOfficeFileOptions, ICSVOptions ICSVOptions, IZip IZip, IWebHostEnvironment IWebHostEnvironment, ICSVOptions iCSVOptions,
            IZip iZip, IWebHostEnvironment iWebHostEnvironment, IPDFOptions iPDFOptions, ViewRender view, IUserContextRepository userContextRepository, IClientsRepository clientsRepository)
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
            _ClientsRepository = clientsRepository;
        }

        public IActionResult Index()
        {
            try
            {
                List<LU_LineItems_Clients> ClientLineList = _lookupRepository.GetLineItems_ClientsByClient_Id(userSession.Client_Id);
                var ClientLineItemIds = ClientLineList?.Select(x => x.LineItem_Id)?.ToList();
                ViewBag.ClientLineItemIds = ClientLineItemIds;
                ViewBag.ClientDirections = _lookupRepository.GetAllDirections().Where(d => d.Direction_Id == MasterLookup._Directions.AR || d.Direction_Id == MasterLookup._Directions.AP);
                var Report_Id = MasterLookup._Reports.DirectBill;
                var FileType_Id = MasterLookup._LUFileTypes.AgentSpecificPDF;
                ViewBag.Report_Id = Report_Id;
                ViewBag.FileType_Id = FileType_Id;
                var isSuperAdmin = userSession?.UserRoleMappingList?.Any(x => x.Role_Id == MasterLookup._Roles.SuperAdmin) ?? false;
                ViewBag.IsSuperAdmin = isSuperAdmin;
                var Clients = userSession.Client_Id;
                if (!Is_AjaxRequest)
                    return View();
                return PartialView();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IActionResult CreatePayeeBill(int EntityType_Id, int Entity_Id, int Invoice_Id, string Direction_Id, string EntityName)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                StateDetails statedetails = new StateDetails();
                SqlTransaction EntityTransaction = null;
                EntityTransaction = _baseRepository.GetNewTransaction();
                if ((EntityType_Id > 0 && Entity_Id > 0 ) || (EntityType_Id > 0 && EntityType_Id == MasterLookup._EntityTypes.State && Entity_Id == 0))
                {
                    long Client_Id = userSession.Client_Id;
                    if ((!string.IsNullOrEmpty(EntityName) && EntityType_Id == MasterLookup._EntityTypes.State))
                    {
                        EntityName = $"State of {EntityName}";
                        statedetails = _policyDetailsRepository.GetStateDetailsByName(EntityName, Client_Id);
                        if (statedetails == null)
                        {
                            statedetails = new StateDetails();  
                            statedetails.Client_Id = Client_Id;
                            statedetails.Name = EntityName;
                            statedetails = _policyDetailsRepository.InsertUpdateStateDetails(EntityTransaction, statedetails);
                            EntityTransaction.Commit();
                        }
                    }
                    EntityDetails Entity = _makepaymentRepository.GetEntityDetailById(Client_Id, EntityType_Id, Entity_Id, EntityName);
                    List<PaymentsEntity> EntityInvoice = _makepaymentRepository.GetInvoiceLineItemsByIds(Client_Id, Invoice_Id);
                    var BillDetail = new
                    {
                        EntityDetail = Entity,
                        EntityInvoice
                    };
                    serviceResponse.Status = true;
                    serviceResponse.Data = BillDetail;
                }
                else
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Entity Detail is missing!";
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return Ok(serviceResponse);
        }
        public IActionResult CreateBill(MakePayments makePayments)
        {
            var Report_Id = MasterLookup._Reports.DirectBill;
            var FileType_Id = MasterLookup._LUFileTypes.AgentSpecificPDF;
            ViewBag.Report_Id = Report_Id;
            ViewBag.FileType_Id = FileType_Id;
            ViewBag.ClientID = userSession.Client_Id;
            long Clients = userSession.Client_Id;
            var AgentInvoice = _makepaymentRepository.GetAgencyInvoices(Clients, makePayments);
            if (!Is_AjaxRequest)
                return View(AgentInvoice);
            return PartialView(AgentInvoice);
        }
        public IActionResult GetAllEntities(PaymentsEntity paymentEntity, int pageIndex, int pageSize)
        {
            var Clients = userSession.Client_Id;
            var Agencies = _makepaymentRepository.GetInvoicedEntity(Clients, paymentEntity, pageIndex, pageSize);
            if (!Is_AjaxRequest)
                return View(Agencies);
            return PartialView("_EntityInfo", Agencies);

        }
        public IActionResult InvoiceTable(MakePayments makePayments)
        {
            long Clients = userSession.Client_Id;
            string EntityName = makePayments.EntityName;
            if (makePayments.PayeeType_Id == MasterLookup._EntityTypes.State)
            {
                EntityName = makePayments.EntityName.Replace("State of ", "");
            }
            makePayments.EntityName = EntityName;

            var AgentInvoice = _makepaymentRepository.GetAgencyInvoices(Clients, makePayments);
            ViewBag.EntityID = makePayments.Payee_Id;
            ViewBag.Direction_Id = makePayments.Direction_Id;
            if (!Is_AjaxRequest)
                return View(AgentInvoice);
            return PartialView("_InvoiceTable", AgentInvoice);
        }
        public IActionResult MakePaymentsGrid()
        {
            return PartialView(_PaymentsList);
        }
        public IActionResult MakePaymentDetails(long? Payment_Id)
        {
            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id ?? 0);
            return PartialView(payments);
        }
        public string BuildReportHtmlFromTemplate(long Report_Id, List<ReportOutputTables> reportOutputTablesList, LU_ReportTemplates ReportTemplate, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null)
        {
            var Clients = _accountRepository.GetClients(userSession.Client_Id);
            string FinalHTMLAlongWithTemplate = "";
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

                        //if (policiesToRemove.Any())
                        //{
                        //    foreach (var row in policiesTableTemp.AsEnumerable().ToList())
                        //    {
                        //        if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                        //            policiesTableTemp.Rows.Remove(row);
                        //    }

                        //    foreach (var row in invoicesTableTemp.AsEnumerable().ToList())
                        //    {
                        //        if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                        //            invoicesTableTemp.Rows.Remove(row);
                        //    }

                        //    foreach (var row in table4Temp.AsEnumerable().ToList())
                        //    {
                        //        if (!row.IsNull("PolicyNumber") && policiesToRemove.Contains(row.Field<string>("PolicyNumber")))
                        //            table4Temp.Rows.Remove(row);
                        //    }
                        //}

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
                    ViewBag.IsSimplifiedView = IsSimplifiedView;
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
                if (table4Final.Rows.Count > 0)
                {
                    List<ReportOutputTables> agencyOutputTables = new()
{
    new() { DataTable = agencyTableFinal },
    new() { DataTable = policiesTableFinal },
    new() { DataTable = invoicesTableFinal },
    new() { DataTable = table4Final }
};



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

                    DynamicReportHTMLContent = this.view.Render("Reports/AgencyBilledAccountStatementDynamicReport", viewDataDictionary);
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
            else
            {
                FinalHTMLAlongWithTemplate = "";
            }
            return FinalHTMLAlongWithTemplate;
        }
        [NonAction]
        public DocumentFileInfo Download(byte FileType_Id, List<ReportOutputTables> reportOutputTables, long Report_Id, LU_ReportTemplates ReportTemplate = null, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null, bool IsAgentId_Include = false, List<SelectedInvoice> selectedInvoices = null)
        {
            if (selectedInvoices == null || !selectedInvoices.Any())
                return new DocumentFileInfo();
            var selectedInvoiceIds = new HashSet<long>(selectedInvoices.Select(x => x.Invoice_Id));

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
                if (FileType.FileType_Id == MasterLookup._LUFileTypes.Excel)
                {
                    if (Report_Id == MasterLookup._Reports.DirectBill)
                    {
                        content = _IMSOfficeFileOptions.DownloadDirectBillExcel(ds, AsOfDate);
                        documentFileInfo.Bytes = content;
                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                    else
                    {
                        content = _IMSOfficeFileOptions.DownloadExcel(ds, reportOutputTables);
                        documentFileInfo.Bytes = content;
                        documentFileInfo.Base64String = Convert.ToBase64String(content);
                        documentFileInfo.ContentType = FileType.ContentType;
                    }
                }
                else if (FileType.FileType_Id == MasterLookup._LUFileTypes.CSV)
                {
                    List<DocumentFileInfo> documentFileInfoList = _ICSVOptions.DownloadCSV(ds);
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
                                    var tableAgency = reportOutputTables[0];

                                    var tablePolicies = reportOutputTables[1];

                                    var tableInvoices = reportOutputTables[2];

                                    var table4 = reportOutputTables[3];

                                    var agencyGroups = tableAgency.DataTable.AsEnumerable()
                                        .GroupBy(r => r.Field<long>("AgentDetail_Id"));

                                    List<(string fileName, byte[] fileBytes)> pdfFiles = new List<(string, byte[])>();

                                    foreach (var agencyGroup in agencyGroups)
                                    {

                                        long agentDetailId = agencyGroup.Key;

                                        var policiesForAgent = tablePolicies.DataTable.AsEnumerable()
                                            .Where(r => !r.IsNull("AgentDetail_Id") && r.Field<long>("AgentDetail_Id") == agentDetailId)
                                            .ToList();

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

                                        var invoicesForAgent = tableInvoices.DataTable.AsEnumerable()
                                            .Where(r => !r.IsNull("AgentDetail_Id") &&
                                                        r.Field<long>("AgentDetail_Id") == agentDetailId &&
                                                        !r.IsNull("PolicyNumber") &&
                                                        policyNos.Contains(r.Field<string>("PolicyNumber")))
                                            .ToList();

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

                                        var filteredRows = table4Clone.AsEnumerable().Where(row => selectedInvoiceIds.Contains(row.Field<long>("Invoice_Id")));

                                        DataTable filteredTable;

                                        if (filteredRows.Any())
                                        {
                                            filteredTable = filteredRows.CopyToDataTable();
                                        }
                                        else
                                        {
                                            filteredTable = table4Clone.Clone();
                                        }

                                        var agencyOutputTables = new List<ReportOutputTables>
                                        {
                                            new ReportOutputTables { DataTable = agencyTableClone },
                                            new ReportOutputTables { DataTable = policiesTableClone },
                                            new ReportOutputTables { DataTable = invoicesTableClone },
                                            new ReportOutputTables { DataTable = filteredTable }
                                        };

                                        if (table4Clone.Rows.Count > 0)
                                        {
                                            // Build HTML
                                            Clients clients = _accountRepository.GetClients(ReportTemplate.Client_Id);
                                            string htmlContent = BuildReportHtmlFromTemplate(Report_Id, agencyOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
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
                                                ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(logInfo, serviceUrl, request, typeof(ServiceResponse), true);

                                                if (response.Status && response.Data != null)
                                                {
                                                    byte[] pdfBytes = Convert.FromBase64String(response.Data.ToString());
                                                    string agencyName = agencyGroup.First().Field<string>("Name")?.Replace(" ", "_") ?? $"Agency_{agentDetailId}";
                                                    var Agent_ID = _policyDetailsRepository.GetAgentDetails(agentDetailId).AgentReference_Id;
                                                    //string fileName = $"{agencyName}_{ReportTemplate.Name}_{DateTime.Now:yyyyMMddHHmmss}_{Agent_ID}.pdf";                                                  
                                                    var datePart = (AsOfDate ?? DateTime.Now).ToString("yyyyMMdd");
                                                    string fileName = "";
                                                    if (IsAgentId_Include)
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
                                else
                                {
                                    //Clients clients = _accountRepository.GetClients(ReportTemplate.Client_Id);
                                    var HtmlContent = BuildReportHtmlFromTemplate(Report_Id, reportOutputTables, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate);
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
                                    ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, request, typeof(ServiceResponse), true);
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
        public IActionResult DownloadReport(long Report_Id, byte FileType_Id, List<LU_SQLInputParameters> SQLInputParameters, bool IsAgentId_Include, List<SelectedInvoice> selectedInvoices)
        {
            int? totalRecords;

            int PageSize = Convert.ToInt32(HttpContext.Request.Query["PageSize"]);
            int PageIndex = Convert.ToInt32(HttpContext.Request.Query["PageIndex"]);
            int? TotalRecords = 0;
            long Client_Id = userSession.Client_Id;
            if (Report_Id == MasterLookup._Reports.InvoiceProduction || Report_Id == MasterLookup._Reports.Payment
                || Report_Id == MasterLookup._Reports.Transaction || Report_Id == MasterLookup._Reports.AccountReceivable
                || Report_Id == MasterLookup._Reports.AccountPayable || Report_Id == MasterLookup._Reports.TaxFiling)
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

                if (SQLInputParameters.Count > 0 && ReportTemplate.ReportTemplate_Id == MasterLookup._ReportTemplates.DirectBilledAccountStatement)
                {
                    IsSimplifiedView = Convert.ToBoolean(SQLInputParameters.Where(x => x.ParameterName == "Simplified").Select(x => x.InputValue).FirstOrDefault());
                }
            }
            DocumentFileInfo documentFileInfo = Download(FileType_Id, outputTablesList, Report_Id, ReportTemplate, IsOutstandingInvoices, IsSimplifiedView, AsOfDate, IsAgentId_Include, selectedInvoices);
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

        //[HttpPost]
        //public ServiceResponse ConfirmBillCreation(long Report_Id, byte FileType_Id, List<SelectedInvoice> selectedInvoices, byte Direction_Id, Accounting.Entities.DB.Transactions.Bill billDetail)
        //{
        //    ServiceResponse serviceResponse = new ServiceResponse();
        //    bool IsPostToAgentPortal = billDetail.PostToAgentPortal;
        //    List<LU_SQLInputParameters> SQLInputParameters = new List<LU_SQLInputParameters>();
        //    SqlTransaction sqlTransaction = null;
        //    sqlTransaction = _baseRepository.GetNewTransaction();
        //    DateTime? AsOfDate = DateTime.Now;
        //    bool IsOutstandingInvoices = true;
        //    bool IsSimplifiedView = true;
        //    long Payment_Id = 0;
        //    List<SelectedInvoiceDetailDTO> invoiceDetailsList = new List<SelectedInvoiceDetailDTO>();
        //    Clients clients = _accountRepository.GetClients(userSession.Client_Id);
        //    if (selectedInvoices == null || selectedInvoices?.Count == 0)
        //    {
        //        serviceResponse.Status = false;
        //        serviceResponse.Message = "No Invoices selected for bill.";
        //        return serviceResponse;

        //    }
        //    foreach (var item in selectedInvoices)
        //    {
        //        bool isPartial = item.AppliedAmount != item.RemainingBalance;
        //        int ManipulateSign = Direction_Id == MasterLookup._Directions.AR ? -1 : 1;
        //        SelectedInvoiceDetailDTO dto = new SelectedInvoiceDetailDTO
        //        {
        //            Invoice_Id = item.Invoice_Id,
        //            Amount = item.AppliedAmount * ManipulateSign,
        //            RemainingBalance = item.RemainingBalance * ManipulateSign,
        //            IsPartial = isPartial
        //        };

        //        if (isPartial)
        //        {
        //            dto.InvoiceDetail_Id = 0;
        //            dto.InvoiceDetail_Ids = new List<long> { item.InvoiceLineItem_Id };
        //        }
        //        else
        //        {
        //            dto.InvoiceDetail_Id = item.InvoiceLineItem_Id;
        //            dto.InvoiceDetail_Ids = null;
        //        }

        //        invoiceDetailsList.Add(dto);
        //    }

        //    if (sqlTransaction == null)
        //    {
        //        serviceResponse.Status = false;
        //        serviceResponse.Data = "Unable to initiate a Transaction for the process.";
        //        return serviceResponse;
        //    }
        //    try
        //    {
        //        SQLInputParameters.AddRange(new List<LU_SQLInputParameters>
        //        {
        //            new() {
        //                ParameterName = "As_Of_Date",
        //                InputValue = AsOfDate?.ToString("MM-dd-yyyy")
        //            },
        //            new() {
        //                ParameterName = "OutstandingBalances",
        //                InputValue =IsOutstandingInvoices.ToString().ToLower()
        //            },
        //            new() {
        //                ParameterName = "Simplified",
        //                InputValue = IsSimplifiedView.ToString().ToLower()
        //            }
        //        });

        //        DocumentFileInfo commissionDocs = new DocumentFileInfo();
        //        var invoiceLineItemPairs = selectedInvoices
        //                   ?.Select(i => new { i.Invoice_Id, i.InvoiceLineItem_Id, i.LineItem_Id })
        //                   ?.Distinct();
        //        if (invoiceLineItemPairs != null)
        //        {
        //            foreach (var item in invoiceLineItemPairs)
        //            {
        //                //long invoice_Id = selectedInvoices.Select(i => i.Invoice_Id).FirstOrDefault();
        //                //byte lineItem_Id = selectedInvoices.Select(i => i.LineItem_Id).FirstOrDefault();
        //                var result = _paymentsBillsMappingRepository.GetExternalReferenceLineItems(billDetail.Entity_Id, billDetail.EntityType_Id, item.Invoice_Id, item.InvoiceLineItem_Id, item.LineItem_Id, Direction_Id, userSession.Client_Id);
        //                if (result != null && result.IsBillFullyApplied)
        //                {
        //                    serviceResponse.Status = false;
        //                    serviceResponse.Message += $"Bill fully paid with same InvoiceNo and LineItem (Invoice Id: {item.Invoice_Id} invoice detail id: {item.InvoiceLineItem_Id}";
        //                    return serviceResponse;
        //                }
        //            }
        //        }
        //        if (IsPostToAgentPortal)
        //        {
        //            if (Report_Id > 0 && FileType_Id > 0 && SQLInputParameters != null)
        //            {
        //                bool IsAgentId_Include = true;
        //                IActionResult serviceRes = DownloadReport(Report_Id, FileType_Id, SQLInputParameters, IsAgentId_Include, selectedInvoices);

        //                if (serviceRes != null && serviceRes is OkObjectResult okResult && okResult.Value is ServiceResponse serviceRes2)
        //                {
        //                    if (serviceRes2 != null && serviceRes2.Data != null)
        //                    {
        //                        commissionDocs = serviceRes2.Data as DocumentFileInfo;
        //                    }
        //                }

        //            }

        //            if (commissionDocs != null)
        //            {
        //                List<DocumentFileInfo> docs = ExtractDocuments(commissionDocs);

        //                if (docs != null && docs.Count > 0 && userSession?.Client_Id != null)
        //                {
        //                    EI_ConnectUtility eiUtility = new();
        //                    serviceResponse = eiUtility.CommissionDocumentsUpload(userSession.Client_Id, docs);
        //                }
        //                else
        //                {
        //                    serviceResponse.Message = "No Document Available";
        //                }
        //            }
        //        }
        //        Payments_BillsMapping payments_BillsMapping = new Payments_BillsMapping();
        //        ExternalVendorEntityReference externalVendorEntityReference = new ExternalVendorEntityReference();
        //        Accounting.Entities.DB.Transactions.Bill bill = new Accounting.Entities.DB.Transactions.Bill();
        //        if (billDetail.EntityType_Id == MasterLookup._EntityTypes.State)
        //        {
        //            StateDetails stateDetails = _policyDetailsRepository.GetStateDetailsByName($"State of {billDetail.EntityName}", userSession.Client_Id);
        //            if (stateDetails != null)
        //            {
        //                billDetail.Entity_Id = stateDetails.StateDetail_Id;
        //            }
        //            else
        //            {
        //                stateDetails = new StateDetails();
        //                stateDetails.Name = $"State of {billDetail.EntityName}"; //billDetail.EntityName;
        //                stateDetails.Client_Id = userSession.Client_Id;
        //                stateDetails = _policyDetailsRepository.InsertUpdateStateDetails(sqlTransaction, stateDetails);
        //                billDetail.Entity_Id = stateDetails.StateDetail_Id;
        //            }
        //        }
        //        bill.EntityName = billDetail.EntityName;
        //        string Entityname = bill.EntityName.Replace("State of ", "");
        //        bill.EntityName = Entityname;
        //        Payments OutgoingPayment = new Payments();
        //        ExternalBillReference externalBillReference = new ExternalBillReference();

        //        if ((billDetail.EntityType_Id > 0 && billDetail.Entity_Id > 0) || !string.IsNullOrEmpty(billDetail.EntityName))
        //        {

        //            Accounting.Entities.DB.Transactions.Bill InsertBill = _makepaymentRepository.InsertUpdateBill(sqlTransaction, billDetail);

        //            if (InsertBill.Bill_Id > 0)
        //            {
        //                LineItemBill billLineItem = new LineItemBill();
        //                foreach (var item in selectedInvoices)
        //                {
        //                    billLineItem.Bill_Id = InsertBill.Bill_Id;
        //                    billLineItem.Invoice_Id = item.Invoice_Id;
        //                    billLineItem.LineItem_Id = item.LineItem_Id;
        //                    billLineItem.InvoiceLineItem_Id = item.InvoiceLineItem_Id;
        //                    billLineItem.AmountApplied = item.AppliedAmount;
        //                    LineItemBill lineItemsBill = _makepaymentRepository.InsertUpdateBillLineItems(sqlTransaction, billLineItem);
        //                }
        //                bill = InsertBill;
        //                Payments payments = PaymentDetails(sqlTransaction, billDetail);
        //                OutgoingPayment = payments;
        //                if (payments.Payment_Id > 0)
        //                {
        //                    payments_BillsMapping = _payments_BillsMappingRepository.GetPayments_BillsMapping(payments.Payment_Id);
        //                    if (payments_BillsMapping == null)
        //                    {
        //                        payments_BillsMapping = new Payments_BillsMapping();
        //                        payments_BillsMapping.Payment_Id = payments.Payment_Id;
        //                        payments_BillsMapping.Bill_Id = InsertBill.Bill_Id;
        //                        payments_BillsMapping = _payments_BillsMappingRepository.InsertPayments_BillsMapping(sqlTransaction, payments_BillsMapping);
        //                    }
        //                    Payment_Id = payments.Payment_Id;
        //                    serviceResponse.Status = true;
        //                    serviceResponse.Data = payments;
        //                }

        //            }
        //        }

        //        sqlTransaction.Commit();
        //        if (Payment_Id > 0)
        //        {
        //            //serviceResponse = ApplyPayments(invoiceDetailsList, Payment_Id, MasterLookup._Directions.AP, true);
        //            serviceResponse = ApplyPayments(invoiceDetailsList, Payment_Id, Direction_Id, true);
        //            SqlTransaction sqlTransaction1 = _baseRepository.GetNewTransaction();
        //            if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
        //            {
        //                if (serviceResponse?.Status == true)
        //                {
        //                    if (serviceResponse.Data != null)
        //                    {
        //                        List<PolicyDetails> policyDetailsList = (List<PolicyDetails>)serviceResponse.Data;
        //                        if (policyDetailsList?.Count > 0)
        //                        {

        //                            try
        //                            {
        //                                OutgoingPayment.PolicyDetailsList = policyDetailsList;
        //                                OutgoingPayment.TxnDate = DateTime.Now;
        //                                OutgoingPayment.PaymentDate = billDetail.PaymentToDate;
        //                                OutgoingPayment.MemoDescription = billDetail.MemoDescription;
        //                                QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, MasterLookup._Services.Intuit, clients.AccountingType_Id);
        //                                if (bill.EntityType_Id == MasterLookup._EntityTypes.State)
        //                                {
        //                                    billDetail.EntityName = $"State of {billDetail.EntityName}";
        //                                    StateDetails statedetails = _policyDetailsRepository.GetStateDetailsByName(billDetail.EntityName, userSession.Client_Id);
        //                                    bill.Entity_Id = statedetails.StateDetail_Id;
        //                                }
        //                                externalVendorEntityReference = _externalVendorEntityReferenceRepository.GetExternalVendorEntityReference(userSession.Client_Id, bill.EntityType_Id, bill.Entity_Id, clients.AccountingService_Id);
        //                                bool IsVendorResolved = false;
        //                                if (externalVendorEntityReference == null)
        //                                {
        //                                    ReadByQueryRes vendorByName = qBOUtility.ReadVendorByDisplayName("EI - " + billDetail.EntityName);
        //                                    if (vendorByName?.QueryResponse?.Vendor == null)
        //                                    {
        //                                        long Client_Id = userSession.Client_Id;

        //                                        Entities.External.QBO.Vendor.CreateVendorRes createVendorRes = qBOUtility.CreateVendor(bill.EntityType_Id, bill.Entity_Id, bill.EntityName, Client_Id);

        //                                        if (!string.IsNullOrWhiteSpace(createVendorRes?.Vendor?.Id))
        //                                        {
        //                                            externalVendorEntityReference = new ExternalVendorEntityReference
        //                                            {
        //                                                Client_Id = userSession.Client_Id,
        //                                                EntityType_Id = bill.EntityType_Id,
        //                                                Entity_Id = bill.Entity_Id,
        //                                                Service_Id = clients.AccountingService_Id,
        //                                                ExternalRefId = createVendorRes.Vendor.Id
        //                                            };

        //                                            externalVendorEntityReference =
        //                                                _externalVendorEntityReferenceRepository.InsertExternalVendorEntityReference(
        //                                                    sqlTransaction1,
        //                                                    externalVendorEntityReference
        //                                                );

        //                                            if (externalVendorEntityReference?.ExternalEntityReference_Id > 0)
        //                                            {
        //                                                IsVendorResolved = true;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (sqlTransaction1?.Connection != null)
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                            }

        //                                            serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                            serviceResponse.Status = false;
        //                                            serviceResponse.Message +=
        //                                                $"Error while creating a vendor in QBO.\n{createVendorRes?.Fault?.Error?[0]?.Message}";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        Accounting.Entities.External.QBO.Vendor.Vendor vendor = null;
        //                                        if (vendorByName?.QueryResponse?.Vendor?.Any() == true)
        //                                        {
        //                                            vendor = vendorByName?.QueryResponse?.Vendor?.FirstOrDefault();
        //                                            externalVendorEntityReference = new ExternalVendorEntityReference();
        //                                            externalVendorEntityReference.Client_Id = userSession.Client_Id;
        //                                            externalVendorEntityReference.EntityType_Id = bill.EntityType_Id;
        //                                            externalVendorEntityReference.Entity_Id = bill.Entity_Id;
        //                                            externalVendorEntityReference.Service_Id = clients.AccountingService_Id;
        //                                            externalVendorEntityReference.ExternalRefId = vendor?.Id;
        //                                            externalVendorEntityReference = _externalVendorEntityReferenceRepository.InsertExternalVendorEntityReference(sqlTransaction1, externalVendorEntityReference);
        //                                            if (externalVendorEntityReference.ExternalEntityReference_Id > 0)
        //                                            {
        //                                                IsVendorResolved = true;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (sqlTransaction1.Connection != null)
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                            }
        //                                            serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                            serviceResponse.Status = false;
        //                                            serviceResponse.Message += $"Error while creating a vendor in QBO.\n{vendorByName?.Fault?.Error?[0]?.Message}";
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (!string.IsNullOrEmpty(externalVendorEntityReference.ExternalRefId))
        //                                    {
        //                                        Entities.External.QBO.Vendor.UpdateVendorRes updateVendorRes = qBOUtility.UpdateVendor(externalVendorEntityReference.ExternalRefId, bill.EntityType_Id, bill.Entity_Id, bill.EntityName, userSession.Client_Id);
        //                                        if (!string.IsNullOrWhiteSpace(updateVendorRes?.Vendor?.Id))
        //                                        {
        //                                            IsVendorResolved = true;
        //                                        }
        //                                        else
        //                                        {
        //                                            if (sqlTransaction1.Connection != null)
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                            }
        //                                            serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                            serviceResponse.Status = false;
        //                                            serviceResponse.Message += $"Error while updating a vendor in QBO.\n{updateVendorRes?.Fault?.Error?[0]?.Message}";
        //                                        }
        //                                    }


        //                                }
        //                                if (IsVendorResolved)
        //                                {
        //                                    externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(userSession.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, bill.Bill_Id);
        //                                    if (externalBillReference == null)
        //                                    {
        //                                        Entities.External.QBO.Bill.CreateBillRes createBillRes = qBOUtility.CreateBill(OutgoingPayment, externalVendorEntityReference.ExternalRefId, Direction_Id);
        //                                        if (!string.IsNullOrWhiteSpace(createBillRes?.Bill?.Id))
        //                                        {
        //                                            externalBillReference = new ExternalBillReference();
        //                                            externalBillReference.Client_Id = userSession.Client_Id;
        //                                            externalBillReference.Service_Id = clients.AccountingService_Id;
        //                                            externalBillReference.SourceType_Id = MasterLookup._Sources.Bill;
        //                                            externalBillReference.Source_Id = bill.Bill_Id;
        //                                            externalBillReference.ExternalBillIdentifier = createBillRes.Bill.Id;
        //                                            externalBillReference = _externalBillReferenceRepository.InsertExternalBillReference(sqlTransaction1, externalBillReference);
        //                                            if (externalBillReference.ExternalBillReference_Id > 0)
        //                                            {
        //                                                sqlTransaction1.Commit();
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (sqlTransaction1.Connection != null)
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                            }
        //                                            serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                            serviceResponse.Status = false;
        //                                            serviceResponse.Message += $"Error while creating a Bill in QBO.\n{createBillRes?.Fault?.Error?[0]?.Message}";
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (sqlTransaction1.Connection != null)
        //                                    {
        //                                        sqlTransaction1.Rollback();
        //                                    }
        //                                    serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                    serviceResponse.Status = false;
        //                                    serviceResponse.Message += $"There was an error resolving Vendor (Vendor Entity_Id: {bill.Entity_Id}, EntityType_Id: {bill.EntityType_Id}";
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                if (sqlTransaction1.Connection != null)
        //                                {
        //                                    sqlTransaction1.Rollback();
        //                                }
        //                                serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                serviceResponse.Status = false;
        //                                serviceResponse.Message = ex.Message;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
        //            {
        //                if (serviceResponse?.Status == true)
        //                {
        //                    if (serviceResponse.Data != null)
        //                    {
        //                        List<PolicyDetails> policyDetailsList = (List<PolicyDetails>)serviceResponse.Data;
        //                        if (policyDetailsList?.Count > 0)
        //                        {
        //                            try
        //                            {
        //                                OutgoingPayment.PolicyDetailsList = policyDetailsList;
        //                                OutgoingPayment.TxnDate = DateTime.Now;
        //                                OutgoingPayment.PaymentDate = billDetail.PaymentToDate;
        //                                //string vendorId = $"EI-{bill.EntityName.Replace(" ", "-").ToUpper()}";
        //                                string entityName = bill.EntityName?.Trim() ?? string.Empty;
        //                                string vendorName;
        //                                if (entityName.Length > 15)
        //                                {
        //                                    var words = entityName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //                                    vendorName = words.Length >= 2
        //                                        ? $"{words[0]}-{words[1]}"
        //                                        : words[0];
        //                                }
        //                                else
        //                                {
        //                                    vendorName = entityName.Replace(" ", "-");
        //                                }

        //                                string vendorId = $"EI-{vendorName.ToUpper()}";
        //                                SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, MasterLookup._Services.Sage, clients.AccountingType_Id);
        //                                if (bill.EntityType_Id == MasterLookup._EntityTypes.State)
        //                                {
        //                                    billDetail.EntityName = $"State of {billDetail.EntityName}";
        //                                    StateDetails statedetails = _policyDetailsRepository.GetStateDetailsByName(billDetail.EntityName, userSession.Client_Id);
        //                                    bill.Entity_Id = statedetails.StateDetail_Id;
        //                                }
        //                                string readVendorResponse = sageUtility.ReadVendorByVendorId(vendorId);
        //                                bool vendorExists = readVendorResponse.Contains("<vendor>");
        //                                externalVendorEntityReference = _externalVendorEntityReferenceRepository.GetExternalVendorEntityReference(userSession.Client_Id, bill.EntityType_Id, bill.Entity_Id, clients.AccountingService_Id);
        //                                bool IsVendorResolved = false;
        //                                if (externalVendorEntityReference == null && !vendorExists)
        //                                {
        //                                    SageResponse createVendorRes = sageUtility.CreateVendor(vendorId, bill.EntityName);

        //                                    if (!string.IsNullOrWhiteSpace(createVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo))
        //                                    {
        //                                        var recordNo = createVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo;
        //                                        externalVendorEntityReference =
        //                                        new ExternalVendorEntityReference
        //                                        {
        //                                            Client_Id = userSession.Client_Id,
        //                                            EntityType_Id = bill.EntityType_Id,
        //                                            Entity_Id = bill.Entity_Id,
        //                                            Service_Id = clients.AccountingService_Id,
        //                                            ExternalRefId = createVendorRes.Operation.Result.Data.Vendor.RecordNo
        //                                        };
        //                                        externalVendorEntityReference = _externalVendorEntityReferenceRepository.InsertExternalVendorEntityReference(sqlTransaction1, externalVendorEntityReference);
        //                                        IsVendorResolved = true;

        //                                    }
        //                                    else
        //                                    {
        //                                        if (sqlTransaction1?.Connection != null)
        //                                        {
        //                                            sqlTransaction1.Rollback();
        //                                        }

        //                                        var error = createVendorRes?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
        //                                        serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                        serviceResponse.Status = false;
        //                                        serviceResponse.Message +=
        //                                            $"Error while creating vendor in Sage.\n" +
        //                                            $"Description: {error?.Description2}";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (!string.IsNullOrEmpty(externalVendorEntityReference?.ExternalRefId) && vendorExists)
        //                                    {
        //                                        SageResponse updateVendorRes = sageUtility.UpdateVendor(externalVendorEntityReference.ExternalRefId, bill.EntityName);

        //                                        if (!string.IsNullOrWhiteSpace(updateVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo))
        //                                        {
        //                                            var recordNo = updateVendorRes?.Operation?.Result?.Data?.Vendor?.RecordNo;
        //                                            IsVendorResolved = true;

        //                                        }
        //                                        else
        //                                        {
        //                                            if (sqlTransaction1?.Connection != null)
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                            }

        //                                            var error = updateVendorRes?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
        //                                            serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                            serviceResponse.Status = false;

        //                                            serviceResponse.Message +=
        //                                                $"Error while updating vendor in Sage.\n" +
        //                                                $"ErrorNo: {error?.Errorno}\n" +
        //                                                $"Description: {error?.Description2}\n" +
        //                                                $"Correction: {error?.Correction}";
        //                                        }
        //                                        //IsVendorResolved = true;
        //                                    }
        //                                }

        //                                if (IsVendorResolved)
        //                                {
        //                                    externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(userSession.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, bill.Bill_Id);
        //                                    if (externalBillReference == null)
        //                                    {
        //                                        //var getAccountno = _chartofAccountsRepository.GetChartOfAccountsByService_Id();

        //                                        SageResponse createBillRes = sageUtility.CreateBill(policyDetailsList, OutgoingPayment, vendorId);

        //                                        if (!string.IsNullOrWhiteSpace(createBillRes?.Operation?.Result?.Data?.APBill?.RecordNo))
        //                                        {
        //                                            externalBillReference = new ExternalBillReference();
        //                                            externalBillReference.Client_Id = userSession.Client_Id;
        //                                            externalBillReference.Service_Id = clients.AccountingService_Id;
        //                                            externalBillReference.SourceType_Id = MasterLookup._Sources.Bill;
        //                                            externalBillReference.Source_Id = bill.Bill_Id;
        //                                            externalBillReference.ExternalBillIdentifier = createBillRes.Operation.Result.Data.APBill.RecordNo;
        //                                            externalBillReference = _externalBillReferenceRepository.InsertExternalBillReference(sqlTransaction1, externalBillReference);
        //                                            if (externalBillReference.ExternalBillReference_Id > 0)
        //                                            {
        //                                                sqlTransaction1.Commit();
        //                                            }
        //                                            else
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                                serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                                serviceResponse.Status = false;

        //                                                serviceResponse.Message = "Failed to save Sage Bill reference.";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (sqlTransaction1.Connection != null)
        //                                            {
        //                                                sqlTransaction1.Rollback();
        //                                            }
        //                                            var error = createBillRes?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
        //                                            serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                            serviceResponse.Status = false;
        //                                            serviceResponse.Message +=
        //                                                $"Error while creating Bill in Sage.\n" +
        //                                                $"ErrorNo: {error?.Errorno}\n" +
        //                                                $"Description: {error?.Description2}\n" +
        //                                                $"Correction: {error?.Correction}";
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (sqlTransaction1.Connection != null)
        //                                    {
        //                                        sqlTransaction1.Rollback();
        //                                    }
        //                                    serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                    serviceResponse.Status = false;
        //                                    serviceResponse.Message += $"There was an error resolving Vendor (Vendor Entity_Id: {bill.Entity_Id}, EntityType_Id: {bill.EntityType_Id}";
        //                                }

        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                if (sqlTransaction1.Connection != null)
        //                                {
        //                                    sqlTransaction1.Rollback();
        //                                }
        //                                serviceResponse = DeletePaymentDetails(Payment_Id);
        //                                serviceResponse.Status = false;
        //                                serviceResponse.Message = ex.Message;
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //        }
        //        else
        //        {
        //            serviceResponse.Status = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (sqlTransaction.Connection != null)
        //        {
        //            sqlTransaction.Rollback();
        //        }
        //        serviceResponse = DeletePaymentDetails(Payment_Id);
        //        serviceResponse.Status = false;
        //        serviceResponse.Message = ex.Message;
        //    }
        //    return serviceResponse;
        //}

        [NonAction]
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

        [HttpPost]
        public ServiceResponse ConfirmBillCreation(long Report_Id, byte FileType_Id, List<SelectedInvoice> selectedInvoices, byte Direction_Id, Accounting.Entities.DB.Transactions.Bill billDetail)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            try
            {
                if (billDetail?.MemoDescription?.Length > 4000)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Memo should not be greater than 4000 characters.";
                    return serviceResponse;
                }
                if (selectedInvoices == null || selectedInvoices?.Count == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "No Invoices selected for bill.";
                    return serviceResponse;

                }

                var selectedInvoicesJson = JsonConvert.SerializeObject(selectedInvoices);
                var billDetailJson = JsonConvert.SerializeObject(billDetail);
                var result = _policyDetailsRepository.ProcessBillQueue(userSession.Client_Id, Report_Id, FileType_Id, Direction_Id, selectedInvoicesJson, billDetailJson, userSession.User_Id);
                if (string.IsNullOrEmpty(result))
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Something went wrong Please contact Administrator.";
                    return serviceResponse;
                }
                else
                {
                    if (long.TryParse(result, out long billProcessId))
                    {
                        serviceResponse.Status = true;
                        serviceResponse.Message = billDetail.PaymentType == MasterLookup._PaymentTypes.Check ? "Check creation has been queued. You will be emailed if the check is unable to be created." : "Bill creation has been queued. You will be emailed if the bill is unable to be created.";
                        return serviceResponse;
                    }
                    else
                    {
                        if (result == "E")
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = billDetail.PaymentType == MasterLookup._PaymentTypes.Check ? "Check is alteady under Process" : "Bill is already under process.";
                            return serviceResponse;
                        }
                        else
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Something went wrong Please contact Administrator.";
                            return serviceResponse;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public Payments PaymentDetails(SqlTransaction sqlTransaction, Accounting.Entities.DB.Transactions.Bill makePayments)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            var Client_Id = userSession.Client_Id;
            Payments paymentDetails = new Payments();

            paymentDetails.EntityType_Id = makePayments.EntityType_Id;
            paymentDetails.PaymentDirection_Id = MasterLookup._PaymentDirections.Outgoing;
            paymentDetails.Entity_Id = (long)makePayments.Entity_Id;
            paymentDetails.PaymentDate = makePayments.PaymentToDate;
            paymentDetails.Amount = makePayments.AmountApplied;
            paymentDetails.PaymentStatus_Id = MasterLookup._PaymentStatus.Completed;
            paymentDetails.ReferenceNumber = $"Bill{makePayments.Bill_Id}";
            paymentDetails.PaymentType_Id = MasterLookup._PaymentTypes.Bill;
            paymentDetails.OperatingBank_Id = null;
            paymentDetails.Client_Id = Client_Id;

            CreateJournalRes createJournalResponse = new CreateJournalRes();

            if (paymentDetails.Payment_Id == 0)
            {
                paymentDetails.RemainingAmount = paymentDetails.Amount;
            }
            paymentDetails.SourceType_Id = MasterLookup._Sources.Payment;
            paymentDetails = _paymentsRepository.InsertUpdatePayments(sqlTransaction, paymentDetails);
            if (paymentDetails.Payment_Id > 0)
            {
                //Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                //if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                //{
                //    QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                //    createJournalResponse = qBOUtility.CreatePaymentJournalEntry(sqlTransaction, paymentDetails);

                //}
            }
            return paymentDetails;
        }
        public ServiceResponse ApplyPayments(List<SelectedInvoiceDetailDTO> checkedInvoiceDetails, long Payment_Id, byte Direction_Id, bool IsOverride = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            double RemainingBalance = 0;
            bool Paid = false;
            DateTime? PaidOn = null;
            Clients clients = _accountRepository.GetClients(userSession.Client_Id);
            List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                if (Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = $"No payments were associated.";
                    return serviceResponse;
                }
                else
                {
                    Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
                    List<PolicyDetails> policyDetailsList = new List<PolicyDetails>();
                    double PaymentsRemainingAmount = payments.RemainingAmount;
                    bool hasPositive = checkedInvoiceDetails.Any(x => x.Amount > 0);
                    bool hasNegative = checkedInvoiceDetails.Any(x => x.Amount < 0);
                    bool allPositive = hasPositive && !hasNegative;
                    bool allNegative = hasNegative && !hasPositive;
                    bool hasMixed = hasPositive && hasNegative;


                    bool isAP = Direction_Id == MasterLookup._Directions.AP;
                    bool isIncoming = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming;
                    bool isOutgoing = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing;
                    if (

                        (allPositive && isAP && isIncoming) ||    // Case 4 - All positives and direction = AP and if paymentdirection is Incoming
                        (allNegative && isAP && isOutgoing) ||    // Case 5 -  All negatives and direction = AP and if paymentdirection is outgoing
                        (hasMixed && isAP && isIncoming)          // Case 6 - any negatives any positives and direction = AP and if paymentdirection is incoming
                      )
                    {
                        serviceResponse.Status = false;

                        if (isAP)
                        {
                            if (isIncoming)
                                serviceResponse.Message = "In AP, incoming payments cannot have positive or mixed amounts.";
                            else
                                serviceResponse.Message = "In AP, outgoing payments cannot have negative amounts.";
                        }
                        return serviceResponse;
                    }
                    if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && checkedInvoiceDetails.Any(x => x.Amount > 0) && Direction_Id == MasterLookup._Directions.AP))
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "Incoming payments cannot have positive amounts selected.";

                        return serviceResponse;
                    }
                    if (checkedInvoiceDetails?.Count() == 0)
                    {
                        serviceResponse.Status = false;
                        //serviceResponse.Data = "Please select at least one line item.";
                        serviceResponse.Data = "Please select at least one full or partial invoice to apply payment";
                        return serviceResponse;
                    }
                    else
                    {
                        decimal ValidatePaymentremainingAmount = 0;
                        if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && checkedInvoiceDetails.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AP)
                            ||
                                (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && checkedInvoiceDetails.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AR))
                        {
                            ValidatePaymentremainingAmount = (decimal)checkedInvoiceDetails.Sum(x => Math.Abs(x.Amount));
                        }
                        else
                        {
                            ValidatePaymentremainingAmount = (decimal)checkedInvoiceDetails.Sum(x => x.Amount);
                        }
                        if ((decimal)payments.RemainingAmount < ValidatePaymentremainingAmount || (ValidatePaymentremainingAmount < 0))
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "The amount selected exceeded the payment amount";
                            return serviceResponse;
                        }
                        //var groupedInvoiceList = checkedInvoiceDetails?.GroupBy(c => c.Invoice_Id)?.ToList();
                        var partialpaymentInvoiceList = checkedInvoiceDetails?.Where(c => c.IsPartial == true).GroupBy(c => c.Invoice_Id).ToList();
                        var fullpaymentInvoiceList = checkedInvoiceDetails?.Where(c => c.IsPartial == false).GroupBy(c => c.Invoice_Id).ToList();
                        if (partialpaymentInvoiceList?.Count() > 0)
                        {
                            byte _AccountingType_Id = 0;
                            foreach (var item in partialpaymentInvoiceList)
                            {
                                long invoiceId = item.Key;
                                Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceId);
                                PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                                if (policyDetails != null && payments.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    AgentDetails agentDetails = _policyDetailsRepository.GetAgentDetails(policyDetails.AgentDetail_Id);
                                    if (agentDetails != null)
                                    {
                                        policyDetails.BankName = agentDetails.BankName;
                                        policyDetails.AccountTypeName = agentDetails.AccountTypeName;
                                        policyDetails.RoutingNumber = agentDetails.RoutingNumber;
                                        policyDetails.AccountNumber = agentDetails.AccountNumber;
                                    }
                                }
                                byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                                LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                                if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                                {
                                    _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                                }
                                else
                                {
                                    _AccountingType_Id = clients.AccountingType_Id;
                                }
                                policyDetails.AccountingType_Id = _AccountingType_Id;
                                var invoiceLineItems = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId)?.ToList();
                                var selectedAmountMap = new Dictionary<long, double>();

                                if (invoiceLineItems != null)
                                {
                                    foreach (var selectedLineItem in invoiceLineItems)
                                    {
                                        if (selectedLineItem.InvoiceDetail_Ids != null)
                                        {
                                            foreach (var id in selectedLineItem.InvoiceDetail_Ids)
                                            {
                                                selectedAmountMap[id] = selectedLineItem.Amount;
                                            }
                                        }

                                        if (selectedLineItem.InvoiceDetail_Id > 0)
                                        {
                                            selectedAmountMap[selectedLineItem.InvoiceDetail_Id] = selectedLineItem.Amount;
                                        }
                                    }
                                }
                                decimal TotalAmountChecked = 0;
                                decimal ActualAmount = 0;
                                if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && invoiceLineItems.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AP)
                                    ||
                                (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && checkedInvoiceDetails.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AR))
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Math.Abs(x.Amount));
                                }
                                else
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                }
                                ActualAmount = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                PaymentApplications paymentApplications = new PaymentApplications();
                                paymentApplications.Payment_Id = Payment_Id;
                                paymentApplications.Invoice_Id = invoiceId;
                                paymentApplications.AppliedDate = DateTime.Now;
                                paymentApplications.TotalAppliedAmount = Convert.ToDouble(ActualAmount);
                                paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
                                if (paymentApplications.Payment_Id > 0)
                                {
                                    double AllocatedRemainingAmount = (double)ActualAmount;
                                    var invoiceDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(invoiceId, Direction_Id);
                                    // Collect all selected detail IDs (partial OR full)
                                    var selectedDetailIds = invoiceLineItems.SelectMany(x => x.InvoiceDetail_Ids != null && x.InvoiceDetail_Ids.Count > 0 ? x.InvoiceDetail_Ids : new List<long> { x.InvoiceDetail_Id }).ToList();

                                    if (selectedDetailIds.Count > 0)
                                    {
                                        invoiceDetailsList = invoiceDetailsList.Where(x => selectedDetailIds.Contains(x.Id)).ToList();
                                    }
                                    foreach (var lineItem in invoiceDetailsList)
                                    {
                                        double _ApplyAmount = 0;

                                        if (selectedAmountMap != null && selectedAmountMap.ContainsKey(lineItem.Id))
                                        {
                                            _ApplyAmount = selectedAmountMap[lineItem.Id];
                                        }
                                        if (_ApplyAmount != 0)
                                        {
                                            _ApplyAmount = Math.Sign(lineItem.RemainingBalance) * Math.Abs(_ApplyAmount);
                                            PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                            paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                            paymentApplicationLineItems.InvoiceLineItem_Id = lineItem.Id;
                                            paymentApplicationLineItems.AppliedAmount = _ApplyAmount;
                                            paymentApplicationLineItems.Is_SplitPayment = false;
                                            paymentApplicationLineItems.SubType_Id = null;
                                            paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                                            _paymentsRepository.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
                                            RemainingBalance = Math.Round(lineItem.RemainingBalance - Convert.ToDouble(Math.Sign(lineItem.RemainingBalance) * Math.Abs(_ApplyAmount)), 2);
                                            Paid = RemainingBalance == 0;
                                            PaidOn = Paid == true ? DateTime.Now : null;
                                            _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, lineItem.Id, RemainingBalance, Paid);
                                            PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                            var policyClone = new PolicyDetails
                                            {
                                                // copy required fields manually
                                                PolicyNumber = policyDetails.PolicyNumber,
                                                AccountingType_Id = policyDetails.AccountingType_Id,
                                                CarrierDetail_Id = policyDetails.CarrierDetail_Id,
                                                Client_Id = policyDetails.Client_Id,
                                                BankName = policyDetails.BankName,
                                                AccountTypeName = policyDetails.AccountTypeName,
                                                RoutingNumber = policyDetails.RoutingNumber,
                                                AccountNumber = policyDetails.AccountNumber
                                                // copy only what you need
                                            };

                                            var invoiceClone = new Invoices
                                            {
                                                InvoiceNo = invoices.InvoiceNo,
                                                Id = invoices.Id,
                                            };

                                            var invoiceDetailsClone = new InvoiceDetails
                                            {
                                                Id = lineItem.Id,
                                                Amount = lineItem.Amount,
                                                RemainingBalance = lineItem.RemainingBalance,
                                                LOB_Id = lineItem.LOB_Id,
                                                BillingType_LineItems_Directions_Id = lineItem.BillingType_LineItems_Directions_Id,
                                                Invoice_Id = lineItem.Invoice_Id,
                                            };

                                            invoiceDetailsClone.PaymentApplicationLineItems = new PaymentApplicationLineItems
                                            {
                                                PaymentApplication_Id = paymentApplicationLineItems.PaymentApplication_Id,
                                                AppliedAmount = paymentApplicationLineItems.AppliedAmount,
                                                InvoiceLineItem_Id = paymentApplicationLineItems.InvoiceLineItem_Id
                                            };

                                            invoiceClone.InvoiceDetails = invoiceDetailsClone;
                                            policyClone.Invoices = invoiceClone;

                                            policyDetailsList.Add(policyClone);
                                        }
                                    }
                                    PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(TotalAmountChecked), 2);
                                    _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, Payment_Id, PaymentsRemainingAmount);
                                }
                            }
                        }
                        if (fullpaymentInvoiceList?.Count() > 0)
                        {
                            byte _AccountingType_Id = 0;
                            foreach (var item in fullpaymentInvoiceList)
                            {
                                long invoiceId = item.Key;
                                Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceId);
                                PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                                if (policyDetails != null && payments.EntityType_Id == MasterLookup._EntityTypes.Agency)
                                {
                                    AgentDetails agentDetails = _policyDetailsRepository.GetAgentDetails(policyDetails.AgentDetail_Id);
                                    if (agentDetails != null)
                                    {
                                        policyDetails.BankName = agentDetails.BankName;
                                        policyDetails.AccountTypeName = agentDetails.AccountTypeName;
                                        policyDetails.RoutingNumber = agentDetails.RoutingNumber;
                                        policyDetails.AccountNumber = agentDetails.AccountNumber;
                                    }
                                }
                                byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                                LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                                if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                                {
                                    _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                                }
                                else
                                {
                                    _AccountingType_Id = clients.AccountingType_Id;
                                }
                                policyDetails.AccountingType_Id = _AccountingType_Id;
                                var invoiceLineItems = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId)?.ToList();
                                decimal TotalAmountChecked = 0;
                                decimal ActualAmount = 0;
                                if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && invoiceLineItems.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AP))
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Math.Abs(x.Amount));
                                }
                                else
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                }
                                ActualAmount = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                PaymentApplications paymentApplications = new PaymentApplications();
                                paymentApplications.Payment_Id = Payment_Id;
                                paymentApplications.Invoice_Id = invoiceId;
                                paymentApplications.AppliedDate = DateTime.Now;
                                paymentApplications.TotalAppliedAmount = Convert.ToDouble(ActualAmount);

                                if (Direction_Id == MasterLookup._Directions.AR)
                                {
                                    RemainingBalance = Math.Round(invoices.RemainingBalance - Convert.ToDouble(Math.Sign(invoices.RemainingBalance) * Math.Abs(TotalAmountChecked)), 2);
                                    Paid = RemainingBalance == 0;
                                    _invoiceRepository.UpdateBalanceInInvoices(PaymentsTransaction, invoiceId, RemainingBalance, Paid);
                                }
                                paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
                                if (paymentApplications.Payment_Id > 0)
                                {
                                    var invoiceDetailsList = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId);
                                    if (invoiceDetailsList?.Count() > 0)
                                    {
                                        foreach (var invoice in invoiceDetailsList)
                                        {
                                            if (invoice.RemainingBalance != 0)
                                            {
                                                InvoiceDetails invoiceDetails = _invoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(invoice.InvoiceDetail_Id);
                                                //if ((invoiceDetails.EntityType_Id != payments.EntityType_Id || invoiceDetails.Entity_Id != payments.Entity_Id) && !IsOverride)
                                                if (!IsOverride && payments != null && invoiceDetails != null && payments.Entity_Id > 0 && payments.EntityType_Id > 0 && invoiceDetails.Entity_Id > 0 && invoiceDetails.EntityType_Id > 0 && (invoiceDetails.EntityType_Id != payments.EntityType_Id || invoiceDetails.Entity_Id != payments.Entity_Id))
                                                {
                                                    if (PaymentsTransaction.Connection != null)
                                                    {
                                                        PaymentsTransaction.Rollback();
                                                    }
                                                    serviceResponse.Status = false;
                                                    serviceResponse.Message = "Payment entity type must match the invoice entity type.";
                                                    serviceResponse.Data = new { EntityMismatch = true };
                                                    return serviceResponse;
                                                }
                                                PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                                paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                                paymentApplicationLineItems.InvoiceLineItem_Id = invoice.InvoiceDetail_Id;
                                                paymentApplicationLineItems.AppliedAmount = invoice.Amount;
                                                paymentApplicationLineItems.Is_SplitPayment = false;
                                                paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                                                _paymentsRepository.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
                                                RemainingBalance = Math.Round(invoice.RemainingBalance - Convert.ToDouble(Math.Sign(invoice.RemainingBalance) * Math.Abs(invoice.Amount)), 2);
                                                Paid = RemainingBalance == 0;
                                                PaidOn = Paid == true ? DateTime.Now : null;
                                                _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, invoice.InvoiceDetail_Id, RemainingBalance, Paid);
                                                PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                var policyClone = new PolicyDetails
                                                {
                                                    // copy required fields manually
                                                    PolicyNumber = policyDetails.PolicyNumber,
                                                    AccountingType_Id = policyDetails.AccountingType_Id,
                                                    CarrierDetail_Id = policyDetails.CarrierDetail_Id,
                                                    Client_Id = policyDetails.Client_Id,
                                                    BankName = policyDetails.BankName,
                                                    AccountTypeName = policyDetails.AccountTypeName,
                                                    RoutingNumber = policyDetails.RoutingNumber,
                                                    AccountNumber = policyDetails.AccountNumber,
                                                    // copy only what you need
                                                };

                                                var invoiceClone = new Invoices
                                                {
                                                    InvoiceNo = invoices.InvoiceNo,
                                                    Id = invoices.Id,
                                                };

                                                var invoiceDetailsClone = new InvoiceDetails
                                                {
                                                    Id = invoiceDetails.Id,
                                                    Amount = invoiceDetails.Amount,
                                                    RemainingBalance = invoiceDetails.RemainingBalance,
                                                    LOB_Id = invoiceDetails.LOB_Id,
                                                    BillingType_LineItems_Directions_Id = invoiceDetails.BillingType_LineItems_Directions_Id,
                                                };

                                                invoiceDetailsClone.PaymentApplicationLineItems = new PaymentApplicationLineItems
                                                {
                                                    AppliedAmount = paymentApplicationLineItems.AppliedAmount,
                                                    InvoiceLineItem_Id = paymentApplicationLineItems.InvoiceLineItem_Id
                                                };

                                                invoiceClone.InvoiceDetails = invoiceDetailsClone;
                                                policyClone.Invoices = invoiceClone;

                                                policyDetailsList.Add(policyClone);
                                                //policyDetails.Invoices = invoices;
                                                //policyDetails.Invoices.InvoiceDetails = invoiceDetails;
                                                //policyDetails.Invoices.InvoiceDetails.PaymentApplicationLineItems = paymentApplicationLineItems;
                                                //policyDetailsList.Add(policyDetails);
                                            }
                                        }
                                    }
                                }
                                PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(TotalAmountChecked), 2);
                                _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, Payment_Id, PaymentsRemainingAmount);
                            }
                        }
                        if (PaymentApplicationLineItemsList.Count() > 0 && policyDetailsList?.Count > 0)
                        {
                            serviceResponse.Data = policyDetailsList;
                            //if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                            //{
                            //    JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Payment, payments.Payment_Id);
                            //    QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                            //    UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
                            //    if (updateJournalRes != null && updateJournalRes.JournalEntry?.Id != null)
                            //    {
                            //        serviceResponse.Status = true;
                            //        serviceResponse.Message = "Applied payment successfully.";
                            //        PaymentsTransaction.Commit();
                            //        return serviceResponse;
                            //    }
                            //    else
                            //    {
                            //        if (PaymentsTransaction.Connection != null)
                            //        {
                            //            PaymentsTransaction.Rollback();
                            //        }
                            //        serviceResponse.Status = false;
                            //        serviceResponse.Message = updateJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {updateJournalRes.Fault.Error[0].Detail}" : "Unable to process request";
                            //        return serviceResponse;
                            //    }
                            //}
                            //else
                            //{
                            //    PaymentsTransaction.Commit();
                            //}
                            PaymentsTransaction.Commit();
                        }
                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Applied payment successfully.";
                    return serviceResponse;
                }

            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;

        }
        [HttpPost]
        public ServiceResponse DeletePaymentDetails(long Payment_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    return serviceResponse;
                }
                else
                {
                    List<PaymentApplicationLineItems> _paymentapplicationlineitems = _paymentsRepository.GetPaymentApplicationLineItemsByPayment_Id(Payment_Id);
                    if (_paymentapplicationlineitems != null && _paymentapplicationlineitems?.Count > 0)
                    {
                        foreach (var item in _paymentapplicationlineitems)
                        {
                            ServiceResponse response = UnApplyPayment(item, true);
                            if (!response.Status)
                            {
                                serviceResponse.Status = false;
                                serviceResponse.Message = $"Failed to unapply payment for this payment";
                                return response;
                            }
                        }
                    }
                    Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                    if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                    {
                        QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        Payments_BillsMapping payments_BillsMapping = _payments_BillsMappingRepository.GetPayments_BillsMapping(Payment_Id);
                        if (payments_BillsMapping != null)
                        {
                            ExternalBillReference externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(userSession.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, payments_BillsMapping.Bill_Id);
                            if (!string.IsNullOrWhiteSpace(externalBillReference?.ExternalBillIdentifier))
                            {
                                Entities.External.QBO.Bill.ReadByIdRes readByIdRes = qBOUtility.ReadBillById(externalBillReference.ExternalBillIdentifier);
                                if (!string.IsNullOrWhiteSpace(readByIdRes.Bill.Id))
                                {
                                    qBOUtility.DeleteBill(readByIdRes);
                                    _externalBillReferenceRepository.InactivePaymentBill(Payment_Id, externalBillReference);
                                }
                            }
                        }
                        else
                        {
                            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
                            List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                            foreach (var item in journalEntryList)
                            {
                                DeleteJournalResponse deleteJournalRes = qBOUtility.DeleteJournalEntry(item);
                            }
                            if (journalEntryList != null)
                            {
                                _journalEntryLogRepository.DeleteJournalEntry(journalEntryList.FirstOrDefault().SourceType_Id, journalEntryList.FirstOrDefault().Source_Id);
                            }
                        }
                    }
                    _paymentsRepository.DeletePaymentDetails(Payment_Id);
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ServiceResponse UnApplyPayment(PaymentApplicationLineItems paymentApplicationLineItems, bool Is_PaymentDeleted = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                double RemainingBalance = 0;
                bool Paid = false;
                DateTime? PaidOn = null;
                byte _AccountingType_Id = 0;
                if (paymentApplicationLineItems != null)
                {
                    InvoiceDetails invoiceDetails = _invoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                    if (invoiceDetails != null)
                    {
                        Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                        PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                        byte BillingType_Id = LookupUtility.GetAllBillingTypes()?.FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                        LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                        if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                        {
                            _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                        }
                        else
                        {
                            _AccountingType_Id = clients.AccountingType_Id;
                        }
                        paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                        LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions()?.FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions()?.FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                        RemainingBalance = invoiceDetails.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                        Paid = RemainingBalance == 0;
                        _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, invoiceDetails.Id, RemainingBalance, Paid);
                        if (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (invoices != null)
                            {
                                RemainingBalance = invoices.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                                Paid = RemainingBalance == 0;
                                PaidOn = Paid == true ? DateTime.Now : null;
                                _invoiceRepository.UpdateBalanceInInvoices(PaymentsTransaction, invoices.Id, RemainingBalance, Paid);
                            }
                        }
                        PaymentApplications paymentApplications = _paymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);
                        if (paymentApplications != null)
                        {
                            Payments payments = _paymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
                            if (payments != null)
                            {
                                if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP) || (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR))
                                {
                                    RemainingBalance = payments.RemainingAmount + Math.Abs(paymentApplicationLineItems.AppliedAmount);
                                }
                                else
                                {
                                    RemainingBalance = payments.RemainingAmount + paymentApplicationLineItems.AppliedAmount;
                                }
                                _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, paymentApplications.Payment_Id, RemainingBalance);
                                _paymentsRepository.InactivePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems.PaymentApplicationLineItem_Id);
                                Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                                if (!Is_PaymentDeleted)
                                {
                                    if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                    {
                                        List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                        JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex).FirstOrDefault();
                                        QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, _AccountingType_Id);
                                        //UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, new Payments(), new List<PaymentApplicationLineItems>(), journalEntries, false, paymentApplicationLineItems);
                                        CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, new Payments(), new List<PaymentApplicationLineItems>(), journalEntry, false, paymentApplicationLineItems);
                                        if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                        {
                                            serviceResponse.Status = true;
                                            serviceResponse.Message = "Un Applied payment successfully and synced to accounting software successfully.";
                                            PaymentsTransaction.Commit();
                                        }
                                        else
                                        {
                                            if (PaymentsTransaction.Connection != null)
                                            {
                                                PaymentsTransaction.Rollback();
                                            }
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Couldn't un apply and sync to accounting software";
                                            return serviceResponse;
                                        }
                                    }
                                    else
                                    {
                                        PaymentsTransaction.Commit();
                                    }
                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                    }
                }
                serviceResponse.Status = true;
                serviceResponse.Message = string.IsNullOrEmpty(serviceResponse.Message) ? $"Un Applied payments succesfully." : serviceResponse.Message;
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public IActionResult BillBatchProcess()
        {
            List<BillProcessQueueWeb> billProcessQueues = _paymentsRepository.GetBillProcessQueues(userSession.Client_Id);
            return PartialView(billProcessQueues);
        }

        public IActionResult BillBatchProcessView(long BillProcess_Id, DateTime date)
        {
            try
            {
                var isSuperAdmin = userSession?.UserRoleMappingList?.Any(x => x.Role_Id == MasterLookup._Roles.SuperAdmin) ?? false;
                ViewBag.IsSuperAdmin = isSuperAdmin;
                List<BillProcessQueueEventsWeb> billProcessQueueEvents = _paymentsRepository.GetBillProcessQueuesView(userSession.Client_Id, BillProcess_Id, date);
                return PartialView(billProcessQueueEvents);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
