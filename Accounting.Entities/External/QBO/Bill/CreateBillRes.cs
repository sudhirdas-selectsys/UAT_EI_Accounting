namespace Accounting.Entities.External.QBO.Bill
{
    public class CreateBillRes
    {
        public Bill Bill { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }

    }
    public class Error
    {
        public string Message { get; set; }
        public string Detail { get; set; }
        public string code { get; set; }
    }

    public class Fault
    {
        public List<Error> Error { get; set; }
        public string type { get; set; }
    }

    public class Bill
    {
        public string DueDate { get; set; }
        public VendorAddr VendorAddr { get; set; }
        public double Balance { get; set; }
        public string domain { get; set; }
        public bool sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public MetaData MetaData { get; set; }
        public string TxnDate { get; set; }
        public CurrencyRef CurrencyRef { get; set; }
        public List<Line> Line { get; set; }
        public VendorRef VendorRef { get; set; }
        public APAccountRef APAccountRef { get; set; }
        public double TotalAmt { get; set; }
    }

    public class CurrencyRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public class LastModifiedByRef
    {
        public string value { get; set; }
    }

    public class MetaData
    {
        public DateTime CreateTime { get; set; }
        public LastModifiedByRef LastModifiedByRef { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }

}
