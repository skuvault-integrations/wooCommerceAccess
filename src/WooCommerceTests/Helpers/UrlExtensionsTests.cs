using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WooCommerceAccess.Helpers;

namespace WooCommerceTests.Helpers
{
    public class UrlExtensionsTests
    {
        private static readonly Randomizer _randomizer = new Randomizer();
        
        [Test]
        public void BuildQueryString_BuildsQueryString()
        {
            var queryStringParams = new Dictionary<string, string>
            {
                { "param1", _randomizer.GetString() },
                { "param2", _randomizer.GetString() },
            };
            
            var result = UrlExtensions.BuildQueryString(queryStringParams);
            
            Assert.That(result, Is.EqualTo($"param1={queryStringParams["param1"]}&param2={queryStringParams["param2"]}"));
        }
        
        [Test]
        public void BuildQueryString_ReturnsEmpty_WhenNoParams()
        {
            var queryStringParams = new Dictionary<string, string>();
            
            var result = UrlExtensions.BuildQueryString(queryStringParams);
            
            Assert.That(result, Is.Empty);
        }
        
        [Test]
        public void BuildQueryString_ReturnsEmpty_WhenNullParams()
        {
            var result = UrlExtensions.BuildQueryString(null);
            
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void NormalizeUrl_AddsWwwPrefix()
        {
            var url = "https://example.com/path";

            var result = UrlExtensions.NormalizeUrl(url);

            Assert.That(result, Is.EqualTo("https://www.example.com/path"));
        }

        [Test]
        public void NormalizeUrl_PreservesExistingWww()
        {
            var url = "https://www.example.com/path";

            var result = UrlExtensions.NormalizeUrl(url);

            Assert.That(result, Is.EqualTo(url));
        }
    }
}