using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_SQLInputParameters : BaseLookupEntity
    {
        public long SQLParameter_Id { get; set; }
        public int SQLQuery_Id { get; set; }
        public string ParameterName { get; set; }
        public string DisplayName { get; set; }
        public bool Is_OutputParameter { get; set; }
        public byte DataType_Id { get; set; }
        public string MultiInputSeparator { get; set; }
        public byte HTMLControlType_Id { get; set; }
        public short MinTextLength { get; set; }
        public short MaxTextLength { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        public byte NoOfDecimalDigits { get; set; }

        public string DefaultValue { get; set; }

        public bool ShowInputField { get; set; }

        public bool IsRequired { get; set; }
        public byte PredefinedParameter_Id { get; set; }
        public int? DataDictionaryType_Id { get; set; }
        public int Sequence { get; set; }
        public long? Dependency_SQLParameter_Id { get; set; }
        public long? Dependency_SQLParameter_IdForRequired { get; set; }
        public byte? FormatType_Id { get; set; }
        public string FormatCode { get; set; }
        //public int? SQLInputParameterGrouping_Id { get; set; }
        public string GroupName { get; set; }
        public string RangeGroupName { get; set; }
        public bool AllowFilterWith_Null { get; set; }
        public bool AllowFilterWith_Empty { get; set; }
        public bool OutstandingBalances { get; set; }
        //Not Mapped
        public string InputValue { get; set; }
        public LU_DataDictionaryTypes DataDictionayType { get; set; }
        public List<LU_DataDictionary> DataDictionaryList { get; set; }
    }
}
