namespace Accounting.Entities.DB.Transactions
{
    public class InvoiceDetails : BaseEntity
    {
        public long Invoice_Id { get; set; }
        public double Amount { get; set; }
        public double RemainingBalance { get; set; }
        public bool? Paid { get; set; }
        public DateTime? PaidOn { get; set; }
        public long Entity_Id { get; set; }
        public long Payee_Id { get; set; }
        public byte EntityType_Id { get; set; }
        public byte PayeeType_Id { get; set; }
        public byte PayorType_Id { get; set; }
        public long Schedule_Id { get; set; }
        public int BillingType_LineItems_Directions_Id { get; set; }
        public byte LOB_Id { get; set; }
        //not mapped
        public double AgentCommission {  get; set; }
        public double AgentCommissionPercent { get; set; }
        public double HouseCommission { get; set; }
        public double HouseCommissionPercent { get; set; }
        public string EntityName { get; set; }
        public string PayeeName { get; set; }
        public string EntityTypeName { get; set; }
        public string LineTypeName { get; set; }
        public double ActualPremium { get; set; }
        public string Description { get; set; }
        public int LineItems_Directions_Id { get; set; }
        public long LineItem_Id { get; set; }
        public byte LineItemType_Id { get; set; }
        public byte Direction_Id { get; set; }
        public double ARCollected { get; set; }
        public string LineItemName { get; set; }
        public PaymentApplicationLineItems PaymentApplicationLineItems { get; set; }
        public bool Is_Commissionable { get; set; }
        public DateTime InvoiceDueDate { get; set; }
        //LIST
        public string AgencyName { get; set; }
        public string InsuredName { get; set; }
        public string PolicyNumber { get; set; }
        public string TransactionType { get; set; }

        public string BillType { get; set; }
        public bool Is_Financed { get; set; }
        public string InvoiceNo { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime BillDate { get; set; }
        public bool InvoicePaid { get; set; }
        public DateTime? InvoicePaidOn { get; set; }
        public DateTime InvoiceCreatedOn { get; set; }
        public bool TreatAsPremium { get; set; }
        public string FinanceName { get; set; }
    }
}
