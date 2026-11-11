using Accounting.Entities.DB.Lookups;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILU_SQLInputParametersRepository
    {
        List<LU_SQLInputParameters> GetSQLQueryInputParameters(int SQLQuery_Id);
        List<LU_SQLInputParameters> GetDependencySQLQueryInputParameters(long SQLParameter_Id);
    }
}
