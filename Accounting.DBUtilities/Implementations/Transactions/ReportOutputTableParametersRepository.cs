using Accounting.Entities.DB.Transaction;
using IMSOnline.DBUtilities.DB.Interfaces;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ReportOutputTableParametersRepository: IReportOutputTableParametersRepository
    {
        public List<ReportOutputTableParameters> GetReportOutputTableParameters(int ReportOutputTable_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@ReportOutputTable_Id",ReportOutputTable_Id),
            };
            return SQLHelper.GetObjectList(new ReportOutputTableParameters(), "USP_Accounting_GetReportOutputTableParameters", sqlParameters);
        }
    }
}
