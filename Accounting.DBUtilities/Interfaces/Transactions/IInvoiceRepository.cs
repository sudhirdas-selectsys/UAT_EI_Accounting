using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IInvoiceRepository
    {
        InvoiceDetails InsertUpdateInvoiceDetails(SqlTransaction sqlTransaction, InvoiceDetails invoiceDetails);
        List<InvoiceDetails> GetInvoiceDetails(long Invoice_Id);
        Invoices InsertUpdateInvoices(SqlTransaction sqlTransaction, Invoices invoices);
        Invoices GetInvoices(long PolicyDetails_Id, int InstallmentNo = 1);
        void InactiveInvoiceDetails(SqlTransaction sqlTransaction, long Invoice_Id, long client_Id);
        List<InvoiceDetails> GetInvoiceDetailsByDirection_Id(long Invoice_Id, byte Direction_Id);
        List<Invoices> GetAllInvoices();
        List<Invoices> GetInvoicesByPolicyDetail_Id(long PolicyDetails_Id);
        Invoices GetInvoicesByInvoice_Id(long Invoice_Id);
        void UpdateBalanceInInvoices(SqlTransaction sqlTransaction, long Invoice_Id, double RemainingBalance, bool Paid);
        void UpdateBalanceInInvoiceDetails(SqlTransaction sqlTransaction, long InvoiceDetail_Id, double RemainingBalance, bool Paid);
        InvoiceDetails GetInvoiceDetailsByInvoiceDetail_Id(long? InvoiceDetail_Id);
        List<InvoiceDetails> GetInvoiceDetailsByBillingLineItemDirections_Id(int BillingType_LineItems_Directions_Id);
        List<Invoices> GetAllInvoicesByPolicyDetail_Id(long PolicyDetails_Id);
        void InActiveInvoices(SqlTransaction sqlTransaction, long PolicyDetails_Id, long client_Id);
        Invoices InsertInstallmentInvoices(SqlTransaction sqlTransaction, Invoices invoices);
        void MarkAsFinanced(int invoice_Id, bool Is_Financed);
        List<InvoiceDetails> GetInvoiceDetailsByLineItems_Clients_Id(long LineItems_Clients_Id);
        List<InvoiceDetails> GetInvoiceDetailsByLineitemDirection_Id(int LineItems_Directions_Id);
        Invoices GetInvoicesByInvoiceNumber(string invoiceNumber, long Client_Id, string policyReferenceId);
        bool ValidateInvoiceEntityMismatch(string invoiceDetailIds, long entityId, byte entityTypeId);
    }
}
