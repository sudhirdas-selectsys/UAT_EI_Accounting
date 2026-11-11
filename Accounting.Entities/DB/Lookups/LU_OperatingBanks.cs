namespace Accounting.Entities.DB.Lookups
{
    public class LU_OperatingBanks : BaseLookupEntity
    {
        public int OperatingBank_Id { get; set; }        
        public int OperatingBankCOA_Id { get; set; }
        public bool DefaultPaymentOperatingBank { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string SignerValue { get; set; }
        public string SignerType { get; set; }
        public string Check_CompanyName { get; set; }
        public string Check_BankName {get; set; }
        public string Check_BankAddress { get; set; }
        public string cityName { get; set; }
        public string zipCode { get; set; }
    }
}
