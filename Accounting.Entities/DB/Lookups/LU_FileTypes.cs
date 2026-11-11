namespace Accounting.Entities.DB.Lookups
{
    public  class LU_FileTypes : BaseLookupEntity
    {
        public byte FileType_Id { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public string SubType { get; set; }
    }
}
