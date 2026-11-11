using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System.Data;
using Accounting.Entities.External.QBO.Journal;
using static Azure.Core.HttpHeader;
using Accounting.Entities.External.ChecKeeper;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class PaymentsRepository : IPaymentsRepository
    {
        public List<Payments> GetPaymentsSearchResults(Payments PaymentsSearch )
        {
            try
            {
                SqlParameter[] parameters =
           {
                new SqlParameter("@EntityType_Id", PaymentsSearch.EntityType_Id == 0 ? null : PaymentsSearch.EntityType_Id),
                new SqlParameter("@Entity_Id", PaymentsSearch.Entity_Id == 0 ? null : PaymentsSearch.Entity_Id),
                new SqlParameter("@PaymentDate", PaymentsSearch.PaymentDate),
                new SqlParameter("@Amount", PaymentsSearch.Amount == 0 ? null : PaymentsSearch.Amount ),
                new SqlParameter("@PaymentType_Id", PaymentsSearch.PaymentType_Id == 0 ? null : PaymentsSearch.PaymentType_Id),
                new SqlParameter("@ReferenceNumber", PaymentsSearch.ReferenceNumber),
                new SqlParameter("@PaymentStatus_Id", PaymentsSearch.PaymentStatus_Id == 0 ? null : PaymentsSearch.PaymentStatus_Id),
                new SqlParameter("@OperatingBank_Id", PaymentsSearch.OperatingBank_Id == 0 ? null : PaymentsSearch.OperatingBank_Id),
                new SqlParameter("@Notes", PaymentsSearch.Notes),
                new SqlParameter("@PaymentDirection_Id", PaymentsSearch.PaymentDirection_Id == 0 ? null : PaymentsSearch.PaymentDirection_Id),
                new SqlParameter("@Client_Id", PaymentsSearch.Client_Id),
                new SqlParameter("@OutstandingBalances", PaymentsSearch.OutstandingBalances),
            };
                return SQLHelper.GetObjectList(new Payments(), "USP_Accounting_GetPaymentsSearchResults", parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public List<Payments> GetPaymentsSearchResults(Payments PaymentsSearch, int PageSize, int PageNumber, string SortOrder,  ref long TotalRecords)
        {
            try
            {
                SqlParameter[] parameters =
           {
                new SqlParameter("@EntityType_Id", PaymentsSearch.EntityType_Id == 0 ? null : PaymentsSearch.EntityType_Id),
                new SqlParameter("@Entity_Id", PaymentsSearch.Entity_Id == 0 ? null : PaymentsSearch.Entity_Id),
                //new SqlParameter("@PaymentDate", PaymentsSearch.PaymentDate),
                new SqlParameter("@PaymentFromDate", PaymentsSearch.PaymentFromDate),
                new SqlParameter("@PaymentToDate", PaymentsSearch.PaymentToDate),
                new SqlParameter("@Amount", PaymentsSearch.Amount == 0 ? null : PaymentsSearch.Amount ),
                new SqlParameter("@PaymentType_Id", PaymentsSearch.PaymentType_Id == 0 ? null : PaymentsSearch.PaymentType_Id),
                new SqlParameter("@ReferenceNumber", PaymentsSearch.ReferenceNumber),
                new SqlParameter("@PaymentStatus_Id", PaymentsSearch.PaymentStatus_Id == 0 ? null : PaymentsSearch.PaymentStatus_Id),
                new SqlParameter("@OperatingBank_Id", PaymentsSearch.OperatingBank_Id == 0 ? null : PaymentsSearch.OperatingBank_Id),
                new SqlParameter("@Notes", PaymentsSearch.Notes),
                new SqlParameter("@PaymentDirection_Id", PaymentsSearch.PaymentDirection_Id == 0 ? null : PaymentsSearch.PaymentDirection_Id),
                new SqlParameter("@Client_Id", PaymentsSearch.Client_Id),
                new SqlParameter("@Payment_Id",PaymentsSearch.Payment_Id),
                new SqlParameter("@OutstandingBalances", PaymentsSearch.OutstandingBalances),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@SortOrder", SortOrder),
                new SqlParameter(){ ParameterName="@TotalRecords",Value=TotalRecords,Direction=ParameterDirection.InputOutput}
            };
                var result = SQLHelper.GetObjectList(new Payments(), "USP_Accounting_GetPaymentsSearchResults_paging_2026081", parameters);
                TotalRecords = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public Payments InsertUpdatePayments(SqlTransaction sqlTransaction, Payments payments)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter(){ParameterName="@Payment_Id", Value = payments.Payment_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@EntityType_Id", payments.EntityType_Id),
                new SqlParameter("@Entity_Id", payments.Entity_Id),
                new SqlParameter("@PaymentDate", payments.PaymentDate),
                new SqlParameter("@Amount", payments.Amount),
                new SqlParameter("@PaymentType_Id", payments.PaymentType_Id),
                new SqlParameter("@ReferenceNumber", payments.ReferenceNumber),
                new SqlParameter("@PaymentStatus_Id", payments.PaymentStatus_Id),
                new SqlParameter("@OperatingBank_Id", payments.OperatingBank_Id),
                new SqlParameter("@Notes", payments.Notes),
                new SqlParameter("@RemainingAmount", payments.RemainingAmount),
                new SqlParameter("@PaymentDirection_Id", payments.PaymentDirection_Id),
                new SqlParameter("@Client_Id", payments.Client_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePayments", parameters);
            payments.Payment_Id = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@Payment_Id").FirstOrDefault().Value);
            return payments;
        }
        public Payments GetPaymentDetails(long Payments_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payments_Id",Payments_Id)
            };
            return SQLHelper.GetObject(new Payments(), "USP_Accounting_GetPaymentsDetails", parameters);
        }
        public Payments GetPaymentDetailsForReceipt(long Payments_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payments_Id",Payments_Id)
            };
            return SQLHelper.GetObject(new Payments(), "USP_Accounting_GetPaymentsDetails", parameters);
        }
        public List<Payments> GetAllPayments(long Client_Id, int PageSize, int PageNumber, ref long TotalRecords)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", Client_Id),
                    new SqlParameter("@PageSize",PageSize),
                    new SqlParameter("@PageNumber",PageNumber),
                    new SqlParameter(){ ParameterName="@TotalRecords",Value=TotalRecords,Direction=ParameterDirection.InputOutput}
                };
                var result = SQLHelper.GetObjectList(new Payments(), "USP_Accounting_GetAllPayments", parameters);
                TotalRecords = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void DeletePaymentDetails(long Payments_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payments_Id",Payments_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeletePaymentDetails", parameters);

        }

        public void UpdatePaymentRemainingAmount(SqlTransaction sqlTransaction, long Payments_Id, double RemainingAmount)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id",Payments_Id),
                new SqlParameter("@RemainingAmount",RemainingAmount)
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdatePaymentRemainingAmount", parameters);

        }
        public PaymentApplications InsertUpdatePaymentApplications(SqlTransaction sqlTransaction, PaymentApplications paymentApplications)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){ParameterName = "@PaymentApplication_Id",Value =  paymentApplications.PaymentApplication_Id, Direction = ParameterDirection.InputOutput},
                new SqlParameter("@Payment_Id", paymentApplications.Payment_Id),
                new SqlParameter("@Invoice_Id", paymentApplications.Invoice_Id),
                new SqlParameter("@AppliedDate", paymentApplications.AppliedDate),
                new SqlParameter("@TotalAppliedAmount", paymentApplications.TotalAppliedAmount),
                new SqlParameter("@Notes", paymentApplications.Notes),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePaymentApplications", sqlParameters);
            paymentApplications.PaymentApplication_Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@PaymentApplication_Id").FirstOrDefault().Value);
            return paymentApplications;
        }
        public PaymentApplicationLineItems InsertUpdatePaymentApplicationLineItems(SqlTransaction sqlTransaction, PaymentApplicationLineItems paymentApplicationLineItems)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){ParameterName = "@PaymentApplicationLineItem_Id",Value =  paymentApplicationLineItems.PaymentApplicationLineItem_Id, Direction = ParameterDirection.InputOutput},
                new SqlParameter("@PaymentApplication_Id", paymentApplicationLineItems.PaymentApplication_Id),
                new SqlParameter("@InvoiceLineItem_Id", paymentApplicationLineItems.InvoiceLineItem_Id),
                new SqlParameter("@AppliedAmount", paymentApplicationLineItems.AppliedAmount),
                new SqlParameter("@Is_SplitPayment", paymentApplicationLineItems.Is_SplitPayment),
                new SqlParameter("@SubType_Id", paymentApplicationLineItems.SubType_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePaymentApplicationLineItems", sqlParameters);
            paymentApplicationLineItems.PaymentApplication_Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@PaymentApplicationLineItem_Id").FirstOrDefault().Value);
            return paymentApplicationLineItems;
        }
        public PaymentApplications GetPaymentApplicationsByInvoice_Id(long Invoice_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id)
            };
            return SQLHelper.GetObject(new PaymentApplications(), "USP_Accounting_GetPaymentApplicationsByInvoice_Id", parameters);
        }
        public PaymentApplicationLineItems GetPaymentApplicationLineItemsByInvoiceLineItem_Id(long InvoiceLineItem_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@InvoiceLineItem_Id",InvoiceLineItem_Id)
            };
            return SQLHelper.GetObject(new PaymentApplicationLineItems(), "USP_Accounting_GetPaymentApplicationLineItemsByInvoiceLineItem_Id", parameters);
        }
        public List<PaymentApplications> GetPaymentApplicationsByPayment_Id(long Payment_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id",Payment_Id)
            };
            return SQLHelper.GetObjectList(new PaymentApplications(), "USP_Accounting_GetPaymentApplicationsByPayment_Id", parameters);
        }
        public List<Payments> GetPaymentsByEntity_Id(long Entity_Id, byte Entitytype_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Entity_Id",Entity_Id),
                new SqlParameter("@Entitytype_Id",Entitytype_Id)
            };
            return SQLHelper.GetObjectList(new Payments(), "USP_GetAccounting_GetPaymentsByEntity_Id", parameters);
        }
        public List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByPayment_Id(long Payment_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id",Payment_Id),
            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetPaymentApplicationLineItemsByPayment_Id", parameters);
        }
        public List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsForReceiptByPayment_Id(long Payment_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id",Payment_Id),
            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetPaymentApplicationLineItemsForReceiptByPayment_Id", parameters);
        }
        public PaymentApplicationLineItems GetWriteoffPaymentApplicationLineItemsByPayment_Id(long Payment_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id",Payment_Id),
            };
            return SQLHelper.GetObject(new PaymentApplicationLineItems(), "USP_Accounting_GetWriteoffPaymentApplicationLineItemsByPayment_Id", parameters);
        }
        public List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByInvoice_Id(long Invoice_Id, byte Direction_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id),
                new SqlParameter("@Direction_Id",Direction_Id),
            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetPaymentApplicationLineItemsByInvoice_Id", parameters);
        }
        public List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByInvoiceDetail_Id(long InvoiceDetail_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@InvoiceDetail_Id",InvoiceDetail_Id),

            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetPaymentApplicationLineItemsByInvoiceDetail_Id", parameters);
        }
        public PaymentApplications GetPaymentApplicationsByPaymentApplication_Id(long PaymentApplication_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@PaymentApplication_Id",PaymentApplication_Id),
            };
            return SQLHelper.GetObject(new PaymentApplications(), "USP_Accounting_GetPaymentApplicationsByPaymentApplication_Id", parameters);
        }
        public void InactivePaymentApplicationLineItems(SqlTransaction sqlTransaction, long PaymentApplicationLineItem_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@PaymentApplicationLineItem_Id",PaymentApplicationLineItem_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactivePaymentApplicationLineItems", parameters);

        }
        public List<InvoicePaymentDetails> GetPaymentDetailsByInvoice_Id(long Invoice_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id),
            };
            return SQLHelper.GetObjectList(new InvoicePaymentDetails(), "USP_Accounting_GetInvoicePaymentDetails", parameters);
        }
        public List<PaymentApplicationLineItems> GetAllPaymentApplicationLineItemsByInvoice_Id(long Invoice_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id),
            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetAllPaymentApplicationLineItemsByInvoice_Id", parameters);
        }
        public List<Payments> GetDeletedPayments(long Client_Id)
        {
            SqlParameter[] parameters =
           {
                new SqlParameter("@Client_Id",Client_Id),
            };
            return SQLHelper.GetObjectList(new Payments(), "USP_Accounting_GetDeletedPayments", parameters);
        }
        public List<PaymentApplicationLineItems> GetAllPaymentApplicationLineItemsByPolicyDetail_Id(long PolicyDetails_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetAllPaymentApplicationLineItemsByPolicyDetails_Id", parameters);
        }
        public Clients GetClientByPaymentId(long Payment_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id", Payment_Id),
            };
            return SQLHelper.GetObject(new Clients(), "USP_Accounting_GetPaymentDetail_Id", parameters);

        }
        public List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByPaymentApplication_Id(long PaymentApplication_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@PaymentApplication_Id",PaymentApplication_Id)
            };
            return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_Accounting_GetPaymentApplicationLineItemsByPaymentApplication_Id", sp);
        }
        public int UpdatePaymentStatus(SqlTransaction sqlTransaction, long PaymentId, byte PaymentStatus_Id, long client_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id", client_Id),
                new SqlParameter("@Payment_Id",PaymentId),
                new SqlParameter("@PaymentStatus_Id",PaymentStatus_Id)
            };
            return SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdatePaymentStatus", sp);
        }
        public List<PaymentApplicationLineItems> ApplyPayments(long Client_Id, string InvoiceDetailsJson, long Payment_Id, byte Direction_Id, bool IsNetNegative)
        {
            try
            {
                SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@InvoiceDetailsJson",InvoiceDetailsJson),
                new SqlParameter("@Payment_Id",Payment_Id),
                new SqlParameter("@Direction_Id",Direction_Id),
                new SqlParameter("@IsNetNegative",IsNetNegative)
            };
                return SQLHelper.GetObjectList(new PaymentApplicationLineItems(), "USP_ApplyPayments", sp);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public List<Error> ValidateInvoiceCOAConfiguration(long Client_Id, string InvoiceDetailIds, byte Service_Id, long Payment_Id)
        {
            try
            {
                SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@InvoiceDetailIds",InvoiceDetailIds),
                new SqlParameter("@Service_Id",Service_Id),
                new SqlParameter("@Payment_Id",Payment_Id)
            };
                return SQLHelper.GetObjectList(new Error(), "USP_Accounting_ValidateInvoiceCOAConfiguration_V1", sp);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string UnApplyPayments(long Client_Id, byte Direction_Id, string PaymentApplicationLineitemJson)
        {
            try
            {
                SqlParameter[] sp =
                {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@Direction_Id",Direction_Id),
                new SqlParameter("@PaymentApplicationLineitemJson",PaymentApplicationLineitemJson),
            };
                return Convert.ToString(SQLHelper.ExecuteScalar("USP_UnApplyPayments", sp)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public List<PrintCheckDetails> GetPrintCheckDetails(long paymentId)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id", paymentId)
            };
            return SQLHelper.GetObjectList(new PrintCheckDetails(), "USP_Accounting_GetCheckDetailsByPayment_Id", parameters);
        }
        public int UpdateCheckPaymentPrintStatus(long Payment_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Payment_Id",Payment_Id)
            };
            return SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_UpdateCheckPaymentPrintStatus", sp);
        }
        public List<BillProcessQueueWeb> GetBillProcessQueues(long client_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id", client_Id),
                //new SqlParameter("@TransactionDate","18 Aug 2026")
            };
            return SQLHelper.GetObjectList(new BillProcessQueueWeb(), "USP_Accounting_GetBillProcessQueueByClient", parameters);
        }
        public List<BillProcessQueueEventsWeb> GetBillProcessQueuesView(long client_Id, long billProcess_Id, DateTime date)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id", client_Id),
                new SqlParameter("@TransactionDate",date),
                new SqlParameter("@BillProcess_Id",billProcess_Id)
            };
            return SQLHelper.GetObjectList(new BillProcessQueueEventsWeb(), "USP_Accounting_GetBillProcessQueueEventsByClient", parameters);

        }
        public string RetryBillBatch(long client_Id, long user_Id, long payment_Id, DateTime transactionDate)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id", client_Id),
                new SqlParameter("@User_Id",user_Id.ToString()),
                new SqlParameter("@TransactionDate",transactionDate),
                new SqlParameter("@Payment_Id",payment_Id)
            };
            return Convert.ToString(SQLHelper.ExecuteScalar(CommandType.StoredProcedure, "USP_Accounting_RetryBillProcess", parameters));
        }
public List<Payments> GetAllPaymentsForPrintCheck(long Client_Id, int PageSize, int PageNumber, ref long TotalRecords)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", Client_Id),
                    new SqlParameter("@PageSize",PageSize),
                    new SqlParameter("@PageNumber",PageNumber),
                    new SqlParameter(){ ParameterName="@TotalRecords",Value=TotalRecords,Direction=ParameterDirection.InputOutput}
                };
                var result = SQLHelper.GetObjectList(new Payments(), "USP_Accounting_GetAllPaymentsForPrintCheck", parameters);
                TotalRecords = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public List<Payments> GetCheckPaymentsSearchResults(Payments PaymentsSearch, int PageSize, int PageNumber, string SortOrder, ref long TotalRecords)
        {
            try
            {
                SqlParameter[] parameters =
           {
                new SqlParameter("@EntityType_Id", PaymentsSearch.EntityType_Id == 0 ? null : PaymentsSearch.EntityType_Id),
                new SqlParameter("@Entity_Id", PaymentsSearch.Entity_Id == 0 ? null : PaymentsSearch.Entity_Id),
                //new SqlParameter("@PaymentDate", PaymentsSearch.PaymentDate),
                new SqlParameter("@PaymentFromDate", PaymentsSearch.PaymentFromDate),
                new SqlParameter("@PaymentToDate", PaymentsSearch.PaymentToDate),
                new SqlParameter("@Amount", PaymentsSearch.Amount == 0 ? null : PaymentsSearch.Amount ),
                new SqlParameter("@PaymentType_Id", PaymentsSearch.PaymentType_Id == 0 ? null : PaymentsSearch.PaymentType_Id),
                new SqlParameter("@ReferenceNumber", PaymentsSearch.ReferenceNumber),
                new SqlParameter("@PaymentStatus_Id", PaymentsSearch.PaymentStatus_Id == 0 ? null : PaymentsSearch.PaymentStatus_Id),
                new SqlParameter("@OperatingBank_Id", PaymentsSearch.OperatingBank_Id == 0 ? null : PaymentsSearch.OperatingBank_Id),
                new SqlParameter("@Notes", PaymentsSearch.Notes),
                new SqlParameter("@PaymentDirection_Id", PaymentsSearch.PaymentDirection_Id == 0 ? null : PaymentsSearch.PaymentDirection_Id),
                new SqlParameter("@Client_Id", PaymentsSearch.Client_Id),
                new SqlParameter("@Payment_Id",PaymentsSearch.Payment_Id),
                new SqlParameter("@OutstandingBalances", PaymentsSearch.OutstandingBalances),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@SortOrder", SortOrder),
                new SqlParameter(){ ParameterName="@TotalRecords",Value=TotalRecords,Direction=ParameterDirection.InputOutput}
            };
                var result = SQLHelper.GetObjectList(new Payments(), "USP_Accounting_GetCheckPaymentsSearchResults", parameters);
                TotalRecords = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public InvoicePaymentDetails GetInvoiceAppliedPaymentDetails(long Invoice_Id, long Client_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id),
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObject(new InvoicePaymentDetails(), "USP_Accounting_GetInvoiceAppliedPaymentDetails", parameters);
        }
    }
}
