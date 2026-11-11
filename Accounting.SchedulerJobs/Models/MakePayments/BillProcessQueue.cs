namespace Accounting.SchedulerJobs.Models.MakePayments
{
    public class BillProcessQueue
    {
        public string CreatedBy { get; set; }
        public long BillProcess_Id { get; set; }
        public long Client_Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public int BillProcessStatus_Id { get; set; }
        public string BillProcessStatus { get; set; } = string.Empty;
        public byte CurrentWorkFlowEventType_Id { get; set; }
        public string CurrentWorkFlowEventType { get; set; } = string.Empty;
        public long Report_Id { get; set; }
        public byte FileType_Id { get; set; }
        public byte Direction_Id { get; set; }
        public string SelectedInvoicesJson { get; set; } = string.Empty;
        public string BillDetailJson { get; set; } = string.Empty;
        public string Email {  get; set; }
        public long Bill_Id { get; set; }
        public string BillNumber { get; set; }
    }
}
