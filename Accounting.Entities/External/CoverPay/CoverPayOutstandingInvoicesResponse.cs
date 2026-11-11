using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayOutstandingInvoicesResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; }

        [JsonPropertyName("data")]
        public CoverPayOutstandingInvoicesData Data { get; set; }
    }

    public class CoverPayOutstandingInvoicesData
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonPropertyName("source")]
        public CoverPaySource Source { get; set; }

        [JsonPropertyName("billingAccounts")]
        public List<CoverPayBillingAccount> BillingAccounts { get; set; }
    }

    public class CoverPaySource
    {
        [JsonPropertyName("sourceSystemCode")]
        public string SourceSystemCode { get; set; }

        [JsonPropertyName("referenceId")]
        public string ReferenceId { get; set; }
    }

    public class CoverPayBillingAccount
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("billingType")]
        public string BillingType { get; set; }

        [JsonPropertyName("payer")]
        public CoverPayPayer Payer { get; set; }

        [JsonPropertyName("customer")]
        public CoverPayCustomer Customer { get; set; }

        [JsonPropertyName("policy")]
        public CoverPayPolicy Policy { get; set; }

        [JsonPropertyName("contacts")]
        public List<CoverPayContact> Contacts { get; set; }

        [JsonPropertyName("invoices")]
        public List<CoverPayInvoice> Invoices { get; set; }

        [JsonPropertyName("paymentOptions")]
        public CoverPayPaymentOptions PaymentOptions { get; set; }

        [JsonPropertyName("entities")]
        public List<CoverPayEntity> Entities { get; set; }
    }

    public class CoverPayPayer
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }
    }

    public class CoverPayCustomer
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("addresses")]
        public List<CoverPayAddress> Addresses { get; set; }
    }

    public class CoverPayPolicy
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("policyNumber")]
        public string PolicyNumber { get; set; }

        [JsonPropertyName("insuredName")]
        public string InsuredName { get; set; }

        [JsonPropertyName("policyType")]
        public string PolicyType { get; set; }

        [JsonPropertyName("lineOfBusiness")]
        public CoverPayLineOfBusiness LineOfBusiness { get; set; }

        [JsonPropertyName("effectiveDate")]
        public DateTime EffectiveDate { get; set; }

        [JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }
    }

    public class CoverPayLineOfBusiness
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }
    }

    public class CoverPayAddress
    {
        [JsonPropertyName("addressType")]
        public string AddressType { get; set; }

        [JsonPropertyName("address1")]
        public string Address1 { get; set; }

        [JsonPropertyName("address2")]
        public string Address2 { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }

    public class CoverPayContact
    {
        [JsonPropertyName("contactType")]
        public string ContactType { get; set; }

        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }

        [JsonPropertyName("companyNAIC")]
        public string CompanyNAIC { get; set; }

        [JsonPropertyName("addresses")]
        public List<CoverPayAddress> Addresses { get; set; }
    }

    public class CoverPayInvoice
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonPropertyName("dates")]
        public CoverPayInvoiceDates Dates { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("paidAmount")]
        public double PaidAmount { get; set; }

        [JsonPropertyName("outstandingAmount")]
        public double OutstandingAmount { get; set; }

        [JsonPropertyName("paymentEligibility")]
        public CoverPayPaymentEligibility PaymentEligibility { get; set; }

        [JsonPropertyName("paymentSchedule")]
        public List<CoverPayPaymentSchedule> PaymentSchedule { get; set; }
    }

    public class CoverPayInvoiceDates
    {
        [JsonPropertyName("invoiceDate")]
        public string InvoiceDate { get; set; }

        [JsonPropertyName("dueDate")]
        public string DueDate { get; set; }
    }

    public class CoverPayPaymentEligibility
    {
        [JsonPropertyName("payable")]
        public bool Payable { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    public class CoverPayPaymentSchedule
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("installmentNumber")]
        public int InstallmentNumber { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("dueDate")]
        public string DueDate { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("paidAmount")]
        public double PaidAmount { get; set; }

        [JsonPropertyName("outstandingAmount")]
        public double OutstandingAmount { get; set; }

        [JsonPropertyName("lineItems")]
        public List<CoverPayLineItem> LineItems { get; set; }
    }

    public class CoverPayLineItem
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("outstandingAmount")]
        public double OutstandingAmount { get; set; }

        [JsonPropertyName("charges")]
        public List<CoverPayCharge> Charges { get; set; }

        [JsonPropertyName("totalOutstandingAmount")]
        public double TotalOutstandingAmount { get; set; }
    }

    public class CoverPayCharge
    {
        [JsonPropertyName("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("outstandingAmount")]
        public double OutstandingAmount { get; set; }
    }

    public class CoverPayPaymentOptions
    {
        [JsonPropertyName("allowPartialPayment")]
        public bool AllowPartialPayment { get; set; }

        [JsonPropertyName("paymentMethods")]
        public List<CoverPayPaymentMethod> PaymentMethods { get; set; }
    }

    public class CoverPayPaymentMethod
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class CoverPayEntity
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
