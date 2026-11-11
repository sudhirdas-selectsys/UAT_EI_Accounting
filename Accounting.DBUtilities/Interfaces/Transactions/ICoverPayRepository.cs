using Accounting.Entities.External.CoverPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ICoverPayRepository
    {
        CoverPayInvoiceDetails GetCoverPayInvoiceDetails(long clientId, long policyDetailId);
        long InsertUpdateExternalPaymentDetails(ExternalPaymentDetails externalPaymentDetails, long user_Id);
        ExternalPaymentDetails GetExternalPaymentDetails(long clientId, int serviceId, long policyDetailId);
        List<CoverPayOutstandingPolicyDetails> GetOutstandingPolicyDetails(long Client_Id, long Entity_Id, byte EntityType_Id, string querySearch);
        CoverPayOutstandingInvoiceDetails GetOutstandingInvoiceDetails(long clientId, string policyDetails_Id_Json);
    }
}
