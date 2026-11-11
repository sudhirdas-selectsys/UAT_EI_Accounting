namespace Accounting.Entities.DB.Transactions
{
    public class Reports : BaseEntity
    {
        public long Report_Id { get; set; }
        public string Name { get; set; }
        public int? ReportGrouping_Id { get; set; }
        public int? ParentReportGrouping_Id { get; set; }
        public string ReportGroupingName { get; set; }
    }
}
