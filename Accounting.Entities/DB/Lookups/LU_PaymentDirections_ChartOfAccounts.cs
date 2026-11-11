using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_PaymentDirections_ChartOfAccounts: BaseLookupEntity
    {
        public long PaymentDirections_ChartOfAccount_Id { get; set; }
        public int PaymentDirection_Id { get; set; }
        public int ChartOfAccounts_Id { get; set; }
        public byte PaymentDirectionsSubType_Id { get; set; }
        //not mapped
        public string Direction { get; set; }
        public int? StandardCOA_Id { get; set; }
        public int? WriteoffCOA_Id { get; set; }
    }
}
