using Accounting.Entities.External.CoverPay;

namespace Accounting.Web.Abstract
{
    public interface ICoverPayService
    {
        string GenerateCoverPayPaymentLink(long clientId, long policyDetail_Id, string? policyNumber, long user_Id, string? entityType, string? entityName, string? entityEmail, string? accountNumber, string? postalCode);
        ExternalPaymentDetails GetExternalPaymentDetails(long clientId, int serviceId, long policyDetailId);
        List<CoverPayOutstandingPolicyDetails> GetOutstandingPolicyDetails(long Client_Id, long Entity_Id, byte EntityType_Id, string querySearch);
        CoverPayOutstandingInvoiceDetails GetOutstandingInvoiceDetails(long clientId, string policyDetails_Id_Json);
    }
}
