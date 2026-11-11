using Accounting.Entities.DB.Transaction;
using System.Data;

namespace Accounting.Entities.DB.Transactions
{
    public class ReportOutputTables : BaseEntity
    {
        public int ReportOutputTable_Id { get; set; }
        public int SQLQuery_Id { get; set; }
        public string Name { get; set; }
        public int TableIndex { get; set; }
        public bool Hide { get; set; }
        public bool Enable_Paging { get; set; }
        public int? Default_Paging_Size { get; set; }
        public int? Default_Paging_Increment { get; set; }
        //Not Mapped
        public List<ReportOutputTableParameters> OutputParameters { get; set; }
        public DataTable DataTable { get; set; }
        public DataTable OutputDataTable { get; set; }
    }
}
