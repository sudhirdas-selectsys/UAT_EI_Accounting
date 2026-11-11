using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage.Journal
{
    public class DeleteJournalReq
    {
    }
    

    public class SageFault
    {
        public List<Error> Error { get; set; }
        public string type { get; set; }
    }
}
