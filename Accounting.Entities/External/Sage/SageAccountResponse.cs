using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage
{
    public class SageAccountsResponse
    {
        public string AccountNo { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
    }
}
