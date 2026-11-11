using Microsoft.AspNetCore.Http;
using System.Net;

namespace Accounting.Utilities.Internal
{
    public class OTPHelper
    {
        public static string GetClientIp(HttpContext context)
        {
            if (context == null) return string.Empty;

            string forwardedFor = context.Request.Headers["X-Forwarded-For"];
            string realIp = context.Request.Headers["X-Real-IP"];
            string cloudflareIp = context.Request.Headers["CF-Connecting-IP"];
            string remoteAddr = context.Connection.RemoteIpAddress?.ToString();

            string[] candidates = {
                forwardedFor?.Split(',')[0]?.Trim(),
                realIp,
                cloudflareIp,
                remoteAddr
            };

            foreach (var ip in candidates)
            {
                if (!string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out var parsed))
                {
                    if (parsed.IsIPv4MappedToIPv6)
                        parsed = parsed.MapToIPv4();

                    return parsed.ToString();
                }
            }

            return string.Empty;
        }

        public static string GetRemoteAddress(HttpRequest req)
        {
            var ip = req.HttpContext.Connection.RemoteIpAddress;

            if (ip == null)
                return string.Empty;

            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            return ip.ToString();
        }
    }
}
