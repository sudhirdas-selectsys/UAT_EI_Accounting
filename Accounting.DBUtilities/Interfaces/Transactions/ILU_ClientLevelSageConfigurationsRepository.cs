using Accounting.Entities.DB.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILU_ClientLevelSageConfigurationsRepository
    {
        LU_ClientLevelSageConfigurations GetClientLevelSageConfigurations(long client_Id, byte Action_Id);
    }
}
