using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class BillProcessQueueWeb : BaseEntity
    {
        public long BillProcess_Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BillProcessStatus { get; set; } = string.Empty;
        public string CurrentWorkFlowEvent { get; set; } = string.Empty;
    }
}
