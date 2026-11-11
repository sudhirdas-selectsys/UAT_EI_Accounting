namespace Accounting.Entities.DB.Transactions
{
    public class PaymentApplications : BaseEntity
    {
        public PaymentApplications()
        {
            PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
        }
        public long PaymentApplication_Id { get; set; }
        public long Payment_Id { get; set; }
        public long? Invoice_Id { get; set; }
        public DateTime AppliedDate { get; set; }
        public double TotalAppliedAmount { get; set; }
        public string Notes { get; set; }
        public List<PaymentApplicationLineItems> PaymentApplicationLineItemsList { get; set; }
    }
}
