namespace Accounting.Entities.DB.Transactions
{
    public class PaymentApplicationLineItems : BaseEntity
    {
        public long PaymentApplicationLineItem_Id { get; set; }
        public long PaymentApplication_Id { get; set; }
        public long? InvoiceLineItem_Id { get; set; }
        public double AppliedAmount { get; set; }
        public double UnAppliedAmount { get; set; }
        public bool? Is_SplitPayment { get; set; }
        public byte? SubType_Id { get; set; }
        public bool isNegativeContext { get; set; }
        // not mapped
        public string InvoiceNo { get; set; }
        public string PolicyNumber { get; set; }
        public string AgencyName { get; set; }
        public string InsuredsName { get; set; }
        public string BillingType { get; set; }
        public string LineItemName { get; set; }
        public string Notes { get; set; }
        public DateTime? PolicyEffectiveDate { get; set; }
        public DateTime? PolicyExpirationDate { get; set; }
        public double OriginalInvoiceAmount { get; set; }
        public double RemainingBalance { get; set; }
        public byte AccountingType_Id { get; set; }
    }
}
