using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_PredefinedParameters :BaseLookupEntity
    {
        public byte PredefinedParameter_Id { get; set; }
        public byte Sequence { get; set; }
    }
}
