using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_PaymentsUploadColumnPropertyMappings
    {
        public Enum_PaymentsUploadColumnPropertyMappings()
        {
            EntityType = "Entity Type";
            PaymentDirection = "Payment Direction";
            EntityName = "Entity Name";
            PaymentDate = "Payment Date";
            Amount = "Amount";
            PaymentType = "Payment Type";
            ReferenceNo = "Reference #";
            PaymentStatus = "Payment Status";
            BankAccount = "Bank Account";
        }

        public string EntityType { get; set; }
        public string PaymentDirection { get; set; }
        public string EntityName { get; set; }
        public string PaymentDate { get; set; }
        public string Amount { get; set; }
        public string PaymentType { get; set; }
        public string ReferenceNo { get; set; }
        public string PaymentStatus { get; set; }
        public string BankAccount { get; set; }
    }
}
