using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_ClientLevelSageConfigurations
    {
        public long? ClientLevelSageConfiguration_Id { get; set; }
        public long? Location_Id { get; set; }
        public string? Location {  get; set; }
        public long? Department_Id { get; set; }
        public string? Department { get; set; }
        public long? Class_Id { get; set; }
        public string? Class { get; set; }
    }
}
