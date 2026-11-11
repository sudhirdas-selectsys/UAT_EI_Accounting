using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class OtherEntityDetails
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Client_Id { get; set; }
    }
}
