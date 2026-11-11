namespace Accounting.Entities
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public long Submission_Id { get; set; }
        public long Quote_Id { get; set; }
        public long Client_Id { get; set; }
        public long PolicyDetails_Id { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public bool Status { get; set; }
        public int Sequence { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
