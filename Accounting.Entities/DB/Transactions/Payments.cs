namespace Accounting.Entities.DB.Transactions
{
    public class Payments : BaseEntity
    {
        public Payments()
        {
            PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
            WriteoffPaymentApplicationLineItems = new PaymentApplicationLineItems();
            PolicyDetailsList = new List<PolicyDetails>();
        }
        public long Payment_Id { get; set; }
        public byte? EntityType_Id { get; set; }
        public long Entity_Id { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? PaymentFromDate { get; set; }
        public DateTime? PaymentToDate { get; set; }
        public double Amount { get; set; }
        public byte PaymentType_Id { get; set; }
        public string ReferenceNumber { get; set; }
        public byte PaymentStatus_Id { get; set; }
        public int? OperatingBank_Id { get; set; }
        public string Notes { get; set; }
        public string EntityName { get; set; }
        public double RemainingAmount { get; set; }
        public byte PaymentDirection_Id { get; set; }
        public string ExternalJournalEntryIdentifier { get; set; }
        public byte Direction_Id { get; set; }
        public bool OutstandingBalances { get; set; }
        
        public List<PaymentApplicationLineItems> PaymentApplicationLineItemsList { get; set; }
        public PaymentApplicationLineItems WriteoffPaymentApplicationLineItems { get; set; }
		public Clients Clients { get; set; } 
        public byte SourceType_Id { get; set; }
        public int? JournalIndex { get; set; }
        public List<PolicyDetails> PolicyDetailsList { get; set; }
        public DateTime TxnDate { get; set; }
        public bool IsEdit { get; set; }
        public int CheckPaymentPrintStatus_Id { get; set; }
        public string? MemoDescription { get; set; }
        public string? PayeeNameOnCheck { get; set; }
    }
}
