using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Accounting.Utilities.Internal
{
    public class WebAppReferenceUtility
    {
        public static IMemoryCache _memoryCache { get; set; }
        public static IConfiguration _configuration { get; set; }
        public static string ProjectRootFolderPath { get; set; }
    }
}
