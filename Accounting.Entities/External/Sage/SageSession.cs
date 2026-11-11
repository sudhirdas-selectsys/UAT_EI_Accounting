using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage
{
    public class SageSession
    {
        public string SessionId { get; set; }
        public string Endpoint { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime SessionTimestamp { get; set; }
    }
}
