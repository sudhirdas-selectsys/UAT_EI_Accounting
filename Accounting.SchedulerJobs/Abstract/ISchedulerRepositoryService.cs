using Accounting.SchedulerJobs.Models;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface ISchedulerRepositoryService
    {
        Task<List<SchedulerClientJobs>> GetSchedulerClientJobs();
        Task InsertExecutionLog(SchedulerJobExecutionLog SchedulerJobExecutionLog);
        Task<SchedulerClientJobProcedure> GetClientSchedulerJobProcedures(long SchedulerClientJob_Id);
        Task<SchedulerClientEmailConfigurations> GetSchedulerClientEmailConfigurations(long SchedulerClientJob_Id);
        Task InsertSchedulerJobQueue(SchedulerClientJobQueue clientSchedulerJobQueue);
        Task<Dictionary<string, SchedulerClientJobEmailSettings>> GetAccountingSchedulerEmailProviderSettings(long client_Id, long schedulerClientJob_Id);
    }
}
