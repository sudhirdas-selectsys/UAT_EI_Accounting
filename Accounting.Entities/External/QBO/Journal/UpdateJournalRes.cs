using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Journal
{
    public class UpdateJournalRes
    {
        public JournalEntry JournalEntry { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }
    }
}
