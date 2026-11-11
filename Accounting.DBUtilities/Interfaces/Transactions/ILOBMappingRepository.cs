using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILOBMappingRepository
    {
        List<LU_LOBs> GetAllLobsByClient_Id(long Client_Id);
        List<LOBMapping> GetAllLOBMappings(long Client_Id);
        LOBMapping InsertUpdateLOBMapping(LOBMapping lobmapping, long User_Id);
        void DeleteLOBMapping(int LOB_Mapping_Id);
    }
}
