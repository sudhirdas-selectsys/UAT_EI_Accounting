namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientEmailConfigurations
    {
        public long Job_Id { get; set; }
        public long SchedulerClientJob_Id { get; set; }
        public long ClientSchedulerJobEmailTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailSubjectParameters { get; set; }
        public string EmailBodyParameters { get; set; }
        public string EmailConfiguration { get; set; }
        public string AttachmentTypes { get; set; }
        public List<SchedulerJobEmailAttachments> SchedulerJobEmailAttachments { get; set; }
    }
}
