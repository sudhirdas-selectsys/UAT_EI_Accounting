using Accounting.DBUtilities;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class LU_SQLQueriesRepository : ILU_SQLQueriesRepository
    {
        public List<LU_SQLQueries> GetSQLQueriesByReport(long Report_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@Report_Id",Report_Id),
            };
            return SQLHelper.GetObjectList(new LU_SQLQueries(), "USP_Accounting_GetSQLQueriesByReport", sqlParameters);
        }
        public LU_SQLQueries GetSQLQuery(long SQLQuery_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@SQLQuery_Id",SQLQuery_Id),
            };
            return SQLHelper.GetObject(new LU_SQLQueries(), "USP_Accounting_GetSQLQuery", sqlParameters);
        }
    }
}
