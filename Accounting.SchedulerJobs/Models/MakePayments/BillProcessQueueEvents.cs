namespace Accounting.SchedulerJobs.Models.MakePayments
{
    public class BillProcessQueueEvents
    {
        public long WorkFlowEvent_Id { get; set; }
        public long BillProcess_Id { get; set; }
        public long Client_Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public byte WorkFlowEventType_Id { get; set; }
        public string WorkFlowEventType { get; set; } = string.Empty;
        public int WorkFlowStatus_Id { get; set; }
        public string WorkFlowStatus { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public DateTime NextRetryOn { get; set; }
        public string ServiceResponseMessage {  get; set; } = string.Empty;

    }
}
