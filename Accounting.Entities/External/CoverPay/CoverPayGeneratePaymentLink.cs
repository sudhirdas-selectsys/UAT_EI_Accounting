using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayGeneratePaymentLink
    {
        public string requestId { get; set; }
        public string idempotencyKey { get; set; }
        public context context { get; set; }
        public List<entities> entities { get; set; }
        public paymentLink paymentLink { get; set; }
        public notification notification { get; set; }
    }
    public class context
    {
        public externalSource externalSource { get; set; }
    }
    public class externalSource
    {
        public string referenceId { get; set; }
    }
    public class entities
    {
        public string type { get; set; }
        public string value { get; set; }
    }
    public class paymentLink
    {
        public string purpose { get; set; }
        public DateTime expiresAt { get; set; }
        public bool singleUse { get; set; }
    }
    public class notification
    {
        public bool sendPaymentLink { get; set; }
        public recipients recipients { get; set; }
    }
    public class recipients
    {
        public List<string> to { get; set; }
        public List<string> cc { get; set; }
        public List<string> bcc { get; set; }
    }
}
