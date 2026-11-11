using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Journal
{
    public class CreateJournalRes
    {
        public JournalEntry JournalEntry { get; set; }
        public string time { get; set; }
        public Fault Fault { get; set; }
    }
    public class MetaData
    {
        public string CreateTime { get; set; }
        public string LastUpdatedTime { get; set; }
    }

}
