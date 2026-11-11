namespace Accounting.Entities.DB.Transactions
{
    public class UnderWriterDetails : BaseEntity
    {
        public long UW_Id { get; set; }
        public string ConsolidatedGuid { get; set; }
        public string UWReference_Id { get; set; }
        public long UnderWriterDetail_Id { get; set; }
        public string AssignUserName { get; set; }
        public long AssignUser_Id { get; set; }
    }
}
