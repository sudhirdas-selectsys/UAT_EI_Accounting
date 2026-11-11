namespace Accounting.Entities.External.QBO.Vendor
{
    public class ReadByIdRes
    {
        public Vendor Vendor { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }
    }
}
