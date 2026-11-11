using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_EntityTypes : BaseLookupEntity
    {
        public byte EntityType_Id { get; set; }
        public bool IsDisplayedOnUI { get; set; }
    }
}
