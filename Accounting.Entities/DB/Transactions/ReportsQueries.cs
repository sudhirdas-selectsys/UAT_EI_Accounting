namespace Accounting.Entities.DB.Transactions
{
    public  class ReportsQueries : BaseEntity
    {
        public long Report_Id { get; set; }
        public int SQLQuery_Id { get; set;  }
        public string  Name { get; set; }   
    }
}
