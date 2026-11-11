using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ReportFileDownloadSettingsRepository : IReportFileDownloadSettingsRepository
    {
        public List<ReportFileDownloadSettings> GetReportFileDownloadSettings(long Report_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@Report_Id",Report_Id),
            };
            return SQLHelper.GetObjectList(new ReportFileDownloadSettings(), "USP_Accounting_GetReportFileDownloadSettings", sqlParameters);
        }
    }
}
