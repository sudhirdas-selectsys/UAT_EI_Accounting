using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_BillingType_LineItems_Directions : BaseLookupEntity
    {
        public int BillingType_LineItems_Directions_Id { get; set; }
        public byte BillingType_Id { get; set; }
        public int LineItems_Directions_Id { get; set; }

        //not mapped
        public string DirectionName { get; set; }
        public string LineItemName { get; set; }
        public string BillingTypeName { get; set; }
        public int? IncomeExpenseLiabilityCOA_Id { get; set; }
        public int? ARAPCOA_Id { get; set; }
        public int LOB_Carrier_Grouping_Id { get; set; }
        public bool Is_Passthrough { get; set; }
        public bool Is_Excluded { get; set; }
        public string PassthroughBillingLineItemDirections { get; set; }
        public byte? AccountingType_Id { get; set; }
        public bool Is_Commissionable { get; set; }
    }
}
