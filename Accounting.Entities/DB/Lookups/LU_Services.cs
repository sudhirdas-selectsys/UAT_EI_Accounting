using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_Services:BaseLookupEntity
    {
        public byte Service_Id { get; set; }
        public byte ServiceProvider_Id { get; set; }
        public byte ServiceProviderType_Id { get; set; }
    }
}
