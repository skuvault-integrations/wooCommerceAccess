using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using WooCommerceAccess;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Throttling;

namespace WooCommerceTests
{
	public class ShopCredentials
	{
		public string ShopUrl { get; set; }
		public string ConsumerKey { get; set; }
		public string ConsumerSecret { get; set; }
	}

	public class BaseTest
	{
		protected WooCommerceConfig Config { get; private set; }
		protected IWooCommerceProductsService ProductsService { get; private set; }

		public BaseTest( string shopCredentialsFileName )
		{
			var shopCredentials = this.LoadCredentials( @"\..\..\" + shopCredentialsFileName );
			this.Config = new WooCommerceConfig( shopCredentials.ShopUrl );
			
			if ( !string.IsNullOrWhiteSpace( shopCredentials.ConsumerKey ) && !string.IsNullOrWhiteSpace( shopCredentials.ConsumerSecret ) )
			{
				var factory = new WooCommerceFactory( shopCredentials.ConsumerKey, shopCredentials.ConsumerSecret );
				var throttler = new Throttler( 5, 1, 1 );
				
				this.ProductsService = factory.CreateProductsService( this.Config, throttler );
			}
		}

		private ShopCredentials LoadCredentials( string filePath )
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + filePath ) )
			{
				return new ShopCredentials()
				{
					ShopUrl = reader.ReadLine(),
					ConsumerKey = reader.ReadLine(),
					ConsumerSecret = reader.ReadLine()
				};
			}
		}
	}
}
