using Cronos;

namespace Accounting.SchedulerJobs.Utilities
{
    public static class CronHelper
    {
        public static bool IsDue(string cronExpression, string timeZone)
        {
            try
            {
                var cron = CronExpression.Parse(cronExpression,CronFormat.IncludeSeconds);
                TimeZoneInfo tz = string.IsNullOrEmpty(timeZone) ? TimeZoneInfo.Utc : TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                DateTimeOffset now = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, tz);
                DateTimeOffset? previous = cron.GetPreviousOccurrence(now, tz);

                if (!previous.HasValue)
                {
                    return false;
                }                    

                return (now - previous.Value).TotalSeconds < ConfigurationHelper.SchedulerPollingIntervalInSeconds;
            }
            catch
            {
                return false;
            }
        }
    }
}
