namespace Accounting.Entities.External.QBO.Vendor
{
    public class CreateVendorReq
    {
        public PrimaryPhone PrimaryPhone { get; set; }
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
        public WebAddr WebAddr { get; set; }
        public string DisplayName { get; set; }
        public string FamilyName { get; set; }
        public string TaxIdentifier { get; set; }
        public string AcctNum { get; set; }
        public string CompanyName { get; set; }
        public BillAddr BillAddr { get; set; }
        public string GivenName { get; set; }
        public string PrintOnCheckName { get; set; }
    }
    public class BillAddr
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string Line3 { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string PostalCode { get; set; }
        public string CountrySubDivisionCode { get; set; }
    }
    public class PrimaryEmailAddr
    {
        public string Address { get; set; }
    }
    public class WebAddr
    {
        public string URI { get; set; }
    }
    public class PrimaryPhone
    {
        public string FreeFormNumber { get; set; }
    }
    public class Mobile
    {
        public string FreeFormNumber { get; set; }
    }

}
