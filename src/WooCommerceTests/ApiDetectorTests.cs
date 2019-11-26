using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using WooCommerceAccess.Shared;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class ApiDetectorTests
	{
		private string _shopUrlWithV3ApiVersion;
		private string _shopUrlWithLegacyApiVersion;
		private const string DefaultUserAgentHeader = "SkuVault Inc. WooCommerceAccess library";

		[ SetUp ]
		public void LoadShopList()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\shops.csv" ) )
			{
				_shopUrlWithV3ApiVersion = reader.ReadLine();
				_shopUrlWithLegacyApiVersion = reader.ReadLine();
			}
		}

		[ Test ]
		public void DetectWooCommerceApiVersion()
		{
			var apiDetector = new WooCommerceApiVersionDetector( _shopUrlWithV3ApiVersion, 3, DefaultUserAgentHeader );

			var apiVersion = apiDetector.DetectApiVersion().Result;
			apiVersion.Should().Be( WooCommerceApiVersion.V3 );
		}

		[ Test ]
		public void DetectWooCommerceLegacyApiVersion()
		{
			var apiDetector = new WooCommerceApiVersionDetector( _shopUrlWithLegacyApiVersion, 3, DefaultUserAgentHeader );

			var apiVersion = apiDetector.DetectApiVersion().Result;
			apiVersion.Should().Be( WooCommerceApiVersion.Legacy );
		}
	}
}
