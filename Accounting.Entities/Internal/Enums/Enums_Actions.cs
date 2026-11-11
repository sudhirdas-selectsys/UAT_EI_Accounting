using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enums_Actions
    {
        public byte All { get; set; }
        public byte CreateInvoice { get; set; }
        public byte UpdateInvoice { get; set; }
        public byte DeleteInvoice { get; set; }
        public byte CreatePayment { get; set; }
        public byte UpdatePayment { get; set; }
        public byte DeletePayment { get; set; }
        public byte CreatePaymentWriteoff { get; set; }
        public byte UpdatePaymentWriteoff { get; set; }
        public byte DeletePaymentWriteoff { get; set; }
        public byte ApplyPayment { get; set; }
        public byte UnApply { get; set; }
        public byte CreateBill { get; set; }
    }
}
