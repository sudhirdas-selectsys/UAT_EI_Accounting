using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class InvoiceRepository : IInvoiceRepository
    {
        public InvoiceDetails InsertUpdateInvoiceDetails(SqlTransaction sqlTransaction, InvoiceDetails invoiceDetails)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@Id", Value = invoiceDetails.Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Invoice_Id",invoiceDetails.Invoice_Id),
                    new SqlParameter("@Amount",invoiceDetails.Amount),
                    new SqlParameter("@RemainingBalance",invoiceDetails.RemainingBalance),
                    new SqlParameter("@Entity_Id",invoiceDetails.Entity_Id),
                    new SqlParameter("@EntityType_Id",invoiceDetails.EntityType_Id),
                    new SqlParameter("@BillingType_LineItems_Directions_Id",invoiceDetails.BillingType_LineItems_Directions_Id),
                    new SqlParameter("@Paid",invoiceDetails.Paid),
                    new SqlParameter("@LOB_Id",invoiceDetails.LOB_Id),
                    new SqlParameter("@Client_Id", invoiceDetails.Client_Id)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateInvoiceDetails", spParams);
            invoiceDetails.Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            return invoiceDetails;
        }

        public List<InvoiceDetails> GetInvoiceDetails(long Invoice_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@Invoice_Id", Invoice_Id)
                      };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetails", spParams);
        }
        public InvoiceDetails GetInvoiceDetailsByInvoiceDetail_Id(long? InvoiceDetail_Id)
        {
            SqlParameter[] spParams =
            {
              new SqlParameter("@InvoiceDetail_Id", InvoiceDetail_Id)
            };

            return SQLHelper.GetObject(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByInvoiceDetail_Id", spParams);
        }
        public List<InvoiceDetails> GetInvoiceDetailsByDirection_Id(long Invoice_Id, byte Direction_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@Invoice_Id", Invoice_Id),
                        new SqlParameter("@Direction_Id", Direction_Id)
                      };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByDirection_Id", spParams);
        }

        public Invoices InsertUpdateInvoices(SqlTransaction sqlTransaction, Invoices invoices)
        {
            SqlParameter[] spParams =
             {
                    new SqlParameter(){ParameterName="@Id", Value = invoices.Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter(){ParameterName = "@InvoiceNo", Value = invoices.InvoiceNo ?? (object)DBNull.Value,Direction = ParameterDirection.InputOutput, SqlDbType = SqlDbType.VarChar,Size = 100},
                    new SqlParameter(){ParameterName="@InvoiceCreatedOn",Value= DBNull.Value,Direction=ParameterDirection.Output,SqlDbType=SqlDbType.DateTime },
                    new SqlParameter("@PolicyDetails_Id",invoices.PolicyDetails_Id),
                    new SqlParameter("@InstallmentNo",invoices.InstallmentNo),
                    new SqlParameter("@DueDate",invoices.DueDate == DateTime.MinValue ? (object)DBNull.Value : invoices.DueDate),
                    new SqlParameter("@InvoiceNote",invoices.InvoiceNote),
                    new SqlParameter("@Amount",invoices.Amount),
                    new SqlParameter("@RemainingBalance",invoices.RemainingBalance),
                    new SqlParameter("@InvoiceEffectiveDate",invoices.InvoiceEffectiveDate),
                    new SqlParameter("@Schedule_Id",invoices.Schedule_Id),
                    new SqlParameter("@ClientSpecificDueDateRule",invoices.ClientSpecificDueDateRule),
                    new SqlParameter("@Is_ClientSpecificDueDate",invoices.Is_ClientSpecificDueDate),
                    new SqlParameter("@ClientSpecificInvoiceEffectiveDate",invoices.ClientSpecificInvoiceEffectiveDate),
                    new SqlParameter("@Client_Id",invoices.Client_Id),
                    //new SqlParameter("@PaidOn",invoices.PaidOn)
                };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateInvoices", spParams);
            invoices.Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            invoices.InvoiceNo = Convert.ToString(spParams.Where(c => c.ParameterName == "@InvoiceNo").FirstOrDefault().Value);
            invoices.CreatedOn = Convert.ToDateTime(spParams.Where(c => c.ParameterName == "@InvoiceCreatedOn").FirstOrDefault().Value);
            return invoices;
        }
        public Invoices InsertInstallmentInvoices(SqlTransaction sqlTransaction, Invoices invoices)
        {
            SqlParameter[] spParams =
             [
                    new(){ParameterName="@Id", Value = invoices.Id, Direction=ParameterDirection.InputOutput},
                    new(){ParameterName = "@InvoiceNo", Value = invoices.InvoiceNo ?? (object)DBNull.Value,Direction = ParameterDirection.InputOutput, SqlDbType = SqlDbType.VarChar,Size = 100},
                    new(){ParameterName="@InvoiceCreatedOn",Value= DBNull.Value,Direction=ParameterDirection.Output,SqlDbType=SqlDbType.DateTime },
                    new("@PolicyDetails_Id",invoices.PolicyDetails_Id),
                    new("@InstallmentNo",invoices.InstallmentNo),
                    new("@DueDate",invoices.DueDate == DateTime.MinValue ? (object)DBNull.Value : invoices.DueDate),
                    new("@InvoiceNote",invoices.InvoiceNote),
                    new("@Amount",invoices.Amount),
                    new("@RemainingBalance",invoices.RemainingBalance),
                    new("@InvoiceEffectiveDate",invoices.InvoiceEffectiveDate),
                    new("@Schedule_Id",invoices.Schedule_Id),
                    new("@Client_Id", invoices.Client_Id)
                ];
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertInstallmentInvoices", spParams);
            invoices.Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            invoices.InvoiceNo = Convert.ToString(spParams.Where(c => c.ParameterName == "@InvoiceNo").FirstOrDefault().Value);
            invoices.CreatedOn = Convert.ToDateTime(spParams.Where(c => c.ParameterName == "@InvoiceCreatedOn").FirstOrDefault().Value);
            return invoices;
        }

        public Invoices GetInvoices(long PolicyDetails_Id, int InstallmentNo = 1)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
                        new SqlParameter("@InstallmentNo",InstallmentNo),
                      };
            return SQLHelper.GetObject(new Invoices(), "USP_Accounting_GetInvoices", spParams);
        }
        public List<Invoices> GetAllInvoicesByPolicyDetail_Id(long PolicyDetails_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id)
                      };
            return SQLHelper.GetObjectList(new Invoices(), "USP_Accounting_GetAllInvoicesByPolicyDetail_Id", spParams);
        }
        public void InActiveInvoices(SqlTransaction sqlTransaction, long PolicyDetails_Id, long client_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
                new SqlParameter("@Client_Id",client_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_DeleteInvoices", spParams);
        }
        public void InactiveInvoiceDetails(SqlTransaction sqlTransaction, long Invoice_Id, long client_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Invoice_Id", Invoice_Id),
                new SqlParameter("@Client_Id", client_Id)
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactiveInvoiceDetails", spParams);
        }
        public List<Invoices> GetAllInvoices()
        {
            return SQLHelper.GetObjectList(new Invoices(), "USP_Accounting_GetAllInvoices");
        }
        public List<Invoices> GetInvoicesByPolicyDetail_Id(long PolicyDetails_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@PolicyDetails_Id",PolicyDetails_Id),
                      };
            return SQLHelper.GetObjectList(new Invoices(), "USP_Accounting_GetInvoicesByPolicyDetail_Id", spParams);
        }
        public Invoices GetInvoicesByInvoice_Id(long Invoice_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@Invoice_Id",Invoice_Id),
                      };
            return SQLHelper.GetObject(new Invoices(), "USP_Accounting_GetInvoicesByInvoice_Id", spParams);
        }
        public void UpdateBalanceInInvoices(SqlTransaction sqlTransaction, long Invoice_Id, double RemainingBalance, bool Paid)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@Invoice_Id",Invoice_Id),
                        new SqlParameter("@RemainingBalance",RemainingBalance),
                        new SqlParameter("@Paid",Paid),
                        //new SqlParameter("@PaidOn",PaidOn),
                      };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdateBalanceInInvoices", spParams);
        }
        public void UpdateBalanceInInvoiceDetails(SqlTransaction sqlTransaction, long InvoiceDetail_Id, double RemainingBalance, bool Paid)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@InvoiceDetail_Id",InvoiceDetail_Id),
                        new SqlParameter("@RemainingBalance",RemainingBalance),
                        new SqlParameter("@Paid",Paid),
                        //new SqlParameter("@PaidOn",PaidOn),
                      };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdateBalanceInInvoiceDetails", spParams);
        }

        public List<InvoiceDetails> GetInvoiceDetailsByBillingLineItemDirections_Id(int BillingType_LineItems_Directions_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@BillingType_LineItems_Directions_Id",BillingType_LineItems_Directions_Id),
                      };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByBillingLineItemDirections_Id", spParams);
        }
        public void MarkAsFinanced(int invoice_Id, bool Is_Financed)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Invoice_Id",invoice_Id),
                new SqlParameter("@Is_Financed",Is_Financed)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_UpdateInvoiceFinanceStatus", sp);
        }
        public List<InvoiceDetails> GetInvoiceDetailsByLineItems_Clients_Id(long LineItems_Clients_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@LineItems_Clients_Id",LineItems_Clients_Id),
                      };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByLineItems_Clients_Id", spParams);
        }
        public List<InvoiceDetails> GetInvoiceDetailsByLineitemDirection_Id(int LineItems_Directions_Id)
        {
            SqlParameter[] spParams =
                      {
                        new SqlParameter("@LineItems_Directions_Id", LineItems_Directions_Id)
                      };
            return SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByLineitem_Direction_Id", spParams);
        }
        public Invoices GetInvoicesByInvoiceNumber(string invoiceNumber, long Client_Id, string policyReferenceId)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@PolicyNumber", policyReferenceId)
            };
            return SQLHelper.GetObject(new Invoices(), "USP_Accounting_GetInvoicesByInvoiceNumber", spParams);
        }
        public bool ValidateInvoiceEntityMismatch(string invoiceDetailIds, long entityId, byte entityTypeId)
        {
            bool isMismatch = false;
            SqlParameter[] spParams =
            {
                new SqlParameter("@InvoiceDetailIds", invoiceDetailIds),
                new SqlParameter("@Entity_Id", entityId),
                new SqlParameter("@EntityType_Id", entityTypeId)
            };
            isMismatch =
                Convert.ToBoolean(SQLHelper.ExecuteScalar(CommandType.StoredProcedure, "USP_Accounting_ValidateInvoiceEntityMismatch",spParams));
            return isMismatch;
        }
    }
}
