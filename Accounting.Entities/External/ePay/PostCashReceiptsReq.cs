namespace Accounting.Entities.External.ePay
{
    public class PostCashReceiptsReq
    {
        public int batchId { get; set; }
        public List<PostInvoicesRequestModel> postInvoicesRequestModels { get; set; }
    }
    public class PaidInvoice
    {
        public string id { get; set; }
        public double paidAmount { get; set; }
        public string comment { get; set; }
        public Dictionary<string, object> AttributeValues { get; set; }
        public Dictionary<string, object> SearchAttributeValues { get; set; }
        public string division { get; set; }
    }
    public class PostInvoicesRequestModel
    {
        public long transactionId { get; set; }
        public PaymentAttributes PaymentAttributes { get; set; }
        public List<PaidInvoice> paidInvoices { get; set; }
        public int processorId { get; set; }
        public string checkNumber { get; set; }
        public string checkDate { get; set; }
        public bool paidByThirdParty { get; set; }
        public string thirdPartyPayerId { get; set; }
    }
    public class PaymentAttributes
    {
        public string accountNumber { get; set; }
        public string postalCode { get; set; }
    }
}
