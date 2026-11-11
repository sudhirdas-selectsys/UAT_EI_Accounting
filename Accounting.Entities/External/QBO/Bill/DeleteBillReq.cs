namespace Accounting.Entities.External.QBO.Bill
{
    public class DeleteBillReq
    {
        public string Id { get; set; }
        public string SyncToken { get; set; }
    }
}
