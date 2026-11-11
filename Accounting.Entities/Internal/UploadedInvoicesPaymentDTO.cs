using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class UploadedInvoicesPaymentDTO
    {
        public string InvoiceNumber { get; set; }
        public string PolicyNumber { get; set; }
        public string PaymentAmount { get; set; }
        public string TempFileId { get; set; }
        public byte Direction_Id { get; set; }
    }
}
