using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Journal
{
    public class JournalEntryLine
    {
        public string Id { get; set; }              // optional, may be null 
        public string Description { get; set; }
        public double Amount { get; set; }
        public string DetailType { get; set; }
        public JournalEntryLineDetail JournalEntryLineDetail { get; set; }
    }
    public class CreateJournalReq
    {
        public List<JournalEntryLine> Line { get; set; }
        public string DocNumber { get; set; }
        public string TxnDate { get; set; }
        public Fault Fault { get; set; }
    }
}
