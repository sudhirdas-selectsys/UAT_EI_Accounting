using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class LOBMapping : BaseEntity
    {
        public int LOB_Mapping_Id { get; set; }
        public string Code { get; set; }
        public byte LOB_Id { get; set; }
        public List<int> LOB_Ids { get; set; }
    }
}
