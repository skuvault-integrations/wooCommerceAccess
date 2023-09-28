using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using static System.Collections.Generic.Dictionary< string, int >;

namespace WooCommerceAccess.Services
{
	public class WCObjectBase
	{
		public delegate Task< List< WooCommerceProduct > > GetProductsAsyncDelegate( Dictionary< string, string > filter );
		
		public static void GetProductToUpdate( Dictionary< string, int > skusQuantities, WooCommerceProduct product, List< QuantityUpdate > productsToUpdate )
		{
			var productToUpdate = new QuantityUpdate( product, skusQuantities );

			if( productToUpdate.IsUpdateNeeded )
			{
				productsToUpdate.Add( productToUpdate );
			}
		}

		internal static List< QuantityUpdate > CreateProductsVariationsInventoryUpdateRequests( Dictionary< string, WooCommerceProduct > productsOrVariations,
			Dictionary< string, int > skusQuantities, bool productsOnly = true )
		{
			var inventory = new List< QuantityUpdate >();
			foreach( var sku in productsOrVariations.Keys )
			{
				var productOrVariation = productsOrVariations[ sku ];
				if( productOrVariation.ParentProductId == 0 || !productsOnly )
				{
					inventory.Add( new QuantityUpdate( productOrVariation, skusQuantities ) );
				}
			}

			return inventory;
		}

		internal static Dictionary< ProductId, List< QuantityUpdate > > CreateVariationsInventoryUpdateRequests( Dictionary< string, WooCommerceProduct > variations,
			Dictionary< string, int > skusQuantities )
		{
			var inventory = new Dictionary< ProductId, List< QuantityUpdate > >();
			foreach( var sku in variations.Keys )
			{
				var variation = variations[ sku ];
				// skip products
				if ( variation.ParentProductId == 0 )
					continue;

				var productId = new ProductId( variation.ParentProductId );

				if( inventory.ContainsKey( productId ) )
				{ 
					inventory[ productId ].Add( new QuantityUpdate( variation, skusQuantities ) );
				}
				else 
				{ 
					inventory.Add( productId, new List< QuantityUpdate > 
					{ 
						new QuantityUpdate( variation, skusQuantities ) 
					} );
				}
			}

			return inventory;
		}

		internal static async Task< Dictionary< string, WooCommerceProduct > > GetProductsAsync( Func< string, int, string, Mark, Task< WooCommerceProduct > > getProductBySkuFunc,
			KeyCollection skus, int pageSize, string url, Mark mark )
		{
			var products = new Dictionary< string, WooCommerceProduct >();
			foreach( var sku in skus )
			{
				var product = await getProductBySkuFunc( sku, pageSize, url, mark ).ConfigureAwait( false );
				if( product == null )
					continue;

				products.Add( sku, product );
			}

			return products;
		}
	}
}
