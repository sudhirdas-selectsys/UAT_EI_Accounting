using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_BillProcessWorkFlowStatus
    {
        public Enum_BillProcessWorkFlowStatus()
        {
            Cancelled = 7;
            Deleted = 8;
        }
        public byte Cancelled { get; set; }
        public byte Deleted { get; set; }
    }
}
