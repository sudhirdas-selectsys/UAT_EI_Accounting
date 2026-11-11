using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_QBOSettings
    {

        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string CSRFToken { get; set; }
        public string companyid { get; set; }
        public string minorversion { get; set; }
        public string UserAgent { get; set; }
        public string baseurl { get; set; }
        public string AuthorizationUrl { get; set; }
        public string TokenEndpoint { get; set; }
        public string AuthCallbackUrl { get; set; }
        public string AuthorizationCode { get; set; }
        public string TokenName { get; set; }
        public string access_token_url { get; set; }
        public string grant_type { get; set; }
        public string client_authentication { get; set; }
        public string scope { get; set; }
        public string AccessToken { get; set; }
        public string refresh_token { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PostalCode { get; set; }
        public DateTime? RefreshtokenUpdatedon { get; set; }
    }
}
