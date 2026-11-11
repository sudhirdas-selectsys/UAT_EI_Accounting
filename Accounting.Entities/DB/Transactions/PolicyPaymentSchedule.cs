namespace Accounting.Entities.Internal
{
    public class PolicyPaymentSchedule : BaseEntity
    {
        public long PolicyPaymentSchedule_Id { get; set; }
        public long PolicyDetail_Id { get; set; }
        public long PaymentPlan_Id { get; set; }
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime BillDate { get; set; }
        public byte? InstallmentNo { get; set; }
        public bool IsDownPayment { get; set; }
        public bool Paid { get; set; }
        public DateTime? PaidOn { get; set; }
        public DateTime? InvoiceCreatedDate { get; set; }
        public string Notes { get; set; }
    }
}
