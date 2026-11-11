using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ApplicationSettings : BaseEntity
    {
        public long ApplicationSetting_Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }
}
