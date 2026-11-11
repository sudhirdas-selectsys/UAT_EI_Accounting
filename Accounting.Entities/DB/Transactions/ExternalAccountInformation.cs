using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ExternalAccountInformation
    {
        public long Client_Id { get; set; }
        public long Id { get; set; }
        public byte EntityType_Id { get; set; }
        public long Entity_Id { get; set; }
        public string AccountNumber { get; set; }
        // not mapped
        public string ZipCode { get; set; }
        public string PaymentURL { get; set; }
    }
}
