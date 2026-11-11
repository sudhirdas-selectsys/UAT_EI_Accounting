using Accounting.Entities.DB.Transactions;
namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IReportsRepository
    {
        List<Reports> GetReports(long Client_Id);
        Reports GetReport(long Report_Id);
    }
}
