namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobEmailConfiguration
    {
        public long ClientSchedulerJobEmailConfiguration_Id { get; set; }
        public long SchedulerClientJob_Id { get; set; }
        public string EmailConfiguration { get; set; }
        public string ReportTypes { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}