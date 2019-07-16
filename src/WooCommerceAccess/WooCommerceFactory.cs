using CuttingEdge.Conditions;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Orders;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess
{
	public class WooCommerceFactory : IWooCommerceFactory
	{
		private readonly string _consumerKey;
		private readonly string _consumerSecret;

		public WooCommerceFactory( string consumerKey, string consumerSecret )
		{
			Condition.Requires( consumerKey, "consumerKey" ).IsNotNullOrWhiteSpace();
			Condition.Requires( consumerSecret, "consumerSecret" ).IsNotNullOrWhiteSpace();

			this._consumerKey = consumerKey;
			this._consumerSecret = consumerSecret;
		}

		public IWooCommerceOrdersService CreateOrdersService(WooCommerceConfig config, Throttler throttler)
		{
			return new WooCommerceOrdersService( this._consumerKey, this._consumerSecret, config, throttler );
		}

		public IWooCommerceProductsService CreateProductsService( WooCommerceConfig config, Throttler throttler )
		{
			return new WooCommerceProductsService( this._consumerKey, this._consumerSecret, config, throttler );
		}
	}
}
