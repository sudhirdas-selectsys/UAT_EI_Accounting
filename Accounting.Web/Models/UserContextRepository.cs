using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;

namespace Accounting.Web.Models
{
    public class UserContextRepository : IUserContextRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetPredefinedParameterValue(byte PredefinedParameter_Id)
        {
            if (PredefinedParameter_Id == MasterLookup._PredefinedParameters.Client_Id)
            {
                return GetCurrentClientId().ToString();
            }
            else if (PredefinedParameter_Id == MasterLookup._PredefinedParameters.PageIndex)
            {
                return "0";
            }else if (PredefinedParameter_Id == MasterLookup._PredefinedParameters.PageSize)
            {
                return "20";
            }
            return null;
        }
        public long? GetCurrentClientId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            long? Client_Id = null;
            if (session != null)
            {
                UserSession userSession = session.GetObjectFromJson<UserSession>("UserSession");
                if (userSession != null)
                {
                    Client_Id = userSession.Client_Id;
                }
            }
            return Client_Id;
        }
    }
}
