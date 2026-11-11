namespace Accounting.Entities.DB.Transactions
{
    public class PostCashReceipts_PaymentTransactions
    {
        public long PaymentTransaction_Id { get; set; }
        public long Batch_Id { get; set; }
        public string TransactionReference_Id { get; set; }
        public string ProcessorReference_Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string AccountNumber { get; set; }
        public double TransactionAmount { get; set; }
    }
}
