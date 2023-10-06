using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;

namespace WooCommerceAccess.Services.Products
{
	public interface IWooCommerceProductsService
	{
		/// <summary>
		///	Updates skus quantities asynchronously
		/// </summary>
		/// <param name="skusQuantities">new quantity for each sku</param>
		/// <returns></returns>
		Task< Dictionary< string, int >  > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, Mark mark );

		/// <summary>
		/// Update Inventory by SKUs
		/// Note: This method can be used for regular Inventory Sync without requesting the entire catalog from WooCommerce
		/// </summary>
		/// <param name="skusQuantities"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		Task< Dictionary< string, int > > UpdateInventoryAsync( Dictionary< string, int > skusQuantities, Mark mark );
		
		/// <summary>
		///	Updates sku quantity asynchronously
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		Task< WooCommerceProduct > UpdateSkuQuantityAsync( string sku, int quantity, Mark mark );

		/// <summary>
		/// Returns product
		/// </summary>
		/// <param name="sku"></param>
		/// <returns></returns>
		Task< WooCommerceProduct > GetProductBySkuAsync( string sku, Mark mark );

		/// <summary>
		/// Retrieving a list of products from WcooCommerce (created or updated after a specified date)
		/// </summary>
		/// <param name="startDateUtc"></param>
		/// <returns></returns>
		Task< IEnumerable< WooCommerceProduct > > GetProductsAsync( DateTime startDateUtc, Mark mark );
	}
}
