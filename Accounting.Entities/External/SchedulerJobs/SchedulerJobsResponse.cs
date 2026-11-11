using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.SchedulerJobs
{
    public class SchedulerJobsResponse
    {
        public int Status { get; set; }
        public string StatusDescription { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
    }
}
