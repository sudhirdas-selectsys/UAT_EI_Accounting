namespace Accounting.Web.Models
{
    public interface IUserService
    {
        bool ValidateUser(string username, string password);
    }
}
