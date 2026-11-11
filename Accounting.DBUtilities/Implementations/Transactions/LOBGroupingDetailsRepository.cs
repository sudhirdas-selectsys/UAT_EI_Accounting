using Accounting.DBUtilities.Interfaces.Transactions;
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
    public class LOBGroupingDetailsRepository : ILOBGroupingDetailsRepository
    {
        public LOBGroupingDetails InsertLOBGrouping(SqlTransaction sqlTransaction, LOBGroupingDetails lobgrouping)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter(){ParameterName="@LOB_GroupingDetail_Id", Value = lobgrouping.LOB_GroupingDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@LOB_Carrier_Grouping_Id",lobgrouping.LOB_Carrier_Grouping_Id),
                    new SqlParameter("@LOB_Id",lobgrouping.LOB_Id),
                };
                SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertLOBGroupingDetails", parameters);
                lobgrouping.LOB_GroupingDetail_Id = Convert.ToInt32(parameters.Where(c => c.ParameterName == "@LOB_GroupingDetail_Id").FirstOrDefault().Value);
                return lobgrouping;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
