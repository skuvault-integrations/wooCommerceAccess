using FluentAssertions;
using NUnit.Framework;
using WooCommerceAccess.Shared;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class ApiDetectorTests
	{
		private readonly string _shopUrlWithV3ApiVersion;
		private const string DefaultUserAgentHeader = "SkuVault Inc. WooCommerceAccess library";

		public ApiDetectorTests( string shopCredentialsFileName )
		{
			var shopCredentials = BaseTest.LoadCredentials( @"\..\..\" + shopCredentialsFileName );
			this._shopUrlWithV3ApiVersion = shopCredentials.ShopUrl;
		}

		[ Explicit ]
		[ Test ]
		public void DetectWooCommerceApiVersion()
		{
			var apiDetector = new WooCommerceApiVersionDetector( _shopUrlWithV3ApiVersion, 3, DefaultUserAgentHeader );

			var apiVersion = apiDetector.DetectApiVersion().Result;
			apiVersion.Should().Be( WooCommerceApiVersion.V3 );
		}
	}
}
