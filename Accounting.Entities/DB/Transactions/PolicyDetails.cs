namespace Accounting.Entities.DB.Transactions
{
    public class PolicyDetails : BaseEntity
    {
        public PolicyDetails()
        {
            PolicyDetailsLineItemsList = new List<PolicyDetailsLineItems>();
            InvoicesList = new List<Invoices>();
            InvoicesDetailsList = new List<InvoiceDetails>();
            Installments = new List<Installment>();
        }
        public string PolicyNumber { get; set; }
        public string PolicyInstallmentCode { get; set; }
        public string Corporation { get; set; }
        public string Location { get; set; }
        public string RiskState { get; set; }
        public string PaymentPlan { get; set; }
        public string? BillType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CancelDate { get; set; }
        public DateTime? EndorsementEffectiveDate { get; set; }
        public string PolicyAction { get; set; }
        public string LOBOverride { get; set; }
        public string PolicyType { get; set; }
        public string TransactionType { get; set; }
        public string PolicyDescription { get; set; }
        public string InvoiceNote { get; set; }
        public string MasterPolicy { get; set; }
        public string LoanNum { get; set; }
        public bool PostAsShell { get; set; }
        public int? EndorsementNumber { get; set; }
        public string? CustomerType { get; set; }
        public long? WholesalerDetail_Id { get; set; }
        public long UnderWriterDetail_Id { get; set; }
        public long? InterestDetail_Id { get; set; }
        public long? MortgageeDetail_Id { get; set; }
        public long InsuredDetail_Id { get; set; }
        public long? FinanceDetail_Id { get; set; }
        public long CarrierDetail_Id { get; set; }
        public long AgentDetail_Id { get; set; }
        public byte Source_Id { get; set; }
        public string AgencyName { get; set; }
        public AgentDetails AgentDetails { get; set; }
        public CarrierDetails CarrierDetails { get; set; }
        public WholesalerDetails WholesalerDetails { get; set; }
        public FinanceDetails FinanceDetails { get; set; }
        public InsuredDetails InsuredDetails { get; set; }
        public MortgageeDetails MortgageeDetails { get; set; }
        public InterestDetails InterestDetails { get; set; }
        public UnderWriterDetails UnderWriterDetails { get; set; }
        public SalesRepDetails SalesRepDetails { get; set; }
        public List<PolicyDetailsLineItems> PolicyDetailsLineItemsList { get; set; }
        public Clients Clients { get; set; }
        public Invoices Invoices { get; set; }
        public List<Invoices> InvoicesList { get; set; }
        public List<InvoiceDetails> InvoicesDetailsList { get; set; }
        public string InsuredName { get; set; }
        public byte AccountingType_Id { get; set; }
        public ExternalAccountInformation ExternalAccountInformation { get; set; }
        public long OriginalSubmission_Id { get; set; }
        public long PreviousSubmission_Id { get; set; }
        public long? ReferralUnderwriter_Id { get; set; }
        public string BankName { get; set; }
        public string AccountTypeName { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public long? AgentBrokerContactDetail_Id { get; set; }
        public AgentBrokerContactDetails AgentBrokerContactDetails { get; set; }
        public string ExternalPaymentLink { get; set; }
        public byte LOB_Id { get; set; }
		public List<Installment> Installments { get; set; }
        public double TotalAppliedAmount { get; set; }
    }
}
