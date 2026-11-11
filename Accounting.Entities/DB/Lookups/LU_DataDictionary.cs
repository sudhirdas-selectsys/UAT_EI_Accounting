using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public  class LU_DataDictionary : BaseLookupEntity
    {
        public long  DataDictionary_Id { get; set; }

        public int  DataDictionaryType_Id { get; set; }

        public string Value { get; set; }
    }
}
