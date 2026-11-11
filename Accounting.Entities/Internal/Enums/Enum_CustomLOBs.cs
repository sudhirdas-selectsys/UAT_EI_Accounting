using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_CustomLOBs
    {
        public Enum_CustomLOBs()
        {
            Common = "Common";
            Package = "Package";
        }
        public string Common { get; set; }
        public string Package { get; set; }
    }
}
