using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IUsersRepository
    {
        public Users ValidateUsers(string Username, string Password, bool Is_APIUser);
        Users GetUsers(string Username);
        Users InsertUpdateUsers(Users users);
        List<UserRoleMapping> GetUserRoleMappingByUser_Id(long User_Id);
        void UpdateDefaultClient_Id(long User_Id, long Client_Id);
        Users GetUserByUserId(long User_Id);
        #region Users
        List<Users> GetUserList(int PageNumber, int PageSize, ref int TotalRecords, string? SearchValue);
        Users GetUserById(long UserId);
        Users InsertUpdateUsers(Users users, long? ClientId = null, List<long> ClientIds = null);
        void DeleteUser(long UserId);
        List<Users> GetUserClientLevelAccessByUser_Id(long User_Id);
        void InsertUpdateUserClientLevelAccess(UserClientLevelAccess userClientLevelAccess);
        void InactiveUserClientLevelAccess(UserClientLevelAccess userClientLevelAccess);
        #endregion
        int UpdatePassword(string Username, string password);
        UserOtpVerification GetValidOtpForUser(long userId);
        void SaveOtpRecord(long userId, string otpHash, string otpType, DateTime expiresAt);
        void MarkOtpUsed(long otpId);
		public Users GetUserByEmail(string Email);
    }
}
