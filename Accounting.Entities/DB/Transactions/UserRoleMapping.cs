namespace Accounting.Entities.DB.Transactions
{
    public class UserRoleMapping : BaseEntity
    {
        public long RoleMapping_Id { get; set; }
        public long User_Id { get; set; }
        public byte Role_Id { get; set; }
    }
}
