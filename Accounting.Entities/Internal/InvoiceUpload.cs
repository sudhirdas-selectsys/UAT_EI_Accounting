using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class InvoiceUpload
    {
        public string TempFileId { get; set; }
        public List<string> Columns { get; set; }
        public List<string> SystemFields => new()
    {
        "Invoice #",
        "Policy #",
        "Payment Amount"
    };
        public byte Direction_Id { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public long Excel_Id {get; set;}
    }
}
