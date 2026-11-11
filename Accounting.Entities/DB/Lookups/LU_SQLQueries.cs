namespace Accounting.Entities.DB.Lookups
{
    public  class LU_SQLQueries : BaseLookupEntity
    {
        public int SQLQuery_Id { get; set; }
        public string SQLQuery { get; set; } 
        public bool Is_StoredProcedure { get; set; }
        public string Description { get; set; }
    }
}
