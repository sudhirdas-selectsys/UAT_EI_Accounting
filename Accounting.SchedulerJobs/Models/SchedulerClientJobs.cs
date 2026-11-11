namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobs
    {
        public long Job_Id { get; set; }
        public long SchedulerClientJob_Id { get; set; }

        public long Client_Id { get; set; }

        public string CompanyName { get; set; }
        public byte[]? ClientLogo { get; set; }

        public string JobName { get; set; }

        public string JobType { get; set; }

        public string CronExpression { get; set; }

        public string? TimeZone { get; set; }

        public bool Status { get; set; }
    }
}
