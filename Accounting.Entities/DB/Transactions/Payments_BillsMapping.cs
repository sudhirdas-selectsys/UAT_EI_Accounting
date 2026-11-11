namespace Accounting.Entities.DB.Transactions
{
    public class Payments_BillsMapping : BaseEntity
    {
        public long PaymentBill_Id { get; set; }
        public long Payment_Id { get; set; }
        public long Bill_Id { get; set; }
    }
}
