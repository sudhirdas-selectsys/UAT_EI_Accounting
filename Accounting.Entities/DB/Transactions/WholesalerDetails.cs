namespace Accounting.Entities.DB.Transactions
{
    public class WholesalerDetails : BaseEntity
    {
        public long WholesalerDetail_Id { get; set; }
        public string WholesalerReference_Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int? ParentID { get; set; }
        public bool? IsActive { get; set; }
        public string AlternateID { get; set; }
    }
}
