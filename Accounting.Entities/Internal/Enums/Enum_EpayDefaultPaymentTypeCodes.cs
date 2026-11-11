using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_EpayDefaultPaymentTypeCodes
    {
        public Enum_EpayDefaultPaymentTypeCodes()
        {
            PaymentTypes = new Dictionary<string, byte>()
            {
                { "19", 14 },
                { "11", 13 }
            };
        }
        public Dictionary<string, byte> PaymentTypes { get; set; }
    }
}
