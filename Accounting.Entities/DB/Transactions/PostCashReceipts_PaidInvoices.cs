namespace Accounting.Entities.DB.Transactions
{
    public class PostCashReceipts_PaidInvoices
    {
        public long PaidInvoice_Id { get; set; }
        public long PaymentTransaction_Id { get; set; }
        public long Invoice_Id { get; set; }
        public double PaidAmount { get; set; }
        public string Comment { get; set; }
        public string Division { get; set; }
    }
}
