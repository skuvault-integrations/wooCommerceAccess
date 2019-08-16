using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;

namespace WooCommerceAccess.Services
{
	public class WCObjectBase
	{
		public static async Task GetVariationsToUpdate( Func< int, Task< IEnumerable< WooCommerceVariation > > > getVariationsAsync, Dictionary< string, int > skusQuantities, int productId, Dictionary< ProductId, IEnumerable< QuantityUpdate > > variationsToUpdate )
		{
			var productVariations = await getVariationsAsync( productId );
			var productVariationsToUpdate = new List< QuantityUpdate >();
			foreach ( var variation in productVariations.Where( v => v.Id != null && !string.IsNullOrWhiteSpace( v.Sku ) ) )
			{
				if ( variation.ManagingStock == null || variation.ManagingStock != true )
					continue;
				var quantityUpdate = skusQuantities.FirstOrDefault( s => s.Key == variation.Sku );
				if ( !string.IsNullOrWhiteSpace( quantityUpdate.Key ) && variation.Quantity.HasValue && quantityUpdate.Value != variation.Quantity )
				{
					productVariationsToUpdate.Add( new QuantityUpdate( variation.Id.Value, variation.Sku, quantityUpdate.Value ) );
				}
			}

			if ( productVariationsToUpdate.Any() )
			{
				variationsToUpdate.Add( new ProductId( productId ), productVariationsToUpdate );
			}
		}
	}
}
