namespace Accounting.Entities.External.QBO.Journal
{
    public class ReadByIdRes
    {
        public JournalEntry JournalEntry { get; set; }
        public string time { get; set; }
    }
   

    public class CurrencyRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }
}
