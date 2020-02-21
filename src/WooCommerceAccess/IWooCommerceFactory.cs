using WooCommerceAccess.Configuration;
using WooCommerceAccess.Services.Orders;
using WooCommerceAccess.Services.Products;
using WooCommerceAccess.Services.SystemStatus;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess
{
	public interface IWooCommerceFactory
	{
		IWooCommerceProductsService CreateProductsService( WooCommerceConfig config, Throttler throttler );
		IWooCommerceOrdersService CreateOrdersService( WooCommerceConfig config, Throttler throttler );
		IWooCommerceSystemStatusService CreateSystemStatusService( WooCommerceConfig config, Throttler throttler );
	}
}
