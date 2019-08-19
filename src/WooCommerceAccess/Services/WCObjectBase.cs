using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;

namespace WooCommerceAccess.Services
{
	public class WCObjectBase
	{
		public delegate Task< List< WooCommerceProduct > > GetVariationsAsyncDelegate( Dictionary< string, string > filter );
		
		public static void GetProductToUpdate( Dictionary< string, int > skusQuantities, WooCommerceProduct product, List< QuantityUpdate > productsToUpdate )
		{
			var productToUpdate = new QuantityUpdate( product, skusQuantities );

			if( productToUpdate.IsUpdateNeeded )
			{
				productsToUpdate.Add( productToUpdate );
			}
		}
	}
}
