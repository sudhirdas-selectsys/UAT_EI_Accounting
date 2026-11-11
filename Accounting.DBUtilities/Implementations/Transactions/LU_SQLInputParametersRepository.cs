using Accounting.DBUtilities;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class LU_SQLInputParametersRepository: ILU_SQLInputParametersRepository
    {
        public List<LU_SQLInputParameters> GetSQLQueryInputParameters(int SQLQuery_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
           {
                new SqlParameter("@SQLQuery_Id",SQLQuery_Id),
           };
            return SQLHelper.GetObjectList(new LU_SQLInputParameters(), "USP_Accounting_GetSQLInputParameters", sqlParameters);
        }
        public List<LU_SQLInputParameters> GetDependencySQLQueryInputParameters(long SQLParameter_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
           {
                new SqlParameter("@SQLParameter_Id",SQLParameter_Id),
           };
            return SQLHelper.GetObjectList(new LU_SQLInputParameters(), "USP_Accounting_GetDependencySQLInputParameters", sqlParameters);
        }
    }
}
