using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_ServiceProviders : BaseLookupEntity
    {
        public byte ServiceProvider_Id { get; set; }
        public byte ServiceProviderType_ID { get; set; }
    }
}
