using Newtonsoft.Json;

namespace Accounting.Entities.External.CorePay
{
    public class CorePayPaymentRequest
    {
        [JsonProperty("_event")]
        public CorePayPaymentEventDetails Event { get; set; }

        [JsonProperty("source")]
        public CorePayPaymentSourceDetails Source { get; set; }

        [JsonProperty("entity")]
        public List<CorePayPaymentEntityReference> Entity { get; set; }

        [JsonProperty("customer")]
        public CorePayPaymentCustomerDetails Customer { get; set; }

        [JsonProperty("transactionInfo")]
        public CorePayPaymentTransactionDetails TransactionInfo { get; set; }

        [JsonProperty("Messages")]
        public List<CorePayPaymentEventMessage> Messages { get; set; }
    }

    public class CorePayPaymentEventDetails
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }

    public class CorePayPaymentSourceDetails
    {
        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("component")]
        public string Component { get; set; }
    }

    public class CorePayPaymentEntityReference
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ReferenceId")]
        public string ReferenceId { get; set; }
    }

    public class CorePayPaymentCustomerDetails
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class CorePayPaymentTransactionDetails
    {
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("paymentCategory")]
        public string PaymentCategory { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("installment")]
        public CorePayPaymentInstallmentDetails Installment { get; set; }

        [JsonProperty("amount")]
        public CorePayPaymentAmountDetails Amount { get; set; }

        [JsonProperty("lineItems")]
        public List<CorePayPaymentLineItemDetails> LineItems { get; set; }

        [JsonProperty("paymentProcessor")]
        public CorePayPaymentProcessorDetails PaymentProcessor { get; set; }
    }

    public class CorePayPaymentInstallmentDetails
    {
        [JsonProperty("installmentNo")]
        public int InstallmentNo { get; set; }

        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }
    }

    public class CorePayPaymentAmountDetails
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class CorePayPaymentLineItemDetails
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("charges")]
        public List<CorePayPaymentChargeDetails> Charges { get; set; }

        [JsonProperty("invoice")]
        public CorePayPaymentInvoiceDetails Invoice { get; set; }
    }

    public class CorePayPaymentChargeDetails
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public class CorePayPaymentInvoiceDetails
    {
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }
    }

    public class CorePayPaymentProcessorDetails
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMessage")]
        public string ResponseMessage { get; set; }
    }

    public class CorePayPaymentEventMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
