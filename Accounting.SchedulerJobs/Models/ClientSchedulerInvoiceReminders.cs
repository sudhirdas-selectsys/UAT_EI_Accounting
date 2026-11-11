namespace Accounting.SchedulerJobs.Models
{
    public class ClientSchedulerInvoiceReminders
    {
        public string ClientName
        {
            get;
            set;
        }

        public string ClientEmail
        {
            get;
            set;
        }

        public string ClientLogo
        {
            get;
            set;
        }

        public string AgentName
        {
            get;
            set;
        }

        public string AgentEmail
        {
            get;
            set;
        }

        public long Invoice_Id
        {
            get;
            set;
        }

        public long PolicyDetails_Id
        {
            get;
            set;
        }

        public string PolicyNumber
        {
            get;
            set;
        }

        public string InvoiceNo
        {
            get;
            set;
        }

        public DateTime? DueDate
        {
            get;
            set;
        }

        public string NoticeType
        {
            get;
            set;
        }

        public int DayOffset
        {
            get;
            set;
        }
    }
}
