namespace Accounting.Entities.DB.Transactions
{
    public class InterestDetails : BaseEntity
    {
        public long Interest_Id { get; set; }
        public string TypeID { get; set; }
        public long InterestDetail_Id { get; set; }
        public string InterestReference_Id { get; set; }
    }
}
