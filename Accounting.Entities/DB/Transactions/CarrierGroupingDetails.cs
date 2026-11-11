using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class CarrierGroupingDetails :BaseEntity
    {
        public int Carrier_GroupingDetail_Id { get; set; }
        public int LOB_Carrier_Grouping_Id { get; set; }
        public long? Carrier_Id { get; set; }
    }
}
