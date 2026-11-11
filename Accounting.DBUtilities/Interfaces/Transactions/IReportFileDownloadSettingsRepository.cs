using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IReportFileDownloadSettingsRepository
    {
        List<ReportFileDownloadSettings> GetReportFileDownloadSettings(long Report_Id);
    }
}
