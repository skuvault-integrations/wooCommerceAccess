using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;

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

		internal static List< QuantityUpdate > GetProductsInventory( Dictionary< string, WooCommerceProduct > products, Dictionary< string, int > skusQuantities, bool productsOnly = true )
		{
			var inventory = new List< QuantityUpdate >();
			foreach( var sku in products.Keys )
			{
				var product = products[ sku ];
				if( product.ParentId == 0 || !productsOnly )
				{
					inventory.Add( new QuantityUpdate( product, skusQuantities ) );
				}
			}

			return inventory;
		}

		internal static Dictionary< ProductId, List< QuantityUpdate > > GetProductVariationsInventory( Dictionary< string, WooCommerceProduct > products, Dictionary< string, int > skusQuantities )
		{
			var inventory = new Dictionary< ProductId, List< QuantityUpdate > >();
			foreach( var sku in products.Keys )
			{
				var product = products[ sku ];
				if (product.ParentId == 0)
					continue;

				var productId = new ProductId( product.ParentId );

				if( inventory.ContainsKey( productId ) )
				{ 
					inventory[ productId ].Add( new QuantityUpdate( product, skusQuantities ) );
				}
				else 
				{ 
					inventory.Add( productId, new List< QuantityUpdate > 
					{ 
						new QuantityUpdate( product, skusQuantities ) 
					} );
				}
			}

			return inventory;
		}

		internal static async Task< Dictionary< string, WooCommerceProduct > > GetProductsAsync( Func< string, int, string, Mark, Task< WooCommerceProduct > > getProductBySkuFunc,
			Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark )
		{
			var products = new Dictionary< string, WooCommerceProduct >();
			foreach( var sku in skusQuantities.Keys )
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
