namespace Accounting.Entities.DB.Transactions
{
    public class InsuredDetails : BaseEntity
    {
        public long Insured_Id { get; set; }
        public long InsuredDetail_Id { get; set; }
        public string InsuredReference_Id { get; set; }
        public string FEIN { get; set; }
        public string? BusinessDescription { get; set; }
        public string? InsuredDBA { get; set; }
        public string Physical_Address1 { get; set; }
        public string Physical_Address2 { get; set; }
        public string Physical_City { get; set; }
        public string Physical_State { get; set; }
        public string Physical_ZipCode { get; set; }
        public string Physical_Country { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ReferralSource { get; set; }
    }
}
