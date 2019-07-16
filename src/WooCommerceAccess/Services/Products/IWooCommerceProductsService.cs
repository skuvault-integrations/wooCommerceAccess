using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceAccess.Models;

namespace WooCommerceAccess.Services.Products
{
	public interface IWooCommerceProductsService
	{
		/// <summary>
		///	Updates skus quantities asynchronously
		/// </summary>
		/// <param name="skusQuantities">new quantity for each sku</param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task< IEnumerable< WooCommerceProduct > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token );
		
		/// <summary>
		///	Updates sku quantity asynchronously
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		Task< WooCommerceProduct > UpdateSkuQuantityAsync( string sku, int quantity );

		/// <summary>
		/// Returns product
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task< WooCommerceProduct > GetProductBySkuAsync( string sku, CancellationToken token );
	}
}
