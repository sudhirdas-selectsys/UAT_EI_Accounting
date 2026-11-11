using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Web.Abstract
{
    public interface IReportService
    {
        public string BuildReportHtmlFromTemplate(long Report_Id, long Client_Id, List<ReportOutputTables> reportOutputTablesList, LU_ReportTemplates ReportTemplate, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null);
        public DocumentFileInfo Download(byte FileType_Id, long Client_Id, List<ReportOutputTables> reportOutputTables, long Report_Id, string ActionType, LU_ReportTemplates ReportTemplate = null, bool IsOutstandingInvoices = false, bool IsSimplifiedView = false, DateTime? AsOfDate = null, bool IsAgentId_Include = false);
        List<AgentStatementFile> GenerateAgentStatementPDFs(long client_Id, List<ReportOutputTables> reportOutputTables, long Report_Id, LU_ReportTemplates ReportTemplate, bool IsOutstandingInvoices, bool IsSimplifiedView, DateTime? AsOfDate, bool IsAgentId_Include = false);
    }
}
