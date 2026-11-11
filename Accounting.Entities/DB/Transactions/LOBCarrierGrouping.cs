using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class LOBCarrierGrouping : BaseEntity
    {
        public int LOB_Carrier_Grouping_Id { get; set; }
        public byte Service_Id { get; set; }
        public string Description { get; set; }
        public long CreatedBy { get; set; }
        public string LOBs { get; set; }
        public string Carriers { get; set; }
        public List<int> LOB_Ids { get; set; }
        public List<int> Carrier_Ids { get; set; }
    }
}
