namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobQueue
    {
        public string? CreatedOrUpdatedBy { get; set; }
        public long? Job_Id { get; set; }
        public long? SchedulerClientJob_Id { get; set; }
        public long? Client_Id { get; set; }
        public string QueueReferenceType { get; set; }
        public string QueueReferenceValue { get; set; }
        public int QueueStatus_Id { get; set; }
        public DateTime? ExecutionStartTime { get; set; }
        public DateTime? ExecutionEndTime { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
