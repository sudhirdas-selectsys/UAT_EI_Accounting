namespace Accounting.Entities.DB.Transactions
{
    public class Invoices : BaseEntity
    {
        public Invoices()
        {
            InvoicesDetailsList = new List<InvoiceDetails>();
        }
        public string InvoiceNo { get; set; }
        //public int? InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }
        public string InvoiceNote { get; set; }
        public bool Paid { get; set; }
        public DateTime? PaidOn { get; set; }
        public DateTime InvoiceDueDate { get; set; }
        public DateTime BillDate { get; set; }
        public double Amount { get; set; }
        public double RemainingBalance { get; set; }
        public DateTime? InvoiceEffectiveDate { get; set; }
        //not mapped
        public long Entity_Id { get; set; }
        public long Schedule_Id { get; set; }
        public byte EntityType_Id { get; set; }
        public double PaidAmount { get; set; }
        public long Payment_Id { get; set; }
        public InvoiceDetails InvoiceDetails { get; set; }
        public List<InvoiceDetails> InvoicesDetailsList { get; set; }
        public string AgencyName { get; set; }
        public string InsuredsName { get; set; }
        public string PolicyNumber  { get; set; }
        public string TransactionType { get; set; }
        public string BillingType { get; set; }
        public bool Is_Financed { get; set; }
        public bool? Is_ClientSpecificDueDate { get; set; }
        public string ClientSpecificDueDateRule { get; set; } 
        public DateTime ClientSpecificInvoiceEffectiveDate { get; set; }
        public string FinanceName { get; set; }
    }
}
