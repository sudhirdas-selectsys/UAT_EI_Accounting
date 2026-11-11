using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class Bill:BaseEntity
    {
        public long Bill_Id { get; set; }
        public byte EntityType_Id {  get; set; }
        public long Entity_Id { get; set; }
        public double Due { get; set; }
        public double AmountApplied {  get; set; }
        public long BillLineItems_Id { get; set; }
        public long Invoice_Id { get; set; }
        public long LineItem_Id { get; set; }
        public double Amount { get; set; }
        public double APAmount { get; set; }
        public double AmountCollected { get; set; }
        public double AmountPaid { get; set; }
        public double NetDue { get; set; }
        public bool PostToAgentPortal { get; set; }
        public DateTime PaymentToDate { get; set; }
        public string EntityName { get; set; }
        public string? MemoDescription { get; set; }
        public byte PaymentType { get; set; }
        public int Operating_Bank_ID { get; set; }
        public string PayeeNameOnCheck { get; set; }
    }
}
