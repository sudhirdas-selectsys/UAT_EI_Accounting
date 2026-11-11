namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobEmailTemplate
    {
        public long ClientSchedulerJobEmailTemplate_Id { get; set; }
        public int Job_Id { get; set; }
        public string TemplateName { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
