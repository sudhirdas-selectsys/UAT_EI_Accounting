using Accounting.Entities.DB.Transactions;

namespace Accounting.Entities.Internal
{
    public class InvoicePaymentDetails
    {
        public InvoicePaymentDetails()
        {
            InvoicePaymentDetailsList = new List<InvoicePaymentDetails>();
        }
        public long Invoice_Id { get; set; }
        public string TransactionType { get; set; }
        public string LineItem { get; set; }
        public double AppliedAmount { get; set; }
        public long Payment_Id { get; set; }
        public string ReferenceNumber { get; set; }
        public byte PaymentDirection_Id { get; set; }
        public bool Paid { get; set; }
        public byte Direction_Id { get; set; }
        public double RemainingBalance { get; set; }
        public double Amount { get; set; }
        public bool Status { get; set; }
        //added for Transaction History..
        public string AgentName { get; set; }
        public byte PaymentType_Id { get; set; }
        public int OperatingBank_Id { get; set; }
        public byte PaymentStatus_Id { get; set; }
        public long PaymentApplicationLineItem_Id{ get; set; }
        public string PolicyNumber { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string EntityName { get; set; }
        public PolicyDetails PolicyDetails { get; set; }
        public List<InvoicePaymentDetails> InvoicePaymentDetailsList { get; set; }
        public double TotalAppliedAmount { get; set; }
    }
}
