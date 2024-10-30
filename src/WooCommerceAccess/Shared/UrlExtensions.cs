using System.Collections.Generic;
using System.Text;
using System.Web;

namespace WooCommerceAccess.Shared
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Build query string from dictionary
        /// </summary>
        /// <param name="requestParameters"></param>
        internal static string BuildQueryString(Dictionary<string, string> requestParameters)
        {
            var paramsBuilder = new StringBuilder();

            foreach (var kv in requestParameters)
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