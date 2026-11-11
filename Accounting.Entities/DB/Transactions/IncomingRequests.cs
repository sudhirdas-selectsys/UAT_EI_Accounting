namespace Accounting.Entities.DB.Transactions
{
    public class IncomingRequests : BaseEntity
    {
        public long IncomingRequest_Id { get; set; }
        public byte Service_Id { get; set; }
        public string RequestBody { get; set; }
    }
}
