using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class SelectedInvoice
    {
        public long Invoice_Id { get; set; }
        public long Bill_Id { get; set; }
        public long BillLineItems_Id { get; set; }
        public long LineItem_Id { get; set; }
        public double AppliedAmount { get; set; }
        public long InvoiceLineItem_Id { get; set; }
        public double Amount { get; set; }
        public double RemainingBalance { get; set; }
    }
}