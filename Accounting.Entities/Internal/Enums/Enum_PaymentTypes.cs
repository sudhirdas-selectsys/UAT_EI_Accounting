namespace Accounting.Entities.Internal.Enums
{
    public class Enum_PaymentTypes
    {
        public byte ACH { get; set; }
        public byte CreditCard { get; set; }
        public byte ElectronicPayment { get; set; }
        public byte Cash { get; set; }
        public byte WireTransfer { get; set; }
        public byte Check { get; set; }
        public byte Bill { get; set; }
    }
}
