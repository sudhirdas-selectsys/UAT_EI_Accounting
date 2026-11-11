namespace Accounting.Entities.Internal.Enums
{
    public class Enum_EITransactionTypes
    {
        public Enum_EITransactionTypes()
        {
            NEW = "NEW";
            END = "END";
            REN = "REN";
            CAN = "CAN";
            BinderOnlyRenewal = "Binder Only (Renewal)";
            BinderOnlyNewLine = "Binder Only (New Line)";
            QuickQuote = "QuickQuote";
            REI = "REI";
            NOC = "NOC";
            AUD = "AUD";
        }
        public string NEW { get; set; }
        public string END { get; set; }
        public string REN { get; set; }
        public string CAN { get; set; }
        public string BinderOnlyRenewal { get; set; }
        public string BinderOnlyNewLine { get; set; }
        public string QuickQuote { get; set; }
        public string REI { get; set; }
        public string NOC { get; set; }
        public string AUD { get; set; }
    }
}
