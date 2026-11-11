namespace Accounting.Entities.DB.Transactions
{
    public class AgentDetails : BaseEntity
    {
        public long AgentDetail_Id { get; set; }
        public string AgentReference_Id { get; set; }
        public string Email { get; set; }  
        public string FEIN {  get; set; }
        public string EI_Accounting_Id { get; set; }
        public string? AgentAccountNumber { get; set; }
        public string? AgentRoutingNumber { get; set; }
        public string? SalesRep { get; set; }
        public long AssignedSalesRep_Id { get; set; }
        public string ProducerType { get; set; }
        public long? ParentAgencyDetails_Id { get; set; }
        public string BankName { get; set; }
        public string AccountTypeName { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string DBA { get; set; }
        public DateTime? AgreementDate {  get; set; }
        public string CompanyType {  get; set; }
        public string AgentStatus { get; set; }
        public string Phone { get; set; }
    }
}
