using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using WooCommerceAccess;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Orders;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Services.SystemStatus;
using WooCommerceAccess.Shared;
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
		protected IWooCommerceOrdersService OrdersService { get; private set; }
		protected IWooCommerceSystemStatusService SystemStatusService { get; private set; }
		protected IWooCommerceSettingsService SettingsService { get; private set; }

		protected Mark Mark
		{
			get
			{
				return Mark.CreateNew();
			}
		}

		public BaseTest( string shopCredentialsFileName )
		{
			var shopCredentials = LoadCredentials( @"\..\..\" + shopCredentialsFileName );
			this.Config = new WooCommerceConfig( shopCredentials.ShopUrl );
			
			if ( !string.IsNullOrWhiteSpace( shopCredentials.ConsumerKey ) && !string.IsNullOrWhiteSpace( shopCredentials.ConsumerSecret ) )
			{
				var factory = new WooCommerceFactory();
				this.Config = new WooCommerceConfig( shopCredentials.ShopUrl, shopCredentials.ConsumerKey, shopCredentials.ConsumerSecret );
				var throttler = new Throttler( 5, 1, 1 );
				
				this.ProductsService = factory.CreateProductsService( this.Config, throttler );
				this.OrdersService = factory.CreateOrdersService( this.Config, throttler );
				this.SystemStatusService = factory.CreateSystemStatusService( this.Config, throttler );
				this.SettingsService = factory.CreateSettingsService( this.Config, throttler );
			}
		}

		internal static ShopCredentials LoadCredentials( string filePath )
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
