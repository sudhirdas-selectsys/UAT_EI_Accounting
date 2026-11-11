using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class ChartOfAccountDetails
    {
        public int? StandardCOA_Id { get; set; }
        public long PaymentDirection_Id { get; set; }
        public string StandardCOAName { get; set; }
        public int? WriteoffCOA_Id { get; set; }
        public string WriteoffCOAName { get; set; }
    }
}
