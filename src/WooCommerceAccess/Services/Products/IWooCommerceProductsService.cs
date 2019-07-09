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
		Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token );

		/// <summary>
		/// Returns product
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task< Product > GetProductBySku( string sku, CancellationToken token );
	}
}
