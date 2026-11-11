namespace Accounting.Entities.Internal
{
    public class Search
    {
        public long Client_Id { get; set; }
        public string Name { get; set; }
        public string TabName { get; set; }
        public List<string> EntityTypes { get; set; }
        public string EntityTypesNames { get; set; }
        public byte? PaymentType_Id { get; set; }
        public string ReferenceNumber { get; set; }
        public byte? PaymentStatus_Id { get; set; }
        public byte? OperatingBank_Id { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double? Amount { get; set; }
        public byte? EntityType_Id { get; set; }
        public byte Direction_Id { get; set; }
        public byte? LineItem_Id { get; set; }
        public long? Entity_Id { get; set; }
        public string PolicyNumber { get; set; }
        public string AgencyName { get; set; }
        public string InsuredName { get; set; }
        public bool OutstandingBalances { get; set; }
        public string InvoiceNo { get; set; }
        public string EntityName { get; set; }
        public string LineItemList { get; set; }
        public byte? BillingType_Id { get; set; }
    }
}
