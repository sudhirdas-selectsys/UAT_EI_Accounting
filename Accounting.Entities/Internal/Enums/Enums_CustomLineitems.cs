namespace Accounting.Entities.Internal.Enums
{
    public class Enums_CustomLineitems
    {
        public Enums_CustomLineitems()
        {
            HouseCommission = "House Commission";
            TotalCommissionPercentage = "Total Commission Percentage";
            TotalPremium = "Total Premium";
            MGACommission = "MGA Commission";
            CarrierPremium = "Carrier Premium";
            UnderwriterCommission = "Underwriter Commission";
            AgencyBrokerCommission = "Agency/Broker Commission";
            ReferralUnderwriterCommission = "ReferralUnderwriter Commission";
            FullyEarnedPremium = "Fully Earned Premium";
            TotalCommission = "Total Commission";
        }
        public string HouseCommission { get; set; }
        public string TotalCommissionPercentage { get; set; }
        public string TotalPremium { get; set; }
        public string MGACommission { get; set; }
        public string CarrierPremium { get; set; }
        public string UnderwriterCommission { get; set; }
        public string AgencyBrokerCommission { get; set; }
        public string ReferralUnderwriterCommission { get; set; }
        public string FullyEarnedPremium { get; set; }
        public string TotalCommission { get; set; }
    }
}
