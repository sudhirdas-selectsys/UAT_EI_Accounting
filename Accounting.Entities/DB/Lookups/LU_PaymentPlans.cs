namespace Accounting.Entities.DB.Lookups
{
    public class LU_PaymentPlans : BaseLookupEntity
    {
        public int PaymentPlan_Id { get; set; }
        public long? Carrier_Id { get; set; }
        public string CarrierName { get; set; }
        public double DownPaymentPercent { get; set; }
        public double DownPaymentMinimum { get; set; }
        public byte NumberOfPayments { get; set; }
        public int DownPaymentTermDays { get; set; }
        public byte PaymentAnchorType_Id { get; set; }
        public string PaymentAnchorType { get; set; }
        public byte InstallmentBasisType_Id { get; set; }
        public string InstallmentBasisType { get; set; }
        public byte IntervalUnitType_Id { get; set; }
        public string IntervalUnitType { get; set; }
        public byte IntervalValue { get; set; }
        public int? FirstPaymentOffsetDays { get; set; }
        public byte? FixedDayOfMonth { get; set; }
        public bool IsSetBillDateEqualDue { get; set; }
        public byte LeadTime { get; set; }
        public bool IsInstallmentFeeSetup { get; set; }
        public bool IsAutoSendInvoices { get; set; }
        public bool IsActive { get; set; }
        public DateTime FirstRemainingInstallmentDueDate { get; set; }

    }
}
