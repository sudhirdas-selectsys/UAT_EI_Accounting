using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class LOBGroupingDetails : BaseEntity
    {
        public int LOB_GroupingDetail_Id { get; set; }
        public int LOB_Carrier_Grouping_Id { get; set; }
        public int? LOB_Id { get; set; }
    }
}
