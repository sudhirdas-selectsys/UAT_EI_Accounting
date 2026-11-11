using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ReferralUnderwriter : BaseEntity
    {
        public long ReferralUnderwriter_Id { get; set; }
        public string Name { get; set; }
    }
}
