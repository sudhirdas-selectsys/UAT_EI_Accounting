namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobSMTPSettings
    {
      
        public string SMTPHost { get; set; }

        public int SMTPPort { get; set; }

        public bool EnableSSL { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }
    }
}
