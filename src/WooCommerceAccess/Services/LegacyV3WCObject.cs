using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceNET;
using WLegacyApi = WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerceAccess.Services
{
	public sealed class LegacyV3WCObject : WCObjectBase, IWCObject
	{
		private readonly WLegacyApi.WCObject _legacyApiWCObject;
		private readonly string _apiUrl;

		public LegacyV3WCObject( RestAPI restApi )
		{
			Condition.Requires( restApi, "restApi" ).IsNotNull();
			this._legacyApiWCObject = new WLegacyApi.WCObject( restApi );
			this._apiUrl = restApi.Url;
		}

		public string ProductApiUrl => this._apiUrl + "products";

		public string OrdersApiUrl => this._apiUrl + "orders";

		public Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize )
		{
			var ordersFilters = new Dictionary< string, string >
			{
				{ "filter[updated_at_min]", startDateUtc.ToString( "o" ) },
				{ "filter[updated_at_max]", endDateUtc.ToString( "o" ) }
			};
			
			return CollectOrdersFromAllPagesAsync( ordersFilters, pageSize );
		}

		private async Task< IEnumerable< WooCommerceOrder > > CollectOrdersFromAllPagesAsync( Dictionary< string, string > ordersFilters, int pageSize )
		{
			var orders = new List< WooCommerceOrder >();

			for (var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateLegacyApiV3GetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = ordersFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value );
				var ordersWithinPage = await this._legacyApiWCObject.GetOrders( combinedFilters ).ConfigureAwait( false );

				if ( !ordersWithinPage.Any() )
					break;

				orders.AddRange( ordersWithinPage.Select( order => order.ToSvOrder() ).ToList() );
			}
			
			return orders;
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "filter[sku]", sku }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters, pageSize );

			return products.
				// WooCommerce API returns any sku that contains requested sku
				FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< IEnumerable< WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated, int pageSize )
		{
			var dateFilter = includeUpdated ? "filter[updated_at_min]" : "filter[created_at_min]";

			var productFilters = new Dictionary< string, string >
			{
				{ dateFilter, productsStartUtc.ToString( "o" ) },
			};

			return await CollectProductsFromAllPagesAsync( productFilters, pageSize );
		}

		private async Task< IEnumerable< WooCommerceProduct > > CollectProductsFromAllPagesAsync( Dictionary< string, string > productFilters, int pageSize )
		{
			var products = new List< WooCommerceProduct >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateLegacyApiV3GetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = productFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var productsWithinPage = await GetNextProductPageAsync( combinedFilters );
				if( !productsWithinPage.Any() )
					break;

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		private async Task< List< WooCommerceProduct > > GetNextProductPageAsync( Dictionary< string, string> filter )
		{
			var productsWithinPage = ( await this._legacyApiWCObject.GetProducts( filter ).ConfigureAwait( false ) )
				.Select( p => p.ToSvProduct() ).ToList();
			return productsWithinPage;
		}

		public async Task< Dictionary< string, int > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, int pageSize, string url, Shared.Mark mark )
		{
			var productsToUpdate = await GetProductsToUpdateAsync( async filter => await GetNextProductPageAsync( filter ), skusQuantities, pageSize );
			var updatedProducts = await UpdateProductsAsync( productsToUpdate );
			return updatedProducts.ToDictionary( p => p.Key, p => p.Value );
		}

		public static async Task< List< QuantityUpdate > > GetProductsToUpdateAsync( GetProductsAsyncDelegate getNextProductPageAsync, Dictionary< string, int > skusQuantities, int pageSize )
		{
			var productsToUpdate = new List< QuantityUpdate >();
			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var productsWithinPage = await getNextProductPageAsync( pageFilter );				
				if( !productsWithinPage.Any() )
					break;
				foreach( var product in productsWithinPage.Where( p => p.Id != null ) ) 
				{
					GetProductToUpdate( skusQuantities, product, productsToUpdate );
				}
			}
			return productsToUpdate;
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity )
		{
			var updateProductRequest = new WLegacyApi.Product() { id = productId, stock_quantity = quantity };
			await this._legacyApiWCObject.UpdateProduct( productId, updateProductRequest );
			return updateProductRequest.ToSvProduct();
		}

		private async Task< Dictionary< string, int > > UpdateProductsAsync( IEnumerable< QuantityUpdate > productsToUpdate )
		{
			var result = new Dictionary< string, int >();
			var updatedProductIds = new List< ProductId >();

			foreach( var productToUpdate in productsToUpdate )
			{
				var updatedProduct = await this.UpdateProductQuantityAsync( productToUpdate.Id, productToUpdate.Quantity ).ConfigureAwait( false );

				if ( updatedProduct == null )
					continue;
				result.Add( productToUpdate.Sku, productToUpdate.Quantity );
				updatedProductIds.Add( new ProductId( productToUpdate.Id ));
			}
			return result;
		}
	}
}