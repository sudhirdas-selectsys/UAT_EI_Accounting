namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobEmailSettings
    {
        public SchedulerClientJobSMTPSettings schedulerClientJobSMTPSettings { get; set; }
        public SchedulerClientJobEWSSettings schedulerClientJobEWSSettings { get; set; }
    }
}
