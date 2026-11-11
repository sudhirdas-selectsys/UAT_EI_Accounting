using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class LineItemBill
    {
        public long Bill_Id { get; set; }
        public long Invoice_Id { get; set; }
        public double AmountApplied { get; set; }
        public long BillLineItems_Id { get; set; }
        public long LineItem_Id { get; set; }
        public double Amount { get; set; }
        public double APAmount { get; set; }
        public double AmountCollected { get; set; }
        public double AmountPaid { get; set; }
        public double NetDue { get; set; }
        public long InvoiceLineItem_Id { get; set; }
    }
}
