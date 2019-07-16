using CuttingEdge.Conditions;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Orders;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess
{
	public class WooCommerceFactory : IWooCommerceFactory
	{
		public IWooCommerceOrdersService CreateOrdersService(WooCommerceConfig config, Throttler throttler)
		{
			return new WooCommerceOrdersService( config, throttler );
		}

		public IWooCommerceProductsService CreateProductsService( WooCommerceConfig config, Throttler throttler )
		{
			return new WooCommerceProductsService( config, throttler );
		}
	}
}
