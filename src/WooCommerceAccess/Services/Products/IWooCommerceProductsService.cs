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
		/// Returns products created after a given date and products created before but updated after the given date
		/// </summary>
		/// <param name="productsStartUtc"></param>
		/// <returns></returns>
		Task< IEnumerable< WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated, Mark mark );
	}
}
