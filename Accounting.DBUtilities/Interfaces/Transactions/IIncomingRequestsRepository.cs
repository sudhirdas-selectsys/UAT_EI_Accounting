using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IIncomingRequestsRepository
    {
        IncomingRequests InsertIncomingRequests(IncomingRequests incomingRequests);
    }
}
