namespace Accounting.SchedulerJobs.Models.MakePayments
{
    public class BillProcessRetryConfiguration
    {
        public int RetryNumber { get; set; }
        public byte DelayInMinutes { get; set; }
    }
}
