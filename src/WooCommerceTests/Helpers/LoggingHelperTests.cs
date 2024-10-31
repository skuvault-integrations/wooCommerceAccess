using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WooCommerceAccess.Helpers;

namespace WooCommerceTests.Helpers
{
    public class LoggingHelperTests
    {
        private static readonly Randomizer _randomizer = new Randomizer();
        
        [Test]
        public void BuildQueryString_FiltersSensitiveData()
        {
            var queryStringParams = new Dictionary<string, string>
            {
                { "oauth_something", _randomizer.GetString() },
                { "consumer_key", _randomizer.GetString() },
                { "consumer_secret", _randomizer.GetString() },
                { "not_sensitive_field", _randomizer.GetString() },
            };
            
            var result = LoggingHelper.BuildQueryString(queryStringParams);
            
            Assert.That(result,
                Is.EqualTo(
                    $"oauth_something=***&consumer_key=***&consumer_secret=***&not_sensitive_field={queryStringParams["not_sensitive_field"]}"));
        }
    }
}