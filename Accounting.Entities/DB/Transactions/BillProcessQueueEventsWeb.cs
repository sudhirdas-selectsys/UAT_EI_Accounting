using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class BillProcessQueueEventsWeb : BaseEntity
    {
        public DateTime TransactionDate { get; set; }
        public string WorkFlowEvent { get; set; } = string.Empty;
        public string WorkFlowStatus { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; }
        public string ServiceResponseMessage { get; set; } = string.Empty;
    }
}
