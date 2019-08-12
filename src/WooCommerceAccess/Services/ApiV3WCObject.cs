using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Exceptions;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceAccess.Shared;
using WooCommerceNET;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services
{
	public sealed class ApiV3WCObject : IWCObject
	{
		private readonly WApiV3.WCObject _wcObjectApiV3;
		private readonly IWCObject _fallbackAPI;
		private const int BatchSize = 100;

		public ApiV3WCObject( RestAPI restApi, IWCObject fallbackApi = null )
		{
			Condition.Requires( restApi, "restApi" ).IsNotNull();
			this._wcObjectApiV3 = new WApiV3.WCObject( restApi );
			this._fallbackAPI = fallbackApi;
		}

		public string ProductApiUrl => this._wcObjectApiV3.Product.API.Url + this._wcObjectApiV3.Product.APIEndpoint;

		public string OrdersApiUrl => this._wcObjectApiV3.Order.API.Url + this._wcObjectApiV3.Order.APIEndpoint;

		public Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize )
		{
			if ( this._fallbackAPI != null )
			{
				return this._fallbackAPI.GetOrdersAsync( startDateUtc, endDateUtc, pageSize );
			}
			
			throw new WooCommerceException( "ApiV3 orders endpoint can't filter records by update date! Use legacy api instead!" );
		}

		private async Task< IEnumerable< WooCommerceOrder > > CollectOrdersFromAllPagesAsync( Dictionary< string, string > ordersFilters, int pageSize )
		{
			var orders = new List< WooCommerceOrder >();

			for (var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = ordersFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value );
				var ordersWithinPage = ( await this._wcObjectApiV3.Order.GetAll( combinedFilters ).ConfigureAwait( false ) )
									.Select ( o => o.ToSvOrder() ).ToList();

				if ( !ordersWithinPage.Any() )
					break;

				orders.AddRange( ordersWithinPage );
			}
			
			return orders;
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "sku", sku }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters, pageSize );
			return products.
				// WooCommerce API returns any sku that contains requested sku
				FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< IEnumerable< WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated, int pageSize )
		{
			if ( _fallbackAPI != null )
			{
				return await _fallbackAPI.GetProductsCreatedUpdatedAfterAsync( productsStartUtc, includeUpdated, pageSize ).ConfigureAwait( true );
			}
			
			throw new WooCommerceException( "ApiV3 products endpoint can't filter records by update date! Use legacy api instead!" );
		}
		
		private async Task< List< WooCommerceProduct > > CollectProductsFromAllPagesAsync( Dictionary< string, string > productFilters, int pageSize )
		{
			var products = new List< WooCommerceProduct >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = productFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var productsWithinPage = ( await this._wcObjectApiV3.Product.GetAll( combinedFilters ).ConfigureAwait( false ) ).
					Select( p => p.ToSvProduct() ).ToList();
				if( !productsWithinPage.Any() )
					break;

				foreach( var productWithinPage in productsWithinPage )
				{
					if( productWithinPage.HasVariations && productWithinPage.Id.HasValue ) 
					{ 
						productWithinPage.Variations = await CollectProductVariationsFromAllPagesAsync( productWithinPage.Id.Value, pageSize );
					}
					else
					{
						productWithinPage.Variations = new List<WooCommerceVariation>();
					}
				}

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		public async Task< IEnumerable< WooCommerceVariation > > CollectProductVariationsFromAllPagesAsync( int productId, int pageSize )
		{
			var variations = new List< WooCommerceVariation >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var variationsWithinPage = ( await this._wcObjectApiV3.Product.Variations.GetAll( productId, pageFilter ).ConfigureAwait( false ) ).
					Select( v => v.ToSvVariation() ).ToList();

				if( !variationsWithinPage.Any() )
					break;

				variations.AddRange( variationsWithinPage );
			}

			return variations;
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync(int productId, int quantity)
		{
			var updatedProduct = await this._wcObjectApiV3.Product.Update( productId, new WApiV3.Product() { stock_quantity = quantity });
			return updatedProduct.ToSvProduct();
		}

		public async Task< IEnumerable< WooCommerceProduct > > UpdateSkusQuantityAsync(
			Dictionary< string, int > skusQuantities, int pageSize )
		{
			var productsUpdateRequests = new List< WApiV3.Product >();

			foreach( var skuQuantity in skusQuantities )
			{
				var product = await this.GetProductBySkuAsync( skuQuantity.Key, pageSize ).ConfigureAwait( false );

				if ( product != null && product.ManagingStock != null && product.ManagingStock.Value )
					productsUpdateRequests.Add( new WApiV3.Product() { id = product.Id, sku = skuQuantity.Key, stock_quantity = skuQuantity.Value } );
			}

			return await DoInSequentialBatchesAsync( productsUpdateRequests, BatchSize );
		}

		private async Task< IEnumerable< WooCommerceProduct > > DoInSequentialBatchesAsync( IEnumerable< WApiV3.Product > productsUpdateRequests, int batchSize )
		{
			var result = new List< WooCommerceProduct >();
			var wooCommerceProductBatch = new WApiV3.ProductBatch();

			foreach( var productsUpdateRequestBatch in new BatchList< WApiV3.Product >( productsUpdateRequests, batchSize ) )
			{
				wooCommerceProductBatch.update = productsUpdateRequestBatch.ToList();
				var batchResult = await this._wcObjectApiV3.Product.UpdateRange( wooCommerceProductBatch );
				result.AddRange( batchResult.update.Select( prV3 => prV3.ToSvProduct() ) );
			}

			return result;
		}
	}
}