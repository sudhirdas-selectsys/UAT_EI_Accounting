using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Utilities
{
    public static class JwtValidator
    {
        public static ClaimsPrincipal ValidateToken(string token, string audience, string issuer)
        {
            // 1️ Load the public key
            var rsa = PemUtils.CreateRsaFromPublicKeyFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "public_key.pem"));

            var securityKey = new RsaSecurityKey(rsa);

            // 2️ Validation parameters
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            // 3️ Validate
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

                // Optional extra: ensure RSA-SHA256 was used
                if (!(validatedToken is JwtSecurityToken jwt) ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token algorithm.");
                }

                // Token valid — return principal
                return principal;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Convenience method to read username (Name)
        public static string GetUsername(string token, string audience, string issuer)
        {
            var principal = ValidateToken(token, audience, issuer);
            return principal?.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        }
        public static string GetUsername(ClaimsPrincipal principal)
        {
            return principal?.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        }
    }
}
