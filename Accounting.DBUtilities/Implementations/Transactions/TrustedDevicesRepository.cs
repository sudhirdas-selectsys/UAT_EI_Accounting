using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class TrustedDevicesRepository : ITrustedDevicesRepository
    {
        public TrustedDevice Insert(TrustedDevice device)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter(){ParameterName = "@TrustedDeviceId",Value = device.TrustedDeviceId,Direction = ParameterDirection.InputOutput},
                new SqlParameter("@UserId",device.UserId),
                new SqlParameter("@DeviceId",device.DeviceId),
                new SqlParameter("@TokenHash",device.TokenHash),
                new SqlParameter("@Browser",device.Browser),
                new SqlParameter("@DeviceLabel",device.DeviceLabel),
                new SqlParameter("@MachineName",device.MachineName),
                new SqlParameter("@UserAgent",device.UserAgent),
                new SqlParameter("@IpAddress",device.IpAddress),
                new SqlParameter("@CreatedBy",device.CreatedBy)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertTrustedDevices", spParams);
            device.TrustedDeviceId = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@TrustedDeviceId").FirstOrDefault().Value);
            return device;
        }
        public bool Validate(long userId, Guid deviceId, byte[] tokenHash, string machineName, int trustedDays, out DateTime? expiresAt, out bool? isRevoked)
        {
            expiresAt = null;
            isRevoked = null;
            var pExists = new SqlParameter("@Exists", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var pExp = new SqlParameter("@ExpiresAt", SqlDbType.DateTime) { Direction = ParameterDirection.Output };
            var pRev = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            SqlParameter[] sqlParameters =
            {
            new SqlParameter("@UserId",userId),
            new SqlParameter("@DeviceId",deviceId),
            new SqlParameter("@MachineName",(object)machineName ?? DBNull.Value),
            new SqlParameter("@TokenHash",(object)tokenHash ?? DBNull.Value),
            new SqlParameter("@TrustedDeviceDays", trustedDays),
            pExists, pExp, pRev
            };

            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_TrustedDevicesValidate", sqlParameters);

            bool exists = pExists.Value != DBNull.Value && Convert.ToBoolean(pExists.Value);
            if (exists)
            {
                if (pExp.Value != DBNull.Value) expiresAt = Convert.ToDateTime(pExp.Value);
                if (pRev.Value != DBNull.Value) isRevoked = Convert.ToBoolean(pRev.Value);
            }
            return exists;
        }
        public void Touch(long userId, Guid deviceId, byte[] tokenHash, string ipAddress)
        {
            SqlParameter[] sqlParameters =
            {
            new SqlParameter("@UserId",userId),
            new SqlParameter("@DeviceId",deviceId),
            new SqlParameter("@TokenHash", (object)tokenHash ?? DBNull.Value),
            new SqlParameter("@IpAddress", (object)ipAddress ?? DBNull.Value)
        };

            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "USP_Accounting_TrustedDevicesTouch", sqlParameters);
        }
    }
}
