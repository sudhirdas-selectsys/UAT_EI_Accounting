using Accounting.Entities.DB.Lookups;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IDynamicFilterSQLExpressionsRepository
    {
        List<LU_DynamicFilters> GetDynamicFilters(int SQLQuery_Id);
        List<LU_DynamicFilterSQLExpressions> GetDynamicFilterSQLExpressions(int SQLQuery_Id);
    }
}
