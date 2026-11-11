using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class ChartOfAccountUsageRequest
    {
        public int ChartOfAccount_Id { get; set; }
        public bool Is_Checked { get; set; }
    }
}
