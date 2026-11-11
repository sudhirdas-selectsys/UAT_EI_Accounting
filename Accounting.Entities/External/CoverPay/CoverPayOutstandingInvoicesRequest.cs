using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayOutstandingInvoicesRequest
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }
        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; set; }
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;
        [JsonPropertyName("entities")]
        public List<CoverPayEntity> Entities { get; set; }
    }
}
