using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Models.MakePayments;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.SchedulerJobs.RepositoryServices
{
    public class MakePaymentRepositoryService : IMakePaymentRepositoryService
    {
        public async Task<List<BillProcessQueue>> GetPendingBillProcessQueue()
        {
            try
            {
                SqlParameter[] sp =
                {
                     new SqlParameter("@TransactionDate", (object?)DateTime.Today.Date ?? DBNull.Value),
                };

                return await SQLHelper.GetObjectList(new BillProcessQueue(), "USP_Accounting_GetPendingBillProcessQueue", sp);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: GetPendingBillProcessQueue Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<BillProcessQueue> GetBillProcessQueue(long billProcess_Id, long client_Id)
        {
            try
            {
                SqlParameter[] sp =
                {
                     new SqlParameter("@BillProcess_Id", billProcess_Id),
                     new SqlParameter("@client_Id", client_Id),
                };

                return await SQLHelper.GetObject(new BillProcessQueue(), "USP_Accounting_GetBillProcessQueue", sp);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: GetBillProcessQueue Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BillProcessQueueEvents>> GetPendingBillProcessQueueEvents(long billProcessId, long clientId, DateTime? transactionDate = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                     new SqlParameter("@BillProcess_Id", billProcessId),
                     new SqlParameter("@Client_Id", clientId),
                     new SqlParameter("@TransactionDate", (object?)transactionDate ?? DBNull.Value),
                };

                return await SQLHelper.GetObjectList(new BillProcessQueueEvents(), "USP_Accounting_GetPendingBillProcessQueueEvents", sp);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: GetPendingBillProcessQueueEvents Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BillProcessQueueEvents>> GetCompletedBillProcessQueueEvents(long billProcessId, long clientId, DateTime? transactionDate = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                     new SqlParameter("@BillProcess_Id", billProcessId),
                     new SqlParameter("@Client_Id", clientId),
                     new SqlParameter("@TransactionDate", (object?)transactionDate ?? DBNull.Value),
                };

                return await SQLHelper.GetObjectList(new BillProcessQueueEvents(), "USP_Accounting_GetCompletedBillProcessQueueEvents", sp);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: GetCompletedBillProcessQueueEvents Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UpdateBillProcessWorkFlowEvent(long billProcessId, long clientId, byte workFlowStatusId, long workFlowEventId, byte workFlowEventTypeId, string? serviceResponseMessage = null, string? processedBy = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                     new SqlParameter("@BillProcess_Id", billProcessId),
                     new SqlParameter("@Client_Id", clientId),
                     new SqlParameter("@WorkFlowStatus_Id", workFlowStatusId),
                     new SqlParameter("@WorkFlowEvent_Id", workFlowEventId),
                     new SqlParameter("@WorkFlowEventType_Id", workFlowEventTypeId),
                     new SqlParameter("@ServiceResponseMessage", (object?)serviceResponseMessage ?? DBNull.Value),
                     new SqlParameter("@ProcessedBy", (object?)processedBy ?? DBNull.Value)
                };

                return Convert.ToString(await SQLHelper.ExecuteScalar("USP_Accounting_UpdateBillProcessWorkFlowEvent", sp)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: UpdateBillProcessWorkFlowEvent Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<string> MoveToNextWorkFlowEvent(long billProcessId, long clientId, long? payment_Id, string? processedBy = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                    new SqlParameter("@BillProcess_Id", billProcessId),
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@ProcessedBy", (object?)processedBy ?? DBNull.Value),
                    new SqlParameter("@Payment_Id", payment_Id)
                };

                return Convert.ToString(await SQLHelper.ExecuteScalar("USP_Accounting_MoveToNextWorkFlowEvent", sp)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: MoveToNextWorkFlowEvent Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UpdateBillProcessQueue(long billProcessId, long clientId, byte billProcessStatusId, byte currentWorkFlowEventTypeId, string? errorMessage = null, string? processedBy = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                    new SqlParameter("@BillProcess_Id", billProcessId),
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@BillProcessStatus_Id", billProcessStatusId),
                    new SqlParameter("@CurrentWorkFlowEventType_Id", currentWorkFlowEventTypeId),
                    new SqlParameter("@ErrorMessage", (object?)errorMessage ?? DBNull.Value),
                    new SqlParameter("@ProcessedBy", (object?)processedBy ?? DBNull.Value)
                };

                return Convert.ToString(await SQLHelper.ExecuteScalar("USP_Accounting_UpdateBillProcessQueue", sp)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: UpdateBillProcessQueue Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BillProcessRetryConfiguration>> GetBillProcessRetryConfiguration()
        {
            try
            {
                return await SQLHelper.GetObjectList(new BillProcessRetryConfiguration(), "USP_Accounting_GetBillProcessRetryConfiguration");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: GetBillProcessRetryConfiguration Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<string> SetBillProcessEventRetry(long billProcessId, long clientId, byte workFlowStatusId, long workFlowEventId, byte workFlowEventTypeId, byte delayMinutes, string? serviceResponseMessage = null, string? processedBy = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                    new SqlParameter("@BillProcess_Id", billProcessId),
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@WorkFlowStatus_Id", workFlowStatusId),
                    new SqlParameter("@WorkFlowEvent_Id", workFlowEventId),
                    new SqlParameter("@WorkFlowEventType_Id", workFlowEventTypeId),
                    new SqlParameter("@DelayMinutes", delayMinutes),
                    new SqlParameter("@ServiceResponseMessage", (object?)serviceResponseMessage ?? DBNull.Value),
                    new SqlParameter("@ProcessedBy", (object?)processedBy ?? DBNull.Value)
                };

                return Convert.ToString(await SQLHelper.ExecuteScalar("USP_Accounting_SetBillProcessEventRetry", sp)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: SetBillProcessEventRetry Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<string> FailBillProcess(long billProcessId, long clientId, long workFlowEventId, byte workFlowEventTypeId, string? serviceResponseMessage = null, string? processedBy = null)
        {
            try
            {
                SqlParameter[] sp =
                {
                    new SqlParameter("@BillProcess_Id", billProcessId),
                    new SqlParameter("@Client_Id", clientId),
                    new SqlParameter("@WorkFlowEvent_Id", workFlowEventId),
                    new SqlParameter("@WorkFlowEventType_Id", workFlowEventTypeId),
                    new SqlParameter("@ServiceResponseMessage", (object?)serviceResponseMessage ?? DBNull.Value),
                    new SqlParameter("@ProcessedBy", (object?)processedBy ?? DBNull.Value)
                };

                return Convert.ToString(await SQLHelper.ExecuteScalar("USP_Accounting_FailBillProcess", sp)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: FailBillProcess Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<Payments_BillsMapping> GetPayments_BillsMapping(long Payment_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Payment_Id", Payment_Id),
            };

            return await SQLHelper.GetObject(new Payments_BillsMapping(), "USP_Accounting_GetPayments_BillsMapping", sp);
        }
        public async Task<Payments_BillsMapping> InsertPayments_BillsMapping(SqlTransaction Transaction, Payments_BillsMapping Payments_BillsMapping)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@PaymentBill_Id", Value = Payments_BillsMapping.PaymentBill_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Payment_Id", Payments_BillsMapping.Payment_Id),
                new SqlParameter("@Bill_Id", Payments_BillsMapping.Bill_Id),
            };

            await SQLHelper.ExecuteNonQuery(Transaction, CommandType.StoredProcedure, "USP_Accounting_InsertPayments_BillsMapping", sp);
            Payments_BillsMapping.PaymentBill_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@PaymentBill_Id").FirstOrDefault().Value);
            return Payments_BillsMapping;
        }

        public async Task<ExternalBillReferenceLineItems> GetExternalReferenceLineItems(long Entity_Id, byte EntityType_Id, long Invoice_Id, long InvoiceLineItem_Id, long LineItem_Id, byte Direction_Id, long Client_Id)
        {
            try
            {
                SqlParameter[] sp =
                {
                    new SqlParameter("@Entity_Id", Entity_Id),
                    new SqlParameter("@EntityType_Id", EntityType_Id),
                    new SqlParameter("@Invoice_Id", Invoice_Id),
                    new SqlParameter("@InvoiceLineItem_Id", InvoiceLineItem_Id),
                    new SqlParameter("@LineItem_Id", LineItem_Id),
                    new SqlParameter("@Direction_Id", Direction_Id),
                    new SqlParameter("@Client_Id", Client_Id)
                    //,new SqlParameter("@SourceType_Id", SourceType_Id)
                };
                return await SQLHelper.GetObject(new ExternalBillReferenceLineItems(), "USP_Accounting_IsExternalBillExists", sp);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentRepositoryService: GetExternalReferenceLineItems Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task<StateDetails> GetStateDetailsByName(string Name, long Client_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@Name",Name)
            };

            return await SQLHelper.GetObject(new StateDetails(), "USP_Accounting_GetStateDetailByName", spParams);
        }

        public async Task<StateDetails> InsertUpdateStateDetails(SqlTransaction sqlTransaction, StateDetails stateDetails)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){ParameterName="@StateDetail_Id", Value = stateDetails.StateDetail_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id",stateDetails.Client_Id),
                new SqlParameter("@Name",stateDetails.Name),
            };

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertStateDetails", sqlParameters);
            stateDetails.StateDetail_Id = Convert.ToInt32(sqlParameters.Where(c => c.ParameterName == "@StateDetail_Id").FirstOrDefault().Value);
            return stateDetails;
        }

        public async Task<Bill> InsertUpdateBill(SqlTransaction sqlTransaction, Bill bill)
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

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateBill", sqlparams);
            bill.Bill_Id = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@Bill_Id").FirstOrDefault()?.Value);
            return bill;
        }

        public async Task<LineItemBill> InsertUpdateBillLineItems(SqlTransaction sqlTransaction, LineItemBill lineItemBills)
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

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateBillLineItems", sqlparams);
            lineItemBills.BillLineItems_Id = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@BillLineItems_Id").FirstOrDefault()?.Value);
            return lineItemBills;
        }

        public async Task<Payments> InsertUpdatePayments(SqlTransaction sqlTransaction, Payments payments)
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
                new SqlParameter("@PayeeNameOnCheck",payments.PayeeNameOnCheck)
            };

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePayments", parameters);
            payments.Payment_Id = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@Payment_Id").FirstOrDefault().Value);
            return payments;
        }

        public async Task<Clients> GetClients(long Client_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id",Client_Id)
            };
            return await SQLHelper.GetObject(new Clients(), "USP_Accounting_GetClients", parameters);
        }

        public async Task<Payments> GetPaymentDetails(long payment_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payments_Id",payment_Id)
            };
            return await SQLHelper.GetObject(new Payments(), "USP_Accounting_GetPaymentsDetails", parameters);
        }
        public async Task<Invoices> GetInvoicesByInvoice_Id(long Invoice_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id),
            };

            return await SQLHelper.GetObject(new Invoices(), "USP_Accounting_GetInvoicesByInvoice_Id", spParams);
        }
        public async Task<PolicyDetails> GetPolicyDetails(long Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Id",Id),
            };

            return await SQLHelper.GetObject(new PolicyDetails(), "USP_Accounting_GetPolicyDetails", spParams);
        }
        public async Task<AgentDetails> GetAgentDetails(long AgentDetail_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@AgentDetail_Id",AgentDetail_Id),
            };
            return await SQLHelper.GetObject(new AgentDetails(), "USP_Accounting_GetAgentDetails", spParams);
        }
        public async Task<List<LU_BillingTypes>> GetAllBillingTypes()
        {
            return await SQLHelper.GetObjectList(new LU_BillingTypes(), "USP_Accounting_GetAllBillingTypes");
        }

        public async Task<LU_Clients_AccountingTypes_BillingTypes> GetClients_AccountingTypes_BillingTypes(long Client_Id, byte BillingType_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@BillingType_Id",BillingType_Id)
            };

            return await SQLHelper.GetObject(new LU_Clients_AccountingTypes_BillingTypes(), "USP_Accounting_GetClients_AccountingTypes_BillingTypes", sp);
        }

        public async Task<PaymentApplications> InsertUpdatePaymentApplications(SqlTransaction sqlTransaction, PaymentApplications paymentApplications)
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

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePaymentApplications", sqlParameters);
            paymentApplications.PaymentApplication_Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@PaymentApplication_Id").FirstOrDefault().Value);
            return paymentApplications;
        }

        public async Task<List<InvoiceDetails>> GetInvoiceDetailsByDirection_Id(long Invoice_Id, byte Direction_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Invoice_Id", Invoice_Id),
                new SqlParameter("@Direction_Id", Direction_Id)
            };

            return await SQLHelper.GetObjectList(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByDirection_Id", spParams);
        }

        public async Task<PaymentApplicationLineItems> InsertUpdatePaymentApplicationLineItems(SqlTransaction sqlTransaction, PaymentApplicationLineItems paymentApplicationLineItems)
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
            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdatePaymentApplicationLineItems", sqlParameters);
            paymentApplicationLineItems.PaymentApplication_Id = Convert.ToInt64(sqlParameters.Where(c => c.ParameterName == "@PaymentApplicationLineItem_Id").FirstOrDefault().Value);
            return paymentApplicationLineItems;
        }

        public async Task UpdateBalanceInInvoiceDetails(SqlTransaction sqlTransaction, long InvoiceDetail_Id, double RemainingBalance, bool Paid)
        {
            SqlParameter[] spParams =
            {
              new SqlParameter("@InvoiceDetail_Id",InvoiceDetail_Id),
              new SqlParameter("@RemainingBalance",RemainingBalance),
              new SqlParameter("@Paid",Paid),
              //new SqlParameter("@PaidOn",PaidOn),
            };

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdateBalanceInInvoiceDetails", spParams);
        }

        public async Task UpdatePaymentRemainingAmount(SqlTransaction sqlTransaction, long Payments_Id, double RemainingAmount)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Payment_Id",Payments_Id),
                new SqlParameter("@RemainingAmount",RemainingAmount)
            };

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdatePaymentRemainingAmount", parameters);
        }

        public async Task UpdateBalanceInInvoices(SqlTransaction sqlTransaction, long Invoice_Id, double RemainingBalance, bool Paid)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Invoice_Id",Invoice_Id),
                new SqlParameter("@RemainingBalance",RemainingBalance),
                new SqlParameter("@Paid",Paid),
                //new SqlParameter("@PaidOn",PaidOn),
            };

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdateBalanceInInvoices", spParams);
        }

        public async Task<InvoiceDetails> GetInvoiceDetailsByInvoiceDetail_Id(long? InvoiceDetail_Id)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@InvoiceDetail_Id", InvoiceDetail_Id)
            };

            return await SQLHelper.GetObject(new InvoiceDetails(), "USP_Accounting_GetInvoiceDetailsByInvoiceDetail_Id", spParams);
        }

        public async Task UpdateBillDetailsForBillProcessQueue(SqlTransaction sqlTransaction, long Client_Id, long BillProcess_Id, long Bill_Id, string BillNumber)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@BillProcess_Id",BillProcess_Id),
                new SqlParameter("@Bill_Id",Bill_Id),
                new SqlParameter("@BillNumber",BillNumber)
            };

            await SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_UpdateBillDetailsForBillProcessQueue", spParams);
        }


    }
}
