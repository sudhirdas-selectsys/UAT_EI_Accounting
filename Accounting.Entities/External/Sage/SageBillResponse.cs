using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage
{
    public class SageBillResponse
    {
        public string RecordNo { get; set; }
        public string VendorId { get; set; }
        public string BillNo { get; set; }
        public string TotalDue { get; set; }
    }
}
