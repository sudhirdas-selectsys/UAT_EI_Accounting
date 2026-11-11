using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage.Journal
{
    public class SageJournalResponse
    {
        public string RecordNo { get; set; }
        public string Journal { get; set; }
        public string BatchDate { get; set; }
        public string ReferenceNo { get; set; }
        public List<SageJournalLine> Entries { get; set; }
    }

}
