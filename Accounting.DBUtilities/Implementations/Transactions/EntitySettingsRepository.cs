using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class EntitySettingsRepository : IEntitySettingsReposiroty
    {
        public List<EntitySettings> GetEntitySettings(EntitySettings entitySettings)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",entitySettings.Client_Id),
                new SqlParameter("@EntityType_Id",entitySettings.EntityType_Id),
                new SqlParameter("@Entity_Id",entitySettings.Entity_Id),
            };
            return SQLHelper.GetObjectList(new EntitySettings(), "USP_Accounting_GetEntitySettings", sp);
        }
    }
}
