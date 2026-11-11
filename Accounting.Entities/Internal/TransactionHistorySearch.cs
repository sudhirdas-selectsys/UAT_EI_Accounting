using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class TransactionHistorySearch : BaseEntity
    {
        public byte? Direction_Id { get; set; }
        public string? EntityName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public byte? PaymentType_Id { get; set; }
        public double? Amount_From { get; set; }
        public double? Amount_To { get; set; }
        public byte? PaymentStatus_Id { get; set; }
        public string? PolicyNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public long? Payment_Id { get; set; }
    }
}
