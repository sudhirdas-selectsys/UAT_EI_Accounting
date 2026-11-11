

namespace Accounting.Entities.External.EI
{
    public class EIPost_Req
    {
        public long SubmissionId { get; set; }
        public long QuoteId { get; set; }

        public string InvoiceLine { get; set; }

        public string DirectionType { get; set; }

        public DateTime TransactionDate { get; set; }

        public string TransactionDescription { get; set; }

      

        public decimal OriginalAmount { get; set; }

        public DateTime CheckDate { get; set; }

        public string CheckNumber { get; set; }

        public decimal ApplyAmount { get; set; }
    }
}



