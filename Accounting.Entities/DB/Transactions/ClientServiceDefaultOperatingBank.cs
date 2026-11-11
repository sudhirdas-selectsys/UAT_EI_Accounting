using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ClientServiceDefaultOperatingBank
    { 
        public long ClientServiceDefaultOperatingBank_Id { get; set; }
        public long Client_Id { get; set; }
        public byte ServiceProviderType_Id { get; set; }
        public byte Service_Id { get; set; }
        public int OperatingBank_Id { get; set; }
    }
}
