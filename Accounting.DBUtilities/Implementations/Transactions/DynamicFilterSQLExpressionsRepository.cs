using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class DynamicFilterSQLExpressionsRepository : IDynamicFilterSQLExpressionsRepository
    {
        public List<LU_DynamicFilters> GetDynamicFilters(int SQLQuery_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
           {
                new SqlParameter("@SQLQuery_Id",SQLQuery_Id),
           };
            return SQLHelper.GetObjectList(new LU_DynamicFilters(), "USP_Accounting_GetDynamicFilters", sqlParameters);
        }
        public List<LU_DynamicFilterSQLExpressions> GetDynamicFilterSQLExpressions(int SQLQuery_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
           {
                new SqlParameter("@SQLQuery_Id",SQLQuery_Id),
           };
            return SQLHelper.GetObjectList(new LU_DynamicFilterSQLExpressions(), "USP_GetDynamicFilterSQLExpressions", sqlParameters);
        }
    }
}
