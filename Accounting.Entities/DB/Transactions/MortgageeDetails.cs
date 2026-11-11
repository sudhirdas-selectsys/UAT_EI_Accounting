namespace Accounting.Entities.DB.Transactions
{
    public class MortgageeDetails : BaseEntity
    {
        public long MortgageeDetail_Id { get; set; }
        public string? MortgageeReference_Id { get; set; }
        public string? MortgageeType_Id { get; set; }
        public string? Email { get; set; }
    }
}
