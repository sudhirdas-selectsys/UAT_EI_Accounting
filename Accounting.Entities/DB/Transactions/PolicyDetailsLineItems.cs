namespace Accounting.Entities.DB.Transactions
{
    public class PolicyDetailsLineItems : BaseEntity
    {
        public string LineType { get; set; }
        public double HouseCommission { get; set; }
        public double AgentCommission { get; set; }
        public double BrokerCommissionAmount { get; set; }
        public double CommissionAmount { get; set; }
        public string Coverage { get; set; }
        public string? CustomerType { get; set; }
        public string PolicyDetailDescription { get; set; }
        public double FeeTaxAmount { get; set; }
        public double Premium { get; set; }
        public bool FeeTaxable { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string State { get; set; }
        public bool Is_Commissionable { get; set; }
        public double MinPremium { get; set; }
    }
}
