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
    public class CarrierGroupingDetailsRepository : ICarrierGroupingDetailsRepository
    {
        public CarrierGroupingDetails InsertCarrierGrouping(SqlTransaction SqlTransaction, CarrierGroupingDetails carriergrouping)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter(){ParameterName="@Carrier_GroupingDetail_Id", Value = carriergrouping.Carrier_GroupingDetail_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@LOB_Carrier_Grouping_Id",carriergrouping.LOB_Carrier_Grouping_Id),
                    new SqlParameter("@Carrier_Id",carriergrouping.Carrier_Id),
                };
                SQLHelper.ExecuteNonQuery(SqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertcarrierGroupingDetails", parameters);
                carriergrouping.Carrier_GroupingDetail_Id = Convert.ToInt32(parameters.Where(c => c.ParameterName == "@Carrier_GroupingDetail_Id").FirstOrDefault().Value);
                return carriergrouping;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
