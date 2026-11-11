namespace Accounting.SchedulerJobs.Models
{
    public class EmailRecipients
    {
        public List<string> To { get; set; } = new List<string>();

        public List<string> CC { get; set; } = new List<string>();

        public List<string> BCC { get; set; } = new List<string>();
    }
}
