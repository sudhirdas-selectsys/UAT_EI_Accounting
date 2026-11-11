namespace Accounting.Entities.DB.Transactions
{
    public class LineItemTypes : BaseEntity
    {
        public int LineItemType_Id { get; set; }
        public long Invoice_Id { get; set; }
        public long PolicyDetail_Id { get; set; }
        public long Client_Id { get; set; }
        public int TotalAmount { get; set; }

        public string LineItemTypeName { get; set; }
      
    }
}
