using Accounting.Entities.DB.Lookups;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILU_SQLQueriesRepository
    {
        List<LU_SQLQueries> GetSQLQueriesByReport(long Report_Id);
        public LU_SQLQueries GetSQLQuery(long SQLQuery_Id);
    }
}
