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
    public class LineItemTypesRepository : ILineItemTypesRepository
    {
        public LineItemTypes GetAllInvoicesBasedOnLineItemId(long Invoice_Id, int @LineItemType_Id)
        {
            SqlParameter[] spParams =
                       {
                    new SqlParameter("@Invoice_Id",Invoice_Id),
                     new SqlParameter("@LineItemType_Id", @LineItemType_Id ),
                };
            return SQLHelper.GetObject(new LineItemTypes(), "USP_Accounting_GetAllInvoicesBasedOnLineItemId", spParams);
        }


        


    }
}
