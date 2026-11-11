    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class InvoiceExcelMappingRequest
    {
        public string TempFileId { get; set; }
        public byte Direction_Id { get; set; }
        public Dictionary<string, string> ColumnMapping { get; set; }
        public long Excel_Id { get; set; }
    }
}
