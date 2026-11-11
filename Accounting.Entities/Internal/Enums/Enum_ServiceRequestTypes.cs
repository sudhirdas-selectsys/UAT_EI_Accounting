namespace Accounting.Entities.Internal.Enums
{
    public class Enum_ServiceRequestTypes
    {
        public byte GetAccessToken { get; set; }
        public byte GetAllChartOfAccounts { get; set; }
        public byte CreateJournalEntry { get; set; }
        public byte UpdateJournalEntry { get; set; }
        public byte DeleteJournalEntry { get; set; }
        public byte ReadJournalEntryById { get; set; }
        public byte UploadCommissionsStatements { get; set; }
        public byte CreateVendor {get;set;}
        public byte UpdateVendor {get;set;}
        public byte CreateBill {get;set;}
        public byte ReadVendorById {get;set;}
        public byte DeleteBill { get; set; }
        public byte ReadBillById { get; set; }
        public byte RunJob { get; set; }
        public byte PrintCheck { get; set; }
        public byte GeneratePaymentLink { get; set; }
        public byte GetAgentEmailData { get; set; }
        public byte PDFCoverterAPI { get; set; }
    }
}
