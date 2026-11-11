namespace Accounting.Entities.DB.Transaction
{
    public  class ReportOutputTableParameters : BaseEntity
    {
        public int ReportOutputTable_Id { get; set; }

        public string GroupName { get; set; }
        public string ParameterName { get; set; }

        public string DisplayName { get; set; }

        public bool Enable_Sorting { get; set; }
        public int? DisplaySequence { get; set; }
        public bool Hide { get; set; }
        public string GroupConcatenate { get; set; }
        public byte? FormatType_Id { get; set; }
        public string FormatCode { get; set; }
        public byte? OutPutDataType_Id { get; set; }
        public bool Enable_Total { get; set; }
        public byte? TotalFormatType_Id { get; set; }
        public string TotalFormatCode { get; set; }
        public string Excel_FormatCode { get; set; }

        //Not Mapped
        //public int Is_DuplicateParameter { get; set; }
    }
}
