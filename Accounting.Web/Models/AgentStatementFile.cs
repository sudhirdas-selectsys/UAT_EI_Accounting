namespace Accounting.Web.Models
{
    public class AgentStatementFile
    {
        public long AgentDetailId { get; set; }
        public long AgentReferenceId { get; set; }

        public string FileName { get; set; }

        public byte[] FileBytes { get; set; }
        public string AgencyName { get; set; }
    }
}
