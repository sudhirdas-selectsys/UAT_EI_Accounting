using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IEntitySettingsReposiroty
    {
        List<EntitySettings> GetEntitySettings(EntitySettings entitySettings);
    }
}
