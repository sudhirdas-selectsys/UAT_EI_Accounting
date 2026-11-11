namespace Accounting.Entities.External.QBO.Bill
{
    public class DeleteBillRes
    {
        public BillInfo Bill { get; set; }
        public DateTime time { get; set; }
    }
    public class BillInfo
    {
        public string domain { get; set; }
        public string status { get; set; }
        public string Id { get; set; }
    }
}
