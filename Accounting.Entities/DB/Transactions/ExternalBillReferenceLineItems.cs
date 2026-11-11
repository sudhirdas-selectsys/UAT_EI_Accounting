using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ExternalBillReferenceLineItems
    { 
        public long Invoice_Id { get; set; }
        public double InvoicedAmount { get; set; }
        public double InvoiceRemainingAmount { get; set; }
        public double TotalAppliedAmount { get; set; }
        public bool IsBillFullyApplied { get; set; }
        
        public long LineItem_Id { get; set; }
        public byte Direction_Id { get; set; }
       
    }
}
