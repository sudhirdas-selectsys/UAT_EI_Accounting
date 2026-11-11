namespace Accounting.Entities.External.ePay
{
    public class InvoicesRes
    {
        public InvoicesRes()
        {
            emailAddresses = new List<string>();
            invoiceAttributeMetadata = new List<InvoiceAttributeMetadatum>();
            invoices = new List<Invoice>();
            invoiceItemAttributeMetadata = new List<invoiceItemAttributeMetadatum> { };
        }
        public string payerName { get; set; }
        public List<string> emailAddresses { get; set; }
        public List<InvoiceAttributeMetadatum> invoiceAttributeMetadata { get; set; }
        public List<invoiceItemAttributeMetadatum> invoiceItemAttributeMetadata { get; set; }
        public List<Invoice> invoices { get; set; }
    }
    public class AttributeValue
    {
        public string parameterName { get; set; }
        public string value { get; set; }
    }
    public class Invoice
    {
        public Invoice()
        {
            attributeValues = new List<AttributeValue>();
            invoiceItems = new List<InvoiceItems>();
        }
        public string id { get; set; }
        public string name { get; set; }
        public DateTime? dueDate { get; set; }
        public decimal amount { get; set; }
        public decimal maximumAmount { get; set; }
        public bool allowPartialPayment { get; set; }
        public string divisionId { get; set; }

        public invoiceUnderwriting invoiceUnderwriting { get; set; }

        public List<AttributeValue> attributeValues { get; set; }
        public List<InvoiceItems> invoiceItems { get; set; }
    }
    public class InvoiceAttributeMetadatum
    {
        public int displayOrder { get; set; }
        public string parameterName { get; set; }
        public string displayName { get; set; }
    }

    public class invoiceUnderwriting
    {
        public string billType { get; set; }
        public bool allowInsuredPayment { get; set; }
        public string policyNumber { get; set; }
        public long? agencyId { get; set; }
        public string agencyName { get; set; }
        public string insuredName { get; set; }
        public string insuredEmail { get; set; }
        public string insuredPhone { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime? expirationDate { get; set; }
        public string coverageCode { get; set; }
        public string coverageDescription { get; set; }
        public int flagFinanced { get; set; }
        public string billingType { get; set; }
        public decimal? minimumPremium { get; set; }
        public string? invoiceTypeID { get; set; }
        public string invoiceTypeDescription { get; set; }
        public decimal? totalFees { get; set; }
        public decimal? totalTaxes { get; set; }
        public string customerType { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string dbaName { get; set; }
        public Address address { get; set; }
        public Address mailingAddress { get; set; }
        public Contact carrierContact { get; set; }
        public Contact mgaContact { get; set; }
        public Contact retailAgentContact { get; set; }
    }


    public class Address
    {
        public string suite { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public int country { get; set; }
    }

    public class Contact
    {
        public long? companyID { get; set; }
        public string companyName { get; set; }
        public string companyNAIC { get; set; }
        public string phone { get; set; }
        public string suite { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public int country { get; set; }
    }



    public class invoiceItemAttributeMetadatum
    {

    }
    public class InvoiceItems
    {

    }
}