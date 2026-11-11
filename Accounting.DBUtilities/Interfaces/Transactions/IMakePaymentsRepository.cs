using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IMakePaymentsRepository
    {       
        List<PaymentsEntity> GetInvoicedEntity(long Client_Id, PaymentsEntity paymentEntity, int pageIndex, int pageSize);
        List<MakePayments> GetAgencyInvoices(long Client_Id,MakePayments makepayments);
        EntityDetails GetEntityDetailById(long Client_Id, int EntityType_Id, int Entity_Id,string EntityName);
        List<PaymentsEntity> GetInvoiceLineItemsByIds(long Client_Id, long Invoice_Id);        
        Bill InsertUpdateBill(SqlTransaction sqlTransaction, Bill bill);
        LineItemBill InsertUpdateBillLineItems(SqlTransaction sqlTransaction, LineItemBill lineItemsbill);
    }
}
