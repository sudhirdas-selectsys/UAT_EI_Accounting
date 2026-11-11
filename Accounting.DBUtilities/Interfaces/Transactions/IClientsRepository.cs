using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IClientsRepository
    {
        Clients GetClients(long Client_Id);
        List<Clients> GetAllClientsAssociatedToUser_Id(long User_Id);
        void UpdateImage(long Client_Id, byte[] logo);
        public EI_InstanceNames GetClientIdByEI_InstanceNames(string EI_Name);
    }
}
