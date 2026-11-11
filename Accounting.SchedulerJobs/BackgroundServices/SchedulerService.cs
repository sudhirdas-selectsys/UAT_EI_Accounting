using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Utilities;

namespace Accounting.SchedulerJobs.BackgroundServices
{
    public class SchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SchedulerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //FileLogger.LogInformation($"SchedulerService: ExecuteAsync Started");
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ISchedulerRepositoryService>();
                    var factory = scope.ServiceProvider.GetRequiredService<IJobFactory>();
                    var jobs = ConfigurationHelper.ClientSchedulerJobs;

                    foreach (var jobConfig in jobs)
                    {
                        //if (true) // Uncomment this section when testing in the local environment.
                        if (string.IsNullOrEmpty(jobConfig?.CronExpression))
                        {
                            continue;
                        }

                        if (!CronHelper.IsDue(jobConfig.CronExpression, jobConfig?.TimeZone ?? string.Empty))
                        {
                            continue;
                        }

                        if (!ConfigurationHelper.RunningJobs.TryAdd(jobConfig.SchedulerClientJob_Id,true))
                        {
                            continue;
                        }

                        await ExecuteJobAsync(jobConfig, factory, repository, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.LogError($"SchedulerService: ExecuteAsync Exception: {ex} Message: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(ConfigurationHelper.SchedulerPollingIntervalInSeconds), stoppingToken);
            }
        }

        private async Task ExecuteJobAsync(SchedulerClientJobs jobConfig, IJobFactory factory, ISchedulerRepositoryService repository, CancellationToken cancellationToken)
        {
            DateTime startTime = DateTime.Now;
            bool executionStatus = false;
            string? errorMessage = null;
            ISchedulerJob job = null;
            try
            {
                FileLogger.LogInformation($"SchedulerService: ExecuteJobAsync Started : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                job = factory.GetJob(jobConfig?.JobType ?? string.Empty);
                if (job != null) 
                {
                    await job.ExecuteAsync(jobConfig, cancellationToken, string.Empty, "Auto");
                    executionStatus = true;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"SchedulerService: ExecuteJobAsync Exception: {ex} Message: {ex.Message}");
                errorMessage = ex.ToString();
                await Utils.SendExceptionEmail(jobConfig, "Auto", startTime, ex);
            }
            finally
            {
                ConfigurationHelper.RunningJobs.TryRemove(jobConfig.SchedulerClientJob_Id, out _);
                if(job != null)
                {
                    await repository.InsertExecutionLog(
                        new SchedulerJobExecutionLog
                        {
                            CreatedOrUpdatedBy = "Auto",
                            SchedulerClientJob_Id = jobConfig.SchedulerClientJob_Id,
                            Job_Id = jobConfig.Job_Id,
                            Client_Id = jobConfig.Client_Id,
                            JobName = jobConfig.JobName,
                            ExecutionStartTime = startTime,
                            ExecutionEndTime = DateTime.Now,
                            ExecutionStatus = executionStatus,
                            ErrorMessage = errorMessage
                        }
                    );
                }
            }
        }
    }
}
