namespace Accounting.Entities.DB.Transactions
{
    public class Users : BaseEntity
    {
        public long User_Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long Role_Id { get; set; }
        public bool Is_APIUser { get; set; }
        public long? DefaultClient_Id { get; set; }
        public string Role { get; set; }
        public string ClientIdList { get; set; }
        public long ExistingRole_Id { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string Clients { get; set; }
        public bool Is_OtpVerified { get; set; } = false;
    }
}
