namespace Accounting.Entities.Internal.Enums
{
    public class Enum_AccountingCodes
    {
        public Enum_AccountingCodes()
        {
            Agent = "AG";
            Carrier = "CA";
            Insured = "IS";
            Wholesaler = "WS";
            Mortgagee = "MG";
        }
        public string Agent {  get; set; }
        public string Carrier { get; set; }
        public string Insured { get; set; }
        public string Wholesaler { get; set; }
        public string Mortgagee { get; set; }
    }
}
