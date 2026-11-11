using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class SubmissionDocuments:BaseEntity
    {
        public int? DocumentID { get; set; }
        public long? SubmissionDocument_Id { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? DTDocumentID { get; set; }        
        public int? QuoteID { get; set; }
        public int? TypeID { get; set; }
        public string TypeDescription { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string? LOB { get; set; }
        public string? CompanyName { get; set; }
        
        /// <summary>
        /// Document Content in BASE64
        /// </summary>
        public string FileData { get; set; }
        public string? FileDataBase64 { get; set; }
        public long? BrokerID { get; set; }
        public SubmissionDocuments()
        {

        }
    }
}
