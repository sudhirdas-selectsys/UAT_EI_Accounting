using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Utilities;

namespace Accounting.SchedulerJobs.BackgroundServices
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _provider;

        public JobFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ISchedulerJob? GetJob(string jobType)
        {
            try
            {
                FileLogger.LogInformation($"JobFactory: GetJob Started JobType: {jobType}");
                if (string.IsNullOrWhiteSpace(jobType))
                {
                    return null;
                }

                Type? type = Type.GetType(jobType);

                if (type == null)
                {
                    return null;
                }

                return _provider.GetService(type) as ISchedulerJob;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"JobFactory: GetJob Exception: {ex} Message: {ex.Message}");
                return null;
            }
        }
    }
}
