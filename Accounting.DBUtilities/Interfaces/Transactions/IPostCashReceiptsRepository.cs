using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IPostCashReceiptsRepository
    {
        PostCashReceipts_Batches InsertPostCashReceipts_Batches(PostCashReceipts_Batches postCashReceipts_Batches);
        PostCashReceipts_PaymentTransactions InsertPostCashReceipts_PaymentTransactions(PostCashReceipts_PaymentTransactions postCashReceipts_PaymentTransactions);
        PostCashReceipts_PaidInvoices InsertPostCashReceipts_PaidInvoices(PostCashReceipts_PaidInvoices postCashReceipts_PaidInvoices);
    }
}
