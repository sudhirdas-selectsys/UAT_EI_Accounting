using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_LineItems_Directions : BaseLookupEntity
    {
        public int LineItems_Directions_Id { get; set; }
        public long LineItem_Id { get; set; }
        public byte Direction_Id { get; set; }
        public long LineItems_Clients_Id { get; set; }
        public byte? EntityType_Id { get; set; }
    }
}
