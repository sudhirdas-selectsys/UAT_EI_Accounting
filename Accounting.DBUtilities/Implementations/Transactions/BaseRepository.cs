using Accounting.DBUtilities.Interfaces.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class BaseRepository : IBaseRepository
    {
        public SqlTransaction GetNewTransaction()
        {
            return SQLHelper.GetNewTransaction();
        }
    }
}
