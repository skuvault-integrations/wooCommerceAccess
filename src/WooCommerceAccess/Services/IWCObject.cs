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
		Task< Dictionary< string, int > > UpdateSkusQuantityAsync ( Dictionary< string, int > skusQuantities, int pageSize );
		Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize );
		Task< IEnumerable< WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated, int pageSize );
	}
}
