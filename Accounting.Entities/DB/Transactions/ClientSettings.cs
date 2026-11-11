namespace Accounting.Entities.DB.Transactions
{
    public class ClientSettings : BaseEntity
    {
        public long ClientSetting_Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }
}
