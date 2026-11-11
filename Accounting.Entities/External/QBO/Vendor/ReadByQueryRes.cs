using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Vendor
{
    public class ReadByQueryRes
    {
        public QueryResponse QueryResponse { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }
    }

    public class QueryResponse
    {
        public List<Vendor> Vendor { get; set; }
    }
}
