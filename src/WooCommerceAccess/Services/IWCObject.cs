using WooCommerceAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using WooCommerceAccess.Shared;

namespace WooCommerceAccess.Services
{
	public interface IWCObject
	{
		string ProductApiUrl { get; }
		string OrdersApiUrl { get; }
		string SystemStatusApiUrl { get; }

		Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize, string url, Mark mark );
		Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity, string url, Mark mark );
		Task< Dictionary< string, int > > UpdateSkusQuantityAsync ( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark );
		Task< Dictionary< string, int > > UpdateInventoryAsync( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark );
		Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize, string url, Mark mark );
		Task< IEnumerable< WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated, int pageSize, string url, Mark mark );
		Task< string > GetStoreVersionAsync( string url, Mark mark );
	}
}
