using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class ExternalPaymentDetails
    {
        public long ExternalPaymentDetail_Id { get; set; }
        public long Client_Id { get; set; }
        public int Service_Id { get; set; }
        public string ExternalReferenceNo { get; set; } = string.Empty;
        public long? PolicyDetail_Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string PaymentLink { get; set; } = string.Empty;
    }
}
