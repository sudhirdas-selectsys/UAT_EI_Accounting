using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_LineItems : BaseLookupEntity
    {
        public long LineItem_Id { get; set; }
        public byte LineItemType_Id { get; set; }
        public string Description { get; set; }
    }
}
