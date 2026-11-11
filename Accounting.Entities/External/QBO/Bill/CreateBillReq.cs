using Accounting.Entities.External.QBO.Journal;

namespace Accounting.Entities.External.QBO.Bill
{
    public class CreateBillReq
    {
        public VendorRef VendorRef { get; set; }
        public List<Line> Line { get; set; }
        public string TxnDate { get; set; }
        public string DueDate { get; set; }
        public string DocNumber { get; set; }
        public string? PrivateNote { get; set; }
    }
    public class Line
    {
        public string Id { get; set; }
        public int LineNum { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public List<object> LinkedTxn { get; set; }
        public string DetailType { get; set; }
        public AccountBasedExpenseLineDetail AccountBasedExpenseLineDetail { get; set; }
        public List<object> CustomExtensions { get; set; }
    }
    public class AccountBasedExpenseLineDetail
    {
        public AccountRef AccountRef { get; set; }
        public string BillableStatus { get; set; }
        public TaxCodeRef TaxCodeRef { get; set; }
    }
    public class VendorAddr
    {
        public string Id { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountrySubDivisionCode { get; set; }
        public string PostalCode { get; set; }
    }

    public class VendorRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }
    public class TaxCodeRef
    {
        public string value { get; set; }
    }
    public class AccountRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public class APAccountRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }
}
