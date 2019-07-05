using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess
{
	public interface IWooCommerceFactory
	{
		IWooCommerceProductsService CreateProductsService( WooCommerceConfig config, Throttler throttler );
	}
}
