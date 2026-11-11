using Accounting.SchedulerJobs.Models;
using System.Data;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface ICommonRepositoryService
    {
        Task<List<T>> ExecuteDyanmicGetObjectListAsync<T>(SchedulerClientJobProcedure procedure, object sourceObject) where T : class, new();
        Task<DataSet> ExecuteDyanmicGetDataSetAsync(SchedulerClientJobProcedure procedure, object sourceObject);
    }
}
