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
	public sealed class ApiV3WCObject : WCObjectBase, IWCObject
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
				var productsWithinPage = await GetNextProductPageAsync( combinedFilters );
				if( !productsWithinPage.Any() )
					break;

				foreach( var productWithinPage in productsWithinPage )
				{
					if( productWithinPage.HasVariations && productWithinPage.Id.HasValue ) 
					{ 
						productWithinPage.Variations = await CollectVariationsByProductFromAllPagesAsync( productWithinPage.Id.Value, pageSize );
					}
					else
					{
						productWithinPage.Variations = new List< WooCommerceVariation >();
					}
				}

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		public async Task< IEnumerable< WooCommerceVariation > > CollectVariationsByProductFromAllPagesAsync( int productId, int pageSize )
		{
			var variations = new List< WooCommerceVariation >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var wooCommerceVariations = ( await this._wcObjectApiV3.Product.Variations.GetAll( productId, pageFilter ).ConfigureAwait( false ) );
				var variationsWithinPage = wooCommerceVariations.Select( v => v.ToSvVariation() ).ToList();

				if( !variationsWithinPage.Any() )
					break;

				variations.AddRange( variationsWithinPage );
			}

			return variations;
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity )
		{
			var updatedProduct = await this._wcObjectApiV3.Product.Update( productId, new WApiV3.Product { stock_quantity = quantity } );
			return updatedProduct.ToSvProduct();
		}

		public async Task< Dictionary< string, int > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark )
		{
			var productsToUpdate = new List< QuantityUpdate >();
			var variationsToUpdate = new Dictionary< ProductId, IEnumerable< QuantityUpdate > >();
			await GetProductsAndVariationsToUpdateAsync( async filter => await GetNextProductPageAsync( filter ), 
				async productId => await CollectVariationsByProductFromAllPagesAsync( productId, pageSize ),
				skusQuantities, pageSize, productsToUpdate, variationsToUpdate );

			var variationsJson = variationsToUpdate.Select( x => new { ProductId = x.Key.Id, Variations = x.Value } ).ToJson();
			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "productsToUpdate: {0}. variationsToUpdate: {1}" , productsToUpdate.ToJson(), variationsJson ) ) );
			var updatedProducts = await UpdateProductsAsync( productsToUpdate );
			var updatedVariations = ( await UpdateVariationsAsync( variationsToUpdate ) ).ToDictionary( p => p.Sku, p => p.Quantity ?? 0 );
			return updatedProducts.Concat( updatedVariations ).ToDictionary( p => p.Key, p => p.Value );
		}

		public static async Task GetProductsAndVariationsToUpdateAsync( GetProductsAsyncDelegate getNextProductPageAsync, Func< int, Task< IEnumerable< WooCommerceVariation > > > getVariationsAsync, Dictionary< string, int > skusQuantities, int pageSize, List< QuantityUpdate > productsToUpdate, Dictionary< ProductId, IEnumerable< QuantityUpdate > > variationsToUpdate )
		{
			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var productsWithinPage = await getNextProductPageAsync( pageFilter );				
				if( !productsWithinPage.Any() )
					break;
				foreach( var product in productsWithinPage.Where( p => p.Id != null ) ) 
				{
					GetProductToUpdate( skusQuantities, product, productsToUpdate );

					if( product.HasVariations )
					{
						GetVariationsToUpdate( skusQuantities, await getVariationsAsync( product.Id.Value ), product.Id.Value, variationsToUpdate );
					}
				}
			}
		}

		public static void GetVariationsToUpdate( Dictionary< string, int > skusQuantities, IEnumerable< WooCommerceVariation > variations, int productId, Dictionary< ProductId, IEnumerable< QuantityUpdate > > variationsToUpdate )
		{
			var productVariationsToUpdate = variations.Select( variation => new QuantityUpdate( variation, skusQuantities ) ).
				Where( quantityUpdate => quantityUpdate.IsUpdateNeeded ).ToList();

			if ( productVariationsToUpdate.Any() )
			{
				variationsToUpdate.Add( new ProductId( productId ), productVariationsToUpdate );
			}
		}

		private async Task< List< WooCommerceProduct > > GetNextProductPageAsync( Dictionary< string, string> filter )
		{
			var productsWithinPage = ( await this._wcObjectApiV3.Product.GetAll( filter ).ConfigureAwait( false ) )
				.Select( p => p.ToSvProduct() ).ToList();
			return productsWithinPage;
		}

		private async Task< Dictionary< string, int > > UpdateProductsAsync( IEnumerable< QuantityUpdate > productsToUpdate )
		{
			var productsUpdateRequests = productsToUpdate.Select( productQuantity => new WApiV3.Product
			{
				id = productQuantity.Id, sku = productQuantity.Sku, stock_quantity = productQuantity.Quantity
			} ).ToList();

			return ( await UpdateProductsInSequentialBatchesAsync( productsUpdateRequests, BatchSize ) ).Where( p => !string.IsNullOrWhiteSpace( p.Sku ) ).ToDictionary( p => p.Sku, p => p.Quantity ?? 0 );
		}

		private async Task< IEnumerable< WooCommerceVariation > > UpdateVariationsAsync( Dictionary< ProductId, IEnumerable< QuantityUpdate > > variationsUpdateRequests )
		{
			var result = new List< WooCommerceVariation >();
			var wooCommerceVariationBatch = new WooCommerceNET.Base.BatchObject< WApiV3.Variation >();
			foreach ( var variationsUpdateRequest in variationsUpdateRequests )
			{
				wooCommerceVariationBatch.update = variationsUpdateRequest.Value.Select( v => 
					new WApiV3.Variation
					{
						id = v.Id, sku = v.Sku, stock_quantity = v.Quantity
					} ).ToList();
				var batchResult = await this._wcObjectApiV3.Product.Variations.UpdateRange( variationsUpdateRequest.Key.Id, wooCommerceVariationBatch );
				result.AddRange( batchResult.update.Select( prV3 => prV3.ToSvVariation() ) );
			}

			return result;
		}

		private async Task< IEnumerable< WooCommerceProduct > > UpdateProductsInSequentialBatchesAsync( IEnumerable< WApiV3.Product > productsUpdateRequests, int batchSize )
		{
			var result = new List< WooCommerceProduct >();
			var wooCommerceProductBatch = new WApiV3.ProductBatch();

			foreach( var batch in new BatchList< WApiV3.Product >( productsUpdateRequests, batchSize ) )
			{
				var productsUpdateRequestBatch = batch.ToList();
				if( !productsUpdateRequestBatch.Any() )
					continue;
				wooCommerceProductBatch.update = productsUpdateRequestBatch;
				var batchResult = await this._wcObjectApiV3.Product.UpdateRange( wooCommerceProductBatch );
				result.AddRange( batchResult.update.Select( prV3 => prV3.ToSvProduct() ) );
			}

			return result;
		}
	}
}