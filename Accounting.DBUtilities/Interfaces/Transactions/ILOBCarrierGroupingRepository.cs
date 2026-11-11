using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILOBCarrierGroupingRepository
    {
        List<LOBCarrierGrouping> GetAllLOBCarrierGrouping(long Client_Id, byte Service_Id);
        LOBCarrierGrouping GetLOBCarrierGroupingDetails(byte LOB_Id,long? Carrier_Id, byte Service_Id, long Client_Id);
        LOBCarrierGrouping InsertUpdateLOBCarrierGrouping(SqlTransaction sqlTransaction, LOBCarrierGrouping lobcarriergrouping,long User_Id);
        void DeleteLOBCarrierGrouping(int LOB_Carrier_Grouping_Id);
        void InactiveAllLOBCarrierDetails(int LOB_Carrier_Grouping_Id);
        string GetLOBCarrierDescriptionByLOB_Id(long Client_Id, byte LOB_Id, byte Service_Id);
    }
}
