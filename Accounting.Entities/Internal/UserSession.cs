using Accounting.Entities.DB.Transactions;

namespace Accounting.Entities.Internal
{
    public class UserSession
    {
        public UserSession()
        {
            UserRoleMappingList = new List<UserRoleMapping>();
        }
        public long User_Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public long Client_Id { get; set; }
        public long ConfigurationClient_Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string ISOCountryCode { get; set; }
        public string ZipCode { get; set; }
        public bool Is_Admin { get; set; }
        public string CompanyName { get; set; }
        public string ConfigurationCompanyName { get; set; }
        public string DefaultCompanyName { get; set; }
        public string Username { get; set; }
        public List<UserRoleMapping> UserRoleMappingList { get; set; }
        public bool Is_OtpVerified { get; set; } = false;
    }
}
