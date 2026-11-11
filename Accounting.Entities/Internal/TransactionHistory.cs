using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class TransactionHistory
    {
        public long PaymentApplicationLineItem_Id { get; set; }
        public long Payment_Id{get;set;}
        public string PolicyNumber { get; set; }
        public string InvoiceNo { get; set; }
        public long InvoiceDetails_Id { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string EntityName { get; set; }
        public long Entity_Id { get; set; }
        public string AgentName { get; set; }
        public long AgentDetail_Id { get; set; }
        public string TransactionType { get; set; }
        public byte Direction_Id { get; set; }
        public byte PaymentDirection_Id { get; set; }
        public string PaymentMethod { get; set; }
        public byte PaymentType_Id { get; set; }
        public string ReferenceNumber { get; set; }
        public double AppliedAmount { get; set; }
        public string BankAccount { get; set; }
        public int OperatingBank_Id { get; set; }
        public string PaymentStatus { get; set; }
        public byte PaymentStatus_Id { get; set; }
    }
}
