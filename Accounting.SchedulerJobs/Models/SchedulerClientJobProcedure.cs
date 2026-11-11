namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobProcedure
    {
        public long SchedulerClientJobProcedure_Id { get; set; }
        public long SchedulerClientJob_Id { get; set; }
        public long Job_Id { get; set; }
        public string CommandText { get; set; }
        public bool Is_Procedure { get; set; }
        public int ExecutionOrder { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public List<SchedulerClientJobProcedureParameter> Parameters { get; set; }
    }
}