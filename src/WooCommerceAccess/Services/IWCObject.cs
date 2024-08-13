using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services
{
	public interface IWCObject
	{
		string ProductApiUrl { get; }
		string SystemStatusApiUrl { get; }
		string SettingsApiUrl { get; }
		
		/// <summary>
		/// Object for all v3 API entities from the WooCommerce.NET Api Client package.
		/// </summary>
		WApiV3.WCObject WooCommerceNetObjectV3 { get; }

		Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize, string url, Mark mark );
		Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity, string url, Mark mark );
		Task< Dictionary< string, int > > UpdateSkusQuantityAsync ( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark );
		Task< Dictionary< string, int > > UpdateInventoryAsync( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark );
		/// <summary>
		/// Retrieving a list of products from WooCommerce (created or updated after a specified date)
		/// </summary>
		/// <param name="startDateUtc">Date since products were created or updated</param>
		/// <param name="includeUpdated">Indicates the need to also request updated products (otherwise only created ones) from the specified date</param>
		/// <param name="pageSize"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		Task< IEnumerable< WooCommerceProduct > > GetProductsAsync( DateTime startDateUtc, bool includeUpdated, int pageSize, string url, Mark mark );
		Task< string > GetStoreVersionAsync( string url, Mark mark );
		Task< WooCommerceSettings > GetSettingsAsync( string url, Mark mark );
	}
}
