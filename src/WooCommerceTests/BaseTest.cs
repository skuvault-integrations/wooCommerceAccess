using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using WooCommerceAccess;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Throttling;

namespace WooCommerceTests
{
	public class TestCredentials
	{
		public string ShopUrl { get; set; }
		public string ConsumerKey { get; set; }
		public string ConsumerSecret { get; set; }
	}

	public class BaseTest
	{
		protected WooCommerceConfig Config { get; private set; }
		protected IWooCommerceProductsService ProductsService { get; private set; }

		[ SetUp ]
		public void InitBase()
		{
			var credentials = LoadCredentials();
			this.Config = new WooCommerceConfig( credentials.ShopUrl );
			
			if ( !string.IsNullOrWhiteSpace( credentials.ConsumerKey ) && !string.IsNullOrWhiteSpace( credentials.ConsumerSecret ) )
			{
				var factory = new WooCommerceFactory( credentials.ConsumerKey, credentials.ConsumerSecret );
				var throttler = new Throttler( 5, 1, 1 );
				
				this.ProductsService = factory.CreateProductsService( this.Config, throttler );
			}
		}

		protected TestCredentials LoadCredentials()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\credentials.csv" ) )
			{
				return new TestCredentials()
				{
					ShopUrl = reader.ReadLine(),
					ConsumerKey = reader.ReadLine(),
					ConsumerSecret = reader.ReadLine()
				};
			}
		}
	}
}
