namespace Accounting.Entities.External.QBO.Bill
{
    public class ReadByIdRes
    {
        public Bill Bill { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }
    }
}
