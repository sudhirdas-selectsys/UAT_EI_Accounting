using Newtonsoft.Json;

namespace Accounting.Entities.External.QBO
{
    public class TokenResponse
    {
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("x_refresh_token_expires_in")]
        public int XRefreshTokenExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
