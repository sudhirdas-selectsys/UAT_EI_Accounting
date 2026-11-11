using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class LOBCarrierGroupingRepository : ILOBCarrierGroupingRepository
    {
        public List<LOBCarrierGrouping> GetAllLOBCarrierGrouping(long Client_Id, byte Service_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", Client_Id),
                    new SqlParameter("@Service_Id", Service_Id)
                };
                return SQLHelper.GetObjectList(new LOBCarrierGrouping(), "USP_Accounting_GetAll_LOB_Carrier_Grouping_Details", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public LOBCarrierGrouping GetLOBCarrierGroupingDetails(byte LOB_Id, long? Carrier_Id, byte Service_Id, long Client_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Service_Id",Service_Id),
                    new SqlParameter("@Client_Id",Client_Id),
                    new SqlParameter("@LOB_Id",LOB_Id),
                    new SqlParameter("@Carrier_Id",Carrier_Id),
                };
                return SQLHelper.GetObject(new LOBCarrierGrouping(), "USP_Accounting_GetLOBCarrierGroupingDetails", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public LOBCarrierGrouping InsertUpdateLOBCarrierGrouping(SqlTransaction sqlTransaction, LOBCarrierGrouping lobcarriergrouping, long User_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter(){ParameterName="@LOB_Carrier_Grouping_Id", Value = lobcarriergrouping.LOB_Carrier_Grouping_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Client_Id",lobcarriergrouping.Client_Id),
                    new SqlParameter("@Service_Id",lobcarriergrouping.Service_Id),
                    new SqlParameter("@Description",lobcarriergrouping.Description),
                    new SqlParameter("@CreatedBy",User_Id),
                };
                SQLHelper.ExecuteNonQuery(sqlTransaction,CommandType.StoredProcedure,"USP_Accounting_InsertUpdateLOBCarrierGrouping", parameters);
                lobcarriergrouping.LOB_Carrier_Grouping_Id = Convert.ToInt32(parameters.Where(c => c.ParameterName == "@LOB_Carrier_Grouping_Id").FirstOrDefault().Value);
                return lobcarriergrouping;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteLOBCarrierGrouping(int LOB_Carrier_Grouping_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                new SqlParameter("@LOB_Carrier_Grouping_Id",LOB_Carrier_Grouping_Id)
                };
                SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteLOBCarrierGrouping", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void InactiveAllLOBCarrierDetails(int LOB_Carrier_Grouping_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                new SqlParameter("@LOB_Carrier_Grouping_Id",LOB_Carrier_Grouping_Id)
                };
                SQLHelper.ExecuteNonQuery("USP_Accounting_InactiveAllLOBCarrierDetails", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetLOBCarrierDescriptionByLOB_Id(long Client_Id, byte LOB_Id, byte Service_Id)
        {
            try
            {
                SqlParameter[] sp =
                {
                    new SqlParameter("@Client_Id",Client_Id),
                    new SqlParameter("@Service_Id",Service_Id),
                    new SqlParameter("@LOB_Id",LOB_Id),
                };
                return SQLHelper.ExecuteScalar("USP_Accounting_GetLOBCarrierDescriptionByLOB_Id", sp)?.ToString() ?? string.Empty;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
