using Accounting.Entities.DB.Transactions;

namespace IMSOnline.DBUtilities.DB.Interfaces
{
    public interface IReportOutputTablesRepository
    {
        List<ReportOutputTables> GetReportOutputTables(long SQLQuery_Id);
    }
}
