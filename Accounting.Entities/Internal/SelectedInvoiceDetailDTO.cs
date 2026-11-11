namespace Accounting.Entities.Internal
{
    public class SelectedInvoiceDetailDTO
    {
        public long Invoice_Id { get; set; }
        public long InvoiceDetail_Id { get; set; }
        public double Amount { get; set; }
        public double RemainingBalance { get; set; }
        public bool IsPartial { get; set; }
        public List<long> InvoiceDetail_Ids { get; set; }
    }
}
