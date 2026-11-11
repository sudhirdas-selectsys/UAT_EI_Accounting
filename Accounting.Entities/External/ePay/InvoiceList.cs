namespace Accounting.Entities.External.ePay
{
    public class InvoiceList
    {
        public string PolicyNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CoverageCode { get; set; }
        public string CoverageCodeDescription { get; set; }
        public int FlagFinanced { get; set; }
        public string BillingType { get; set; }
        public double MinimumPremium { get; set; }

        public string CustomerType { get; set; }
        public string ClientPhone { get; set; }

        public string ClientEmail { get; set; }
        public string DBAName { get; set; }
        public string InsuredPhysicalSuite { get; set; }
        public string InsuredPhysicalStreet { get; set; }
        public string InsuredPhysicalCity { get; set; }
        public string InsuredPhysicalState { get; set; }
        public string InsuredPhysicalPostalCode { get; set; }

        public string InsuredMailingSuite { get; set; }
        public string InsuredMailingStreet { get; set; }
        public string InsuredMailingCity { get; set; }
        public string InsuredMailingState { get; set; }
        public string InsuredMailingPostalCode { get; set; }
        public string CarrierName { get; set; }
        public string CarrierSuite { get; set; }
        public string CarrierStreet { get; set; }
        public string CarrierCity { get; set; }
        public string CarrierState { get; set; }
        public string? CarrierNAIC { get; set; }
        public string CarrierPostalCode { get; set; }
        public string MGAContactName { get; set; }
        public string MGAContactPhone { get; set; }
        public string MGAContactStreet { get; set; }
        public string MGAContactCity { get; set; }
        public string MGAContactState { get; set; }
        public string MGAContactPostalCode { get; set; }
        public string RetailAgentName { get; set; }
        public string RetailAgentSuite { get; set; }
        public string RetailAgentStreet { get; set; }
        public string RetailAgentCity { get; set; }
        public string RetailAgentState { get; set; }
        public string RetailAgentPostalCode { get; set; }
        public string InvoiceTypeID { get; set; }
        public double TotalTaxes { get; set; }
        public double TotalFees { get; set; }
        public long InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public long PolicyDetails_Id { get; set; }
        public DateTime DueDate { get; set; }
        public double InvoiceAmount { get; set; }
        public double RemainingBalance { get; set; }
        public string PayerName { get; set; }
        public string AgentEmail { get; set; }
        public string InsuredName { get; set; }
        public string? CarrierReferenceId { get; set; }
        public string External_AccountNumber { get; set; }
        public string External_PostalCode { get; set; }
        public string AgencyName { get; set;  }
        public string InsuredEmail { get; set; }
        public string InsuredPhone { get; set; }
        public string BillType {  get; set; }
        public long? Agency_Id { get; set; }
    }
}
