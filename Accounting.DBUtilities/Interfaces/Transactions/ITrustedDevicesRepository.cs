using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ITrustedDevicesRepository
    {
        public TrustedDevice Insert(TrustedDevice device);
        public bool Validate(long userId, Guid deviceId, byte[] tokenHash, string machineName, int trustedDays, out DateTime? expiresAt, out bool? isRevoked);
        public void Touch(long userId, Guid deviceId, byte[] tokenHash, string ipAddress);
    }
}
