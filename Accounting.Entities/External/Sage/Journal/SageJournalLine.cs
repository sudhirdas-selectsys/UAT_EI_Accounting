using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage.Journal
{
    public class SageJournalLine
    {
        public string AccountNo { get; set; }
        public double Amount { get; set; }
        public bool IsDebit { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
    }
}
