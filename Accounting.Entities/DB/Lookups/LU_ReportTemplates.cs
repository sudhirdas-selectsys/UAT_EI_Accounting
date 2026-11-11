namespace Accounting.Entities.DB.Lookups
{
    public class LU_ReportTemplates : BaseLookupEntity
    {
        public byte ReportTemplate_Id { get; set; }
        public long Report_Id { get; set; }
        public string Template { get; set; }
    }
}
