namespace Accounting.Entities.Internal
{
    public class ARList
    {
        public long Invoice_Id { get; set; }
        public long PolicyDetail_Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string AgencyName { get; set; }
        public string InsuredsName { get; set; }
        public string FinanceCompany { get; set; }
        public string PolicyNumber { get; set; }
        public string TransactionTypes { get; set; }
        public string TransactionNumber { get; set; }
        public double TotalPremium { get; set; }
        public double PaidAmount { get; set; }
        public double RemainingBalance { get; set; }
        public DateTime TransactionEffectiveDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string MarketProvider { get; set; }
        public byte EntityType_Id { get; set; }
        public long Entity_Id { get; set; }
        public long Payment_Id { get; set; }
        public bool Paid { get; set; }
    }
}
