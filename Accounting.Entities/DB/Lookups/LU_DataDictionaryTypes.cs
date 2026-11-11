using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public  class LU_DataDictionaryTypes : BaseLookupEntity
    {
        public int DataDictionaryType_Id { get; set; }

        public int  SQLQuery_Id { get; set; }

        public string Display_ParameterName { get; set; }

        public string Value_ParameterName { get; set;}
    }
}
