using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;

namespace Accounting.Web.Models
{
    public class UserService : IUserService
    {
        internal readonly IUsersRepository _usersRepository = new UsersRepository();
        public bool ValidateUser(string username, string password)
        {
            Users User = _usersRepository.ValidateUsers(username, password, true);
            return User != null ? true : false;
        }
    }
}
