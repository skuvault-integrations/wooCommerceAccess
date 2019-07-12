using WooCommerceAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WooCommerceAccess.Services
{
	public interface IWCObject
	{
		string ProductApiUrl { get; }

		Task< Product > GetProductBySkuAsync( string sku );
		Task< Product > UpdateProductQuantityAsync( int productId, int quantity );
		Task< Product[] > UpdateSkusQuantityAsync ( Dictionary< string, int > skusQuantities );
	}
}
