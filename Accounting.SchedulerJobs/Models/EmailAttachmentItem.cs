namespace Accounting.SchedulerJobs.Models
{
    public class EmailAttachmentItem
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string ContentType { get; set; }
    }
}
