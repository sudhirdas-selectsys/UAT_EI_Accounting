namespace Accounting.Entities.DB.Lookups
{
    public class LU_LineItems_Clients : BaseLookupEntity
    {
        public long LineItems_Clients_Id { get; set; }
        public long LineItem_Id { get; set; }
        public string OverridenLineItemName { get; set; }
        public bool Is_Passthrough { get; set; }
        public bool Is_Excluded { get; set; }
        // not mapped
        public string LineItemName { get; set; }
        public byte  BillingType_Id { get; set; }
        public int LineItems_Directions_Id { get; set; }
        public int BillingType_LineItems_Directions_Id { get; set; }
        public byte Direction_Id { get; set; }
        public string BillingTypeName { get; set; }
        public byte LineItemType_Id { get; set; }
        public string DirectionName { get; set; }
        public string DirectionList { get; set; }
        public byte EntityType_Id { get; set; }
        public string LineItems_Directions_Ids { get; set; }
        public string BillingType_LineItems_Directions_Ids { get; set; }
        public string BillTypeList { get; set; }
        public bool Is_Commissionable { get; set; }
        public string EntityName { get; set; }
        public bool TreatAsPremium { get; set; }
    }
}
