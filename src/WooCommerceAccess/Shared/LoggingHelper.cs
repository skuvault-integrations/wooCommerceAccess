using System.Collections.Generic;
using System.Linq;

namespace WooCommerceAccess.Shared
{
    public static class LoggingHelper
    {
        private const string Mask = "***";
        private static readonly string[] CredentialsFields = { "consumer_key", "consumer_secret" };
        
        /// <summary>
        /// Build query string, with sensitive data masked
        /// </summary>
        /// <param name="queryStringParams"></param>
        /// <returns></returns>
        internal static string BuildQueryString(Dictionary<string, string> queryStringParams)
        {
            return UrlExtensions.BuildQueryString(
                queryStringParams?
                    .Select( x => new KeyValuePair<string, string>( x.Key, MaskSensitiveData(x.Key, x.Value) ) )
                    .ToDictionary( x => x.Key, x => x.Value )
                ?? new Dictionary<string, string>());
        }
        
        /// <summary>
        /// Mask sensitive data, for logs
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string MaskSensitiveData(string key, string value)
        {
            if (key.StartsWith("oauth") || CredentialsFields.Contains(key))
            {
                return Mask;
            }

            return value;
        }
    }
}