using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Utilities.Internal.Interfaces
{
    public interface IMSOfficeFileOptions
    {
        public byte[]? DownloadExcel(DataSet dataSet, List<ReportOutputTables> reportOutputTables);
        public byte[] ExportToExcel(DataSet ds);
        public byte[] DownloadDirectBillExcel(DataSet ds, DateTime? AsOfDate = null);
        public byte[] DownloadUnderwriterExcel(DataSet ds, DateTime? AsOfDate = null);
        public byte[] DownloadSimplifiedAgencyBillExcel(DataSet ds, bool IsOutstandingInvoices, long Client_Id, DateTime? AsOfDate = null);
        public DataTable BalanceDueInAgencyBill(bool IsOutstandingInvoices, DataTable table4Final);



    }
}
