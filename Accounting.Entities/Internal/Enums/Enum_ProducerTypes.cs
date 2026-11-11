using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_ProducerTypes
    {
        public Enum_ProducerTypes()
        {
            Retailer = "Retailer";
            Wholesaler = "Wholesaler";
        }
        public string Retailer { get; set; }
        public string Wholesaler { get; set; }
    }
}
