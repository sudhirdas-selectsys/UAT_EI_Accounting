namespace Accounting.Entities.External.EI
{
    public class TransactionDetails
    {
        public decimal BrokerCommission { get; set; }
        public decimal BrokerPercentage { get; set; }
        public decimal GrossHouse { get; set; }
        public decimal GrossPremium { get; set; }
        public decimal HousePercentage { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal NetHouse { get; set; }
        public int SubmissionId { get; set; }
        public decimal TotalAP { get; set; }
        public decimal TotalAPBalance { get; set; }
        public decimal TotalAPPaid { get; set; }
        public decimal TotalAR { get; set; }
        public decimal TotalARBalance { get; set; }
        public decimal TotalARPaid { get; set; }
    }
}
