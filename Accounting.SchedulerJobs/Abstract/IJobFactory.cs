namespace Accounting.SchedulerJobs.Abstract
{
    public interface IJobFactory
    {
        ISchedulerJob GetJob(string jobName);
    }
}
