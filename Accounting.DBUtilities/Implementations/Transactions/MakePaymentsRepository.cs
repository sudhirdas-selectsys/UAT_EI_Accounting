using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System.Data;
using static Azure.Core.HttpHeader;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class MakePaymentsRepository : IMakePaymentsRepository
    {
        public EntityDetails GetEntityDetailById(long Client_Id, int EntityType_Id, int Entity_Id, string EntityName)
        {
            try
            {
                SqlParameter[] sqlParameters =
            {
               new SqlParameter("@EntityType_Id", EntityType_Id),
               new SqlParameter("@Entity_Id", Entity_Id),
               new SqlParameter("@Client_Id", Client_Id),
               new SqlParameter("@Entity_Name",EntityName)
            };
                return SQLHelper.GetObject(new EntityDetails(), "USP_Accounting_GetEntityDetails", sqlParameters);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<PaymentsEntity> GetInvoiceLineItemsByIds(long Client_Id, long Invoice_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@Client_Id", Client_Id),
                        new SqlParameter("@Invoice_Id", Invoice_Id)
                      };
            return SQLHelper.GetObjectList(new PaymentsEntity(), "USP_Accounting_GetLineItems_ByInvoice_Id", spParams);
        }
        public List<PaymentsEntity> GetInvoicedEntity(long Client_Id, PaymentsEntity paymentEntity, int pageIndex, int pageSize)
        {
            SqlParameter[] sqlParameters =
            {
            new SqlParameter("@Client_Id",Client_Id),
            new SqlParameter("@InvoiceDate", paymentEntity.InvoiceDate),
            new SqlParameter("@InvoiceCreatedFromDate", paymentEntity.InvoiceCreatedFromDate),
            new SqlParameter("@InvoiceCreatedToDate", paymentEntity.InvoiceCreatedToDate),
            new SqlParameter("@InvoiceEffectiveFromDate", paymentEntity.InvoiceEffectiveFromDate),
            new SqlParameter("@InvoiceEffectiveToDate", paymentEntity.InvoiceEffectiveToDate),
            new SqlParameter("@EntityType_Id",paymentEntity.EntityType_Id),
            new SqlParameter("@Entity_Id",paymentEntity.Entity_Id),
            new SqlParameter("@AmountDueBasedOnType_Id",paymentEntity.AmountDueBasedOnType_Id),
            new SqlParameter("@AgencyName",paymentEntity.AgencyName),
            new SqlParameter("@LineItem_Id", (object)paymentEntity.LineItem_Id ?? DBNull.Value),
            new SqlParameter("@LineItemIds", paymentEntity.LineItemList),
            new SqlParameter("@PolicyNumber", paymentEntity.PolicyNumber),
            new SqlParameter("@InsuredName", paymentEntity.InsuredName),
            new SqlParameter("@As_Of_Date", paymentEntity.PaymentBookedDate),
            new SqlParameter("@InvoiceNo", paymentEntity.InvoiceNo),
            new SqlParameter("@EntityName",paymentEntity.EntityName),
            new SqlParameter("@Direction_Id",paymentEntity.Direction_Id),
            new SqlParameter("@OutstandingBalances",paymentEntity.OutstandingBalances),
            new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize),
            };
            return SQLHelper.GetObjectList(new PaymentsEntity(), "USP_Accounting_GetMakePaymentsEntities_V6", sqlParameters);
        }
        public List<MakePayments> GetAgencyInvoices(long Client_Id, MakePayments makepayments)
        {
            SqlParameter[] sqlParameters =
            {
            new SqlParameter("@Client_Id",Client_Id),
            new SqlParameter("@InvoiceDate", makepayments.InvoiceDate),
            new SqlParameter("@InvoiceCreatedFromDate", makepayments.InvoiceCreatedFromDate),
            new SqlParameter("@InvoiceCreatedToDate", makepayments.InvoiceCreatedToDate),
            new SqlParameter("@InvoiceEffectiveFromDate", makepayments.InvoiceEffectiveFromDate),
            new SqlParameter("@InvoiceEffectiveToDate", makepayments.InvoiceEffectiveToDate),
            new SqlParameter("@AgentDetail_Id",makepayments.AgentDetail_Id),
            new SqlParameter("@Payee_Id",makepayments.Payee_Id),
            new SqlParameter("@PayeeType_Id",makepayments.PayeeType_Id),
            new SqlParameter("@EntityType_Id",makepayments.EntityType_Id),
            new SqlParameter("@Entity_Id",makepayments.Entity_Id),
            new SqlParameter("@EntityName",makepayments.EntityName),
            new SqlParameter("@AmountDueBasedOnType_Id",makepayments.AmountDueBasedOnType_Id),
            new SqlParameter("@AgencyName",makepayments.AgencyName),
            new SqlParameter("@LineItem_Id", makepayments.LineItem_Id),
            new SqlParameter("@LineItemIds", makepayments.LineItemList),
            new SqlParameter("@PolicyNumber", makepayments.PolicyNumber),
            new SqlParameter("@InsuredName", makepayments.InsuredName),
            new SqlParameter("@As_Of_Date", makepayments.PaymentBookedDate),
            new SqlParameter("@InvoiceNo",makepayments.InvoiceNo),
            new SqlParameter("@OutstandingBalances",makepayments.OutstandingBalances),
             new SqlParameter("@Direction_Id",makepayments.Direction_Id)
            };
            return SQLHelper.GetObjectList(new MakePayments(), "USP_Accounting_GetMakePaymentsInvoices_V3", sqlParameters);
        }

        public Bill InsertUpdateBill(SqlTransaction sqlTransaction, Bill bill)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter (){ParameterName="@Bill_Id",Value=bill.Bill_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@EntityType_Id",bill.EntityType_Id),
                new SqlParameter("@Entity_Id",bill.Entity_Id),
                new SqlParameter("@Due",Math.Round(bill.Due, 2)),
                new SqlParameter("@Amount_Applied",Math.Round(bill.AmountApplied, 2)),
                new SqlParameter("@Entityname",bill.EntityName)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateBill", sqlparams);
            bill.Bill_Id = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@Bill_Id").FirstOrDefault()?.Value);
            return bill;
        }
        public LineItemBill InsertUpdateBillLineItems(SqlTransaction sqlTransaction, LineItemBill lineItemBills)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter (){ParameterName="@BillLineItems_Id",Value=lineItemBills.BillLineItems_Id,Direction=ParameterDirection.InputOutput},

                new SqlParameter("@Bill_Id",lineItemBills.Bill_Id),
                new SqlParameter("@Invoice_Id",lineItemBills.Invoice_Id),
                new SqlParameter("@LineItem_Id",lineItemBills.LineItem_Id),
                new SqlParameter("@InvoiceLineItem_Id",lineItemBills.InvoiceLineItem_Id),
                new SqlParameter("@AppliedAmount",lineItemBills.AmountApplied),

            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateBillLineItems", sqlparams);
            lineItemBills.BillLineItems_Id = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@BillLineItems_Id").FirstOrDefault()?.Value);
            return lineItemBills;
        }
    }
}
