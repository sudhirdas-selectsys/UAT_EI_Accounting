using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Accounting.Utilities.Internal
{
    public static class TrustedDeviceHelper
    {
        private static string GetCookieNameFromUserId(long userId, string cookieNameSubstring)
        {
            using (var sha = SHA256.Create())
            {

                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(userId.ToString()));
                string shortHash = BitConverter.ToString(hash, 0, 8).Replace("-", ""); // first 8 bytes
                return cookieNameSubstring + shortHash;
            }
        }

        public static void SetTrustedCookie(
            long userId,
            Guid deviceId,
            byte[] rawToken,
            DateTime ExpiresAt, HttpContext _httpContextAccessor, string cookieNameSubstring
        )
        {
            try
            {
                var context = _httpContextAccessor;
                if (context == null)
                    throw new InvalidOperationException("No active HTTP context available.");

                var name = GetCookieNameFromUserId(userId, cookieNameSubstring);
                var cookieValue = $"{deviceId}|{Convert.ToBase64String(rawToken)}";

                var options = new CookieOptions
                {
                    Path = "/",
                    Expires = ExpiresAt,
                    HttpOnly = true,
                    Secure = true,// context.Request.IsHttps || context.Request.Host.Host.Contains("localhost"),
                    //SameSite = SameSiteMode.Lax,
                    SameSite = SameSiteMode.None
                };


                context.Response.Cookies.Append(name, cookieValue, options);
            }
            catch (Exception ex) { }
        }

        //public bool TryGetTrustedCookie(int userId, out Guid deviceId, out string tokenB64)
        //{
        //    var context = _httpContextAccessor.HttpContext;
        //    deviceId = Guid.Empty;
        //    tokenB64 = null;

        //    if (context == null)
        //        return false;

        //    var name = GetCookieNameFromUserId(userId);
        //    if (!context.Request.Cookies.TryGetValue(name, out var cookieValue))
        //        return false;

        //    var parts = cookieValue.Split('|');
        //    if (parts.Length != 2)
        //        return false;

        //    if (!Guid.TryParse(parts[0], out deviceId))
        //        return false;

        //    tokenB64 = parts[1];
        //    return !string.IsNullOrEmpty(tokenB64);
        //}

        public static byte[] GenerateRandomToken(int bytes = 32)
        {
            var buf = new byte[bytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buf);
            }
            return buf;
        }

        public static byte[] ComputeTokenHash(byte[] rawToken, string trustedDeviceSecretValue)
        {
            var secretBytes = Encoding.UTF8.GetBytes(trustedDeviceSecretValue);
            var payload = new byte[secretBytes.Length + rawToken.Length];
            Buffer.BlockCopy(secretBytes, 0, payload, 0, secretBytes.Length);
            Buffer.BlockCopy(rawToken, 0, payload, secretBytes.Length, rawToken.Length);

            using (var sha = SHA256.Create())
                return sha.ComputeHash(payload);
        }

        public static bool TryGetTrustedCookie(
    HttpContext context,
    long userId, string cookieNameSubstring,
    out Guid deviceId,
    out string tokenB64)
        {
            string name = GetCookieNameFromUserId(userId, cookieNameSubstring);
            deviceId = Guid.Empty;
            tokenB64 = null;

            if (context?.Request?.Cookies == null)
                return false;


            // Try to read the cookie
            if (!context.Request.Cookies.TryGetValue(name, out var cookieValue) || string.IsNullOrEmpty(cookieValue))
                return false;

            cookieValue = Uri.UnescapeDataString(cookieValue);

            // Expected format: "{deviceId}|{base64Token}"
            var parts = cookieValue.Split('|');
            if (parts.Length != 2)
                return false;

            if (!Guid.TryParse(parts[0], out deviceId))
                return false;

            tokenB64 = parts[1];
            return !string.IsNullOrEmpty(tokenB64);
        }

    }
}
