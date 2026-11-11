using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IBaseRepository
    {
        SqlTransaction GetNewTransaction();
    }
}
