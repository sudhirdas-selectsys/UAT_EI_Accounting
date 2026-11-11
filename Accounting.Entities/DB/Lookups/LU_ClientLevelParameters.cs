using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public  class LU_ClientLevelParameters : BaseLookupEntity
    {
        public long Client_Id { get; set; }
        public byte Service_Id { get; set; }
        public short Parameter_Id { get; set; }
        public byte DataType_Id { get; set; }
        public byte HTMLControlType_Id { get; set; }
        public short ValidationType_Id { get; set; }
        public byte NoOfDecimalDigits { get; set; }
        public int DataDictionaryType_Id { get; set; }
        public string Value { get; set; }
    }
}
