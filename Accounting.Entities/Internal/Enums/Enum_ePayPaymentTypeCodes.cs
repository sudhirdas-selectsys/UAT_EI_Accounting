
namespace Accounting.Entities.Internal.Enums
{
    public class Enum_ePayPaymentTypeCodes
    {
        public Enum_ePayPaymentTypeCodes()
        {
            PaymentTypes = new Dictionary<string, byte>()
            {
                { "13", 2 },
                { "12", 2 },
                { "19", 2 },
                { "17", 1 },
                { "11", 11 }
            };
        }
        public Dictionary<string, byte> PaymentTypes { get; set; }
    }
}
