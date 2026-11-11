using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_LineItems_ChartOfAccounts : BaseLookupEntity
    {
        public int LineItems_ChartOfAccounts_Id { get; set; }
        public long Client_Id { get; set; }
        public int BillingType_LineItems_Directions_Id { get; set; }
        public int ChartOfAccount_Id { get; set; }
        public int LOB_Carrier_Grouping_Id { get; set; }
        public byte ChartOfAccountsClassificationType_Id { get; set; }
        // mapped
        public string DirectionName { get; set; }
        public string LineItemName { get; set; }
        public string BillingTypeName { get; set; }

    }
}
