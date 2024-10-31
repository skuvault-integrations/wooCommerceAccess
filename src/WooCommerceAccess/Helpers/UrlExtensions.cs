using System.Collections.Generic;
using System.Text;

namespace WooCommerceAccess.Helpers
{
    internal static class UrlExtensions
    {
        /// <summary>
        /// Build query string from dictionary
        /// </summary>
        /// <param name="requestParameters"></param>
        internal static string BuildQueryString(Dictionary<string, string> requestParameters)
        {
            var paramsBuilder = new StringBuilder();

            foreach (var kv in requestParameters ?? new Dictionary<string, string>())
            {
                if (paramsBuilder.Length > 0)
                {
                    paramsBuilder.Append('&');
                }

                paramsBuilder.Append(kv.Key).Append('=').Append(kv.Value);
            }

            return paramsBuilder.ToString();
        }
    }
}