namespace Accounting.SchedulerJobs.Models
{
    public class SchedulerClientJobProcedureParameter
    {
        public long ClientSchedulerJobProcedureParameter_Id { get; set; }
        public long SchedulerClientJobProcedure_Id { get; set; }
        public string ParameterDataType { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public bool IsOutputParameter { get; set; }
        public bool IsDefaultValue { get; set; }
        public bool IsDynamic { get; set; }
        public string DynamicSource { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}