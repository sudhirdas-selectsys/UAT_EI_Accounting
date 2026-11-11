namespace Accounting.Entities.DB.Transactions
{
    public class CarrierDetails : BaseEntity
    {
        public long Carrier_Id { get; set; }
        public string CarrierReference_Id  { get; set; }
        public long CarrierDetail_Id { get; set; }
        public bool Is_Deleted { get; set; }
        public string? CarrierStatus { get; set; }
        public string NAIC { get; set; }
    }
}
