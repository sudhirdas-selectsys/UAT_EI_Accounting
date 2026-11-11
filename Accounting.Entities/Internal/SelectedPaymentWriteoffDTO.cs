using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class SelectedPaymentWriteoffDTO
    {
        public long Payment_Id { get; set; }
        public double Amount { get; set; }
        public string Note { get; set; }
    }
}
