using Accounting.Entities.DB.Transactions;
using IMSOnline.DBUtilities.DB.Interfaces;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ReportOutputTablesRepository: IReportOutputTablesRepository
    {
        public List<ReportOutputTables> GetReportOutputTables(long SQLQuery_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@SQLQuery_Id",SQLQuery_Id),
            };
            return SQLHelper.GetObjectList(new ReportOutputTables(), "USP_Accounting_GetReportOutputTables", sqlParameters);
        }
    }
}
