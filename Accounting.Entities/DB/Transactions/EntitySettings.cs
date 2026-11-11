using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class EntitySettings : BaseEntity
    {
        public byte EntityType_Id { get; set; }
        public long Entity_Id { get; set; }
        public long Setting_Id { get; set; }
        public string? SettingValue_Text { get; set; }
        public byte[]? SettingValue_Binary { get; set; }
        public string? SettingName {  get; set; }
        public string? SettingCode { get; set; }
        public string? SettingDescription { get; set; }
    }
}
