using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.EI
{
    public class EIUploadDocumentRequest
    {
        public int? DocumentID { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? DTDocumentID { get; set; }
        public int SubmissionID { get; set; }
        public int? QuoteID { get; set; }
        public int TypeID { get; set; }
        public string TypeDescription { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
        public long? BrokerID { get; set; }
     
    }
}
