using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayTokenResponse
    {
        [JsonPropertyName("data")]
        public CorePayTokenData Data { get; set; } = new();

        [JsonPropertyName("Status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("Messages")]
        public List<CorePayTokenMessage> Messages { get; set; } = new();
    }

    public class CorePayTokenData
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("expiresSeconds")]
        public int ExpiresSeconds { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class CorePayTokenMessage
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
