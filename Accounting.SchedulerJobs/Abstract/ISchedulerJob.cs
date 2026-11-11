using Accounting.SchedulerJobs.Models;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface ISchedulerJob
    {
        Task ExecuteAsync(SchedulerClientJobs jobConfig, CancellationToken cancellationToken, string inputField, string createdOrUpdatedBy);
    }
}
