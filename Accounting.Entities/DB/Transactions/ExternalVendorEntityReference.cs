namespace Accounting.Entities.DB.Transactions
{
    public class ExternalVendorEntityReference : BaseEntity
    {
        public long ExternalEntityReference_Id { get; set; }
        public byte EntityType_Id { get; set; }
        public long Entity_Id { get; set; }
        public byte Service_Id { get; set; }
        public string? ExternalRefId { get; set; }
    }
}
