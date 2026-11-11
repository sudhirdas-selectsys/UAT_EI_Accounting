namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerJobExecutionLog
    {
        public string? CreatedOrUpdatedBy { get; set; }
        public long SchedulerClientJob_Id { get; set; }

        public long Job_Id { get; set; }

        public long Client_Id { get; set; }

        public string JobName { get; set; }

        public DateTime ExecutionStartTime { get; set; }

        public DateTime ExecutionEndTime { get; set; }

        public int ExecutionDurationInSeconds { get; set; }

        public bool ExecutionStatus { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
