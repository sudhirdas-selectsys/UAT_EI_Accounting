using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayPaymentResponse
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("response")]
        public CorePayPaymentResponse response { get; set; }

        [JsonPropertyName("Messages")]
        public List<CorePayPaymentLinkMessage> Messages { get; set; } = new();

        [JsonPropertyName("Status")]
        public string Status { get; set; }
    }

    public class CorePayPaymentResponse
    {
        [JsonPropertyName("paymentLink")]
        public CorePayPaymentLinkResponse? paymentLink { get; set; }
    }

    public class CorePayPaymentLinkResponse
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("paymentLinkId")]
        public string? PaymentLinkId { get; set; }

        [JsonPropertyName("paymentLink")]
        public CorePayPaymentLinkResponseData paymentLink { get; set; }
    }

    public class CorePayPaymentLinkResponseData
    {
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("expiresAt")]
        public string? ExpiresAt { get; set; }

        [JsonPropertyName("singleUse")]
        public bool SingleUse { get; set; }
    }

    public class CorePayPaymentLinkMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("allowedValues")]
        public List<string> AllowedValues { get; set; } = new();

        [JsonPropertyName("validationType")]
        public string ValidationType { get; set; } = string.Empty;
    }
}
