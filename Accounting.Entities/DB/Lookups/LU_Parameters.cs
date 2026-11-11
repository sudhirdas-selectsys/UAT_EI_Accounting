using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_Parameters : BaseLookupEntity
    {
        public short Parameter_Id { get; set; }
        public byte DataType_Id { get; set; }
    }

}
