using Accounting.Utilities.Internal.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Utilities.Internal.Implementations
{
    public class ClientInfoHelper 
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientInfoHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public (string Browser, string DeviceLabel, string UserAgent) GetClientInfo()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return ("Unknown", "Unknown", "No HttpContext");

            string userAgent = context.Request.Headers["User-Agent"];

            if (string.IsNullOrWhiteSpace(userAgent))
                return ("Unknown", "Unknown", "Empty User-Agent");

            string browser = GetBrowserName(userAgent);
            string device = GetDeviceLabel(userAgent);

            return (browser, device, userAgent);
        }

        public string GetBrowserName(string userAgent)
        {
            userAgent = userAgent.ToLower();

            if (userAgent.Contains("edg/"))
                return "Microsoft Edge";
            if (userAgent.Contains("chrome/") && !userAgent.Contains("edg/"))
                return "Google Chrome";
            if (userAgent.Contains("firefox/"))
                return "Mozilla Firefox";
            if (userAgent.Contains("safari/") && !userAgent.Contains("chrome/"))
                return "Apple Safari";
            if (userAgent.Contains("opr/") || userAgent.Contains("opera"))
                return "Opera";
            if (userAgent.Contains("msie") || userAgent.Contains("trident/"))
                return "Internet Explorer";

            return "Unknown Browser";
        }

        public string GetDeviceLabel(string userAgent)
        {
            userAgent = userAgent.ToLower();

            if (userAgent.Contains("mobile"))
                return "Mobile";
            if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                return "Tablet";
            if (userAgent.Contains("windows") || userAgent.Contains("macintosh") || userAgent.Contains("linux"))
                return "Desktop";

            return "Unknown Device";
        }
    }
}
