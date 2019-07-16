using CuttingEdge.Conditions;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess
{
	public class WooCommerceFactory : IWooCommerceFactory
	{
		public IWooCommerceProductsService CreateProductsService( WooCommerceConfig config, Throttler throttler )
		{
			return new WooCommerceProductsService( config, throttler );
		}
	}
}
