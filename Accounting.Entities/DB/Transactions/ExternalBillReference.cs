namespace Accounting.Entities.DB.Transactions
{
    public class ExternalBillReference : BaseEntity
    {
        public long ExternalBillReference_Id { get; set; }
        public byte Service_Id { get; set; }
        public byte SourceType_Id { get; set; }
        public long Source_Id { get; set; }
        public string ExternalBillIdentifier { get; set; }
    }
}
