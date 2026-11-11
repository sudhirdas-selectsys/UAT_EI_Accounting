namespace Accounting.Entities.DB.Lookups
{
    public class LU_DynamicFilterSQLExpressions
    {
        public long DynamicFilterSQLExpression_Id { get; set; }
        public int DynamicFilter_Id { get; set; }
        public string SQLExpression { get; set; }
        public string GroupChar { get; set; }
        public string ParentGroupChar { get; set; }
        public bool UseORForConcatenate { get; set; }
        public string Dependency_SQLParameters { get; set; }

    }
}
