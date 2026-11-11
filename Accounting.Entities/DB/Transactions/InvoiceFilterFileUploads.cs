using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class InvoiceFilterFileUploads
    {
        public long InvoiceFilterFileUploads_Id { get; set; }
        public byte[] Document { get; set; }
        public string Filename { get; set; }
        public long CreatedBy { get; set; }
    }
}
