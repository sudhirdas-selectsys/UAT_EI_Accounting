namespace Accounting.SchedulerJobs.Models
{
    public class InvoiceNoticeLog
    {
        public string? CreatedOrUpdatedBy { get; set; }
        public long? Job_Id { get; set; }
        public long? SchedulerClientJob_Id { get; set; }
        public int? NoticeType_Id { get; set; }
        public long? Client_Id { get; set; }
        public long? PolicyDetails_Id { get; set; }
        public long? Invoice_Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? SentOn { get; set; }
        public bool? IsEmailSent { get; set; }
        public long? EI_Document_Id { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
