using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class InstallmentDetails : BaseEntity
    {
        public long Invoice_Id { get; set; }
        public long PolicyDetails_Id { get; set; }
        public string PolicyNumber { get; set; }
        public string TransactionType { get; set; }
        public string EndorsementNumber { get; set; }
        public string InvoiceNo { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }
        public double Premium { get; set; }

    }
    public class Installment
    {
        public DateTime DueDate { get; set; }
        public int InstallmentNo { get; set; }
        public double TotalPremium { get; set; }
        public List<InstallmentDetails> Transactions { get; set; }
    }
}
