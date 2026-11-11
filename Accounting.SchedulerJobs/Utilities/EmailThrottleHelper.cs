using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Utilities;

public static class EmailThrottleHelper
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static int _emailCount = 0;
    private static DateTime _windowStart = DateTime.Now;

    public static async Task WaitForSlotAsync(SchedulerClientJobs jobConfig, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            bool acquired = false;
            bool released = false;
            try
            {
                await _semaphore.WaitAsync(cancellationToken); // cancellable 
                acquired = true;

                if ((DateTime.Now - _windowStart).TotalMinutes >= ConfigurationHelper.EmailRateLimitWindowMinutes)
                {
                    _windowStart = DateTime.Now;
                    _emailCount = 0;
                    FileLogger.LogInformation($"EmailThrottleHelper: Window reset. New window starts at {_windowStart} : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                }

                if (_emailCount < ConfigurationHelper.MaxEmailsPerMinute)
                {
                    _emailCount++;
                    _semaphore.Release();
                    released = true;
                    FileLogger.LogInformation($"EmailThrottleHelper: Slot granted. _emailCount={_emailCount}, _windowStart={_windowStart} : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName}");
                    return;
                }

                var waitTime = _windowStart.AddMinutes(ConfigurationHelper.EmailRateLimitWindowMinutes) - DateTime.Now;
                FileLogger.LogInformation($"EmailThrottleHelper: Throttled. _emailCount={_emailCount}, waitTime={waitTime.TotalSeconds}s");
                if (waitTime <= TimeSpan.Zero)
                {
                    waitTime = TimeSpan.FromMilliseconds(50);
                }

                _semaphore.Release();
                released = true;
                await Task.Delay(waitTime, cancellationToken); // cancellable 
            }
            catch (OperationCanceledException ex)
            {
                FileLogger.LogError($"EmailThrottleHelper: WaitForSlotAsync cancelled via CancellationToken: Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"EmailThrottleHelper: WaitForSlotAsync : Client_Id: {jobConfig?.Client_Id}: SchedulerClientJob_Id: {jobConfig?.SchedulerClientJob_Id}: jobName: {jobConfig?.JobName} Exception: {ex} Message: {ex.Message}");
                throw;
            }
            finally
            {
                // Only release if we acquired but didn't release yet
                if (acquired && !released)
                {
                    _semaphore.Release();
                }
            }
        }
    }
}