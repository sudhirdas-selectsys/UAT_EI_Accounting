namespace Accounting.Entities.DB.Transactions
{
    public class PostCashReceipts_Batches
    {
        public long Batch_Id { get; set; }
        public byte Service_Id { get; set; }
        public string BatchReference_Id { get; set; }
        public long IncomingRequest_Id { get; set; }
    }
}
