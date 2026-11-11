using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ClientSettingsRepository : IClientSettingsRepository
    {
        public List<ClientSettings> GetClientSettings(long Client_Id, string SettingKey)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@SettingKey", SettingKey)
            };
            return SQLHelper.GetObjectList(new ClientSettings(), "USP_Accounting_GetClientSettings", sp);
        }
        public List<ApplicationSettings> GetApplicationSettings(string settingKey)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@SettingKey", settingKey)
            };
            return SQLHelper.GetObjectList(new ApplicationSettings(), "USP_Accounting_GetApplicationSettings", sp);
        }
    }
}
