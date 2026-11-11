namespace Accounting.Entities.DB.Lookups
{
    public class BaseLookupEntity
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string DisplayText { get; set; }
        public string Code { get; set; }
        public int SequenceNo { get; set; }
        public string Value { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Text { get; set; }
        public bool Status { get; set; }
        public short MinTextLength { get; set; }
        public short MaxTextLength { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public byte Service_Id { get; set; }
        public long Client_Id { get; set; }
    }
}
