using WooCommerceAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WooCommerceAccess.Services
{
	public interface IWCObject
	{
		string ProductApiUrl { get; }
		string OrdersApiUrl { get; }

		Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize );
		Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity );
		Task< IEnumerable < WooCommerceProduct > > UpdateSkusQuantityAsync ( Dictionary< string, int > skusQuantities, int pageSize );
		Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc );
		Task< IEnumerable < WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated, int pageSize );
	}
}
