using Microsoft.Data.SqlClient;
using Accounting.Entities.DB.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class UsersRepository : IUsersRepository
    {
        public Users ValidateUsers(string Username, string Password, bool Is_APIUser)
        {
            SqlParameter[] _parameters =
            {
                new SqlParameter("@Username", Username),
                new SqlParameter("@Password", Password),
                new SqlParameter("@Is_APIUser", Is_APIUser)
            };
            return SQLHelper.GetObject(new Users(), "USP_Accounting_ValidateUser", _parameters);
        }
        public Users GetUsers(string Username)
        {
            SqlParameter[] _parameters =
            {
                new SqlParameter("@Username", Username),
            };
            return SQLHelper.GetObject(new Users(), "USP_Accounting_GetUsers", _parameters);
        }
        public Users InsertUpdateUsers(Users users)
        {
            SqlParameter[] spParams =
              {
                    new SqlParameter(){ParameterName="@User_Id", Value = users.User_Id, Direction=ParameterDirection.InputOutput},
                    new SqlParameter("@Username",users.Username),
                    new SqlParameter("@Password",users.Password),
                    new SqlParameter("@Email",users.Email),
                    new SqlParameter("@Client_Id",users.Client_Id),
                    new SqlParameter("@Role_Id",users.Role_Id),
                    new SqlParameter("@Is_APIUser",users.Is_APIUser)
                };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateUsers", spParams);
            users.User_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@User_Id").FirstOrDefault().Value);
            return users;
        }
        public List<UserRoleMapping> GetUserRoleMappingByUser_Id(long User_Id)
        {
            SqlParameter[] _parameters =
            {
                new SqlParameter("@User_Id", User_Id),
            };
            return SQLHelper.GetObjectList(new UserRoleMapping(), "USP_Accounting_GetUserRoleMappingByUser_Id", _parameters);
        }
        public void UpdateDefaultClient_Id(long User_Id, long Client_Id)
        {
            SqlParameter[] _parameters =
            {
                new SqlParameter("@User_Id", User_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_UpdateDefaultClient_Id", _parameters);
        }
        public Users GetUserByUserId(long User_Id)
        {
            SqlParameter[] _parameters =
            {
                new SqlParameter("@User_Id", User_Id),
            };
            return SQLHelper.GetObject(new Users(), "USP_Accounting_GetUserByUserId", _parameters);
        }
        #region Users
        public List<Users> GetUserList(int PageNumber, int PageSize, ref int TotalRecords, string? SearchValue)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@SearchValue",SearchValue),
                new SqlParameter(){ParameterName="@TotalRecords",Direction=ParameterDirection.Output,Size=100 },
            };
            var _Result = SQLHelper.GetObjectList(new Users(), "USP_Accounting_GetAllUsers", sqlparams);
            TotalRecords = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
            return _Result;
        }
        public Users GetUserById(long UserId)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@User_Id",UserId)
            };
            return SQLHelper.GetObject(new Users(), "USP_Accounting_GetUserByUserId", sqlparams);
        }
        public Users InsertUpdateUsers(Users users, long? ClientId = null, List<long> ClientIds = null)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter (){ParameterName="@User_Id",Value=users.User_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Username",users.Username),
                new SqlParameter("@Password",users.Password),
                new SqlParameter("@Email",users.Email),
                new SqlParameter("@FirstName",users.FirstName),
                new SqlParameter("@LastName",users.LastName),
                new SqlParameter("@Client_Id",ClientId > 0 ? ClientId : null),
                new SqlParameter("@DefaultClient_Id",users.DefaultClient_Id > 0 ? users.DefaultClient_Id : null),
                new SqlParameter("@Is_APIUser",users.Is_APIUser),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateUsers", sqlparams);
            users.User_Id = Convert.ToInt64(sqlparams.Where(x => x.ParameterName == "@User_Id").FirstOrDefault()?.Value);
            if (users.User_Id > 0)
            {
                SqlParameter[] sql =
                {
                    new SqlParameter("@User_Id",users.User_Id),
                    new SqlParameter("@Role_Id",users.Role_Id),
                    new SqlParameter("@ExistingRole_Id",users.ExistingRole_Id)
                };
                SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateUserRoleMapping", sql);
                if (ClientIds?.Count > 0)
                {
                    HandleUserClientAccess(users.User_Id, ClientIds, users.IsSuperAdmin);
                }
            }
            return users;
        }
        public void DeleteUser(long UserId)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter ("@User_Id",UserId)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteUser", sqlparams);
        }
        public List<Users> GetUserClientLevelAccessByUser_Id(long User_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter ("@User_Id",User_Id)
            };
            var _Result = SQLHelper.GetObjectList(new Users(), "UAT_Accounting_GetUserClientLevelAccessByUser_Id", sqlparams);
            return _Result;
        }
        public void InsertUpdateUserClientLevelAccess(UserClientLevelAccess userClientLevelAccess)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter ("@User_Id",userClientLevelAccess.User_Id),
                new SqlParameter("@Client_Id",userClientLevelAccess.Client_Id)
            };
            SQLHelper.ExecuteNonQuery("UAT_Accounting_InsertUpdateUserClientLevelAccess", sqlparams);
        }

        public void InactiveUserClientLevelAccess(UserClientLevelAccess userClientLevelAccess)
        {
            SqlParameter[] sqlparams =
           {
                new SqlParameter ("@User_Id",userClientLevelAccess.User_Id),
                new SqlParameter("@Client_Id",userClientLevelAccess.Client_Id)
            };
            SQLHelper.ExecuteNonQuery("UAT_Accounting_InactiveUserClientLevelAccess", sqlparams);
        }
        #endregion
        private void HandleUserClientAccess(long userId, List<long>? selectedClientIds, bool isSuperAdmin)
        {
            // ---------------- Normalize inputs ----------------
            selectedClientIds ??= new List<long>();

            var existingMappings = GetUserClientLevelAccessByUser_Id(userId);

            // Build HashSets
            var selectedSet = new HashSet<long>(selectedClientIds);
            var existingSet = new HashSet<long>();
            if (existingMappings != null)
            {
                foreach (var map in existingMappings)
                {
                    existingSet.Add(map.Client_Id);
                }
            }
            // ---------------- INSERT NEW ----------------
            if (selectedSet != null)
            {
                foreach (var clientId in selectedSet)
                {
                    if (!existingSet.Contains(clientId))
                    {
                        InsertUpdateUserClientLevelAccess(new UserClientLevelAccess
                        {
                            User_Id = userId,
                            Client_Id = clientId,
                        });
                    }
                }
            }
            // ---------------- DEACTIVATE REMOVED ----------------
            if (selectedSet != null)
            {
                foreach (var clientId in existingSet)
                {
                    if (!selectedSet.Contains(clientId))
                    {
                        InactiveUserClientLevelAccess(new UserClientLevelAccess
                        {
                            User_Id = userId,
                            Client_Id = clientId
                        });
                    }
                }
            }
            // ---------------- SINGLE CLIENT NON-SUPERADMIN ----------------
            if (!isSuperAdmin && selectedSet?.Count == 1)
            {
                foreach (var clientId in existingSet)
                {
                    InactiveUserClientLevelAccess(new UserClientLevelAccess
                    {
                        User_Id = userId,
                        Client_Id = clientId
                    });
                }
            }
        }
        public int UpdatePassword(string Username,string password)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Username",Username),
                new SqlParameter("@Password",password)
            };
            return SQLHelper.ExecuteNonQuery("USP_Accounting_UpdatePassword", sp);
        }
        public void SaveOtpRecord(long userId, string otpHash, string otpType, DateTime expiresAt)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@User_Id", userId),
                new SqlParameter("@OtpHash", otpHash),
                new SqlParameter("@OtpType", otpType),
                new SqlParameter("@ExpiresOn", expiresAt)
            };

            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertOtpRecord", spParams);
        }
        public UserOtpVerification GetValidOtpForUser(long userId)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@User_Id", userId)
            };

            return SQLHelper.GetObject(new UserOtpVerification(), "USP_Accounting_GetValidOtpForUser", spParams);
        }
        public void MarkOtpUsed(long otpId)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@OtpId", otpId)
            };

            SQLHelper.ExecuteNonQuery("USP_Accounting_UpdateOtpUsedStatus", spParams);
        }
        public Users GetUserByEmail(string Email)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Email",Email)
            };
            return SQLHelper.GetObject(new Users(), "USP_Accounting_GetUserByEmail", sp);
        }
    }
}
