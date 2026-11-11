using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class ProcessAccountingPaymentRequest
    {
        public long Submission_Id { get; set; }
        public long Quote_Id { get; set; }
        public string PolicyNumber { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPaidAmount { get; set; }
    }
}
