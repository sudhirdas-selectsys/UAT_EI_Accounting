namespace Accounting.Entities.Internal.Enums
{
    public class Enum_Settings
    {
        public Enum_Settings()
        {
            InvoiceCarrierLogo = "InvoiceCarrierLogo";
            InvoiceShowCarrierHeader = "InvoiceShowCarrierHeader";
            CarrierSpecificAddress = "CarrierSpecificAddress";
            ACHInfo = "ACHInfo";
        }
        public string InvoiceCarrierLogo { get; set; }
        public string InvoiceShowCarrierHeader { get; set; }
        public string CarrierSpecificAddress { get; set; }
        public string ACHInfo {  get; set; } 
    }
}
