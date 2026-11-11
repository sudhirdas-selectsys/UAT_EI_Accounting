using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.ChecKeeper
{
    
    public class CreateCheckRes
    {      
        public string message { get; set; }
        public Dictionary<string, List<string>> errors { get; set; }
        public CheckData data { get; set; }
    }

    public class CheckData
    {
        public string pdf { get; set; }
    }
    public class CreateCheckErrorRes
    {
        public string message { get; set; }

        public Dictionary<string, List<string>> errors { get; set; }
    }
}
