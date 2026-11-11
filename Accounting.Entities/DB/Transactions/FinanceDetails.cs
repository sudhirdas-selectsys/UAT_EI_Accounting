namespace Accounting.Entities.DB.Transactions
{
    public class FinanceDetails : BaseEntity
    {
        public string Finance_Id { get; set; }
        public long FinanceDetail_Id { get; set; }
        public string FinanceReference_Id { get; set; }
    }
}
