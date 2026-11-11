using Accounting.Entities.External.QBO.ChartOfAccounts;

namespace Accounting.Entities.External.QBO.Vendor
{
    public class CreateVendorRes
    {
        public Vendor Vendor { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }
    }
    public class Vendor
    {
        public BillAddr BillAddr { get; set; }
        public decimal Balance { get; set; }
        public decimal BillRate { get; set; }
        public bool Vendor1099 { get; set; }
        public CurrencyRef CurrencyRef { get; set; }
        public decimal CostRate { get; set; }
        public string domain { get; set; }
        public bool sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public MetaData MetaData { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public string PrintOnCheckName { get; set; }
        public bool Active { get; set; }
        public string V4IDPseudonym { get; set; }
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
        public string CompanyName { get; set; }
        public string TaxIdentifier { get; set; }
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
}
