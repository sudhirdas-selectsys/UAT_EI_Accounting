using Accounting.Entities.DB.Transaction;

namespace IMSOnline.DBUtilities.DB.Interfaces
{
    public interface IReportOutputTableParametersRepository
    {
         List<ReportOutputTableParameters> GetReportOutputTableParameters(int ReportOutputTable_Id);
    }
}
