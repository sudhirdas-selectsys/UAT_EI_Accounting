using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ReportsRepository : IReportsRepository
    {
        public List<Reports> GetReports(long Client_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@Client_Id",Client_Id),
            };
            return SQLHelper.GetObjectList(new Reports(), "USP_Accounting_GetReportsByClient", sqlParameters);
        }
        public Reports GetReport(long Report_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
                        {
                new SqlParameter("@Report_Id",Report_Id),
                        };
            return SQLHelper.GetObject(new Reports(), "USP_Accounting_GetReport", sqlParameters);
        }
    }
}
