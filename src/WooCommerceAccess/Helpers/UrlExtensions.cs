using System;
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

        /// <summary>
        /// Normalizes a URL by ensuring www. prefix is present when needed
        /// </summary>
        /// <param name="url">The URL to normalize</param>
        /// <returns>The normalized URL</returns>
        internal static string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // Create a URI from the URL
            var uri = new Uri(url);

            // Only add www. if it's not already present
            if (!uri.Host.StartsWith("www."))
            {
                // Create a new URI with www. prefix
                var builder = new UriBuilder(uri)
                {
                    Host = "www." + uri.Host
                };
                return builder.Uri.ToString();
            }

            return url;
        }
    }
}