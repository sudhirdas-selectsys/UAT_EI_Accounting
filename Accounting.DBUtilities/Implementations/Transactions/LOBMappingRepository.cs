using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class LOBMappingRepository : ILOBMappingRepository
    {
        public List<LU_LOBs> GetAllLobsByClient_Id(long Client_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", Client_Id)
                };
                return SQLHelper.GetObjectList(new LU_LOBs(), "USP_Accounting_GetAllClientLOBs", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<LOBMapping> GetAllLOBMappings(long Client_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", Client_Id)
                };
                return SQLHelper.GetObjectList(new LOBMapping(), "USP_Accounting_GetAll_LOB_Mapping_Details", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public LOBMapping InsertUpdateLOBMapping(LOBMapping lobmapping, long User_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter(){ParameterName="@LOB_Mapping_Id", Value = lobmapping.LOB_Mapping_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Client_Id",lobmapping.Client_Id),
                    new SqlParameter("@LOB_Id",lobmapping.LOB_Id),
                    new SqlParameter("@CreatedBy",User_Id),
                };
                SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_InsertUpdateLOBMapping", parameters);
                lobmapping.LOB_Mapping_Id = Convert.ToInt32(parameters.Where(c => c.ParameterName == "@LOB_Mapping_Id").FirstOrDefault().Value);
                return lobmapping;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteLOBMapping(int LOB_Mapping_Id)
        {
            try
            {
                SqlParameter[] parameters =
                {
                new SqlParameter("@LOB_Mapping_Id",LOB_Mapping_Id)
                };
                SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteLOBMapping", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
