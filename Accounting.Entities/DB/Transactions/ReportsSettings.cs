namespace Accounting.Entities.DB.Transaction
{
    public  class ReportsSettings: BaseEntity
    {
        public int Account_Id { get; set; }
        public long Report_Id { get; set; }
    }
}
