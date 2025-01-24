using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceAccess.Shared;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3.Extension;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services
{
	/// <inheritdoc />
	public sealed class ApiV3WCObject : WCObjectBase, IWCObject
	{
		public WApiV3.WCObject WooCommerceNetObjectV3 { get; private set; }
		private const int BatchSize = 100;

		public ApiV3WCObject( RestAPI restApi )
		{
			Condition.Requires( restApi, "restApi" ).IsNotNull();
			this.WooCommerceNetObjectV3 = new WApiV3.WCObject( restApi );
		}

		public string ProductApiUrl => this.WooCommerceNetObjectV3.Product.API.Url + this.WooCommerceNetObjectV3.Product.APIEndpoint;

		public string SystemStatusApiUrl => this.WooCommerceNetObjectV3.SystemStatus.API.Url + this.WooCommerceNetObjectV3.SystemStatus.APIEndpoint;

		public string SettingsApiUrl => this.WooCommerceNetObjectV3.Setting.API.Url + this.WooCommerceNetObjectV3.Setting.APIEndpoint;

		public async Task< string > GetStoreVersionAsync( string url, Mark mark )
		{
			var storeInfo = await this.WooCommerceNetObjectV3.SystemStatus.Get().ConfigureAwait( false );

			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Store Info: {0}" , storeInfo.ToJson() ) ) );

			return storeInfo.environment?.version;
		}

		public async Task< WooCommerceSettings > GetSettingsAsync( string url, Mark mark )
		{
			// get the Weight_unit setting from the SettingsApi but the Currency from the SystemStatusApi
			var storeInfo = await this.WooCommerceNetObjectV3.SystemStatus.Get().ConfigureAwait( false );
			var weightUnitSetting = await this.WooCommerceNetObjectV3.Setting.GetSettingOption( "products", "woocommerce_weight_unit" ).ConfigureAwait( false );
			var settings = new WooCommerceSettings
			{
				Currency = storeInfo?.settings?.currency,
				WeightUnit = weightUnitSetting?.value?.ToString(),
			};

			return settings;
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize, string url, Mark mark )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "sku", sku }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters, pageSize, url, mark );
			return products.
				// WooCommerce API returns any sku that contains requested sku
				FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< IEnumerable< WooCommerceProduct > > GetProductsAsync( DateTime startDateUtc, bool includeUpdated, int pageSize, string url, Mark mark )
		{
			var dateFilter = includeUpdated ? "modified_after" : "after";
			var productFilters = new Dictionary< string, string >
			{
				//TODO PBL-9291 In this ticket's v1 PR, will now always send startDateTime as UTC. When we did this earlier for orders, it cased an issue for at least one tenant (PBL-9276).
				//	Test if the v1 change didn't now introduce the same issue for Products (possibly on the PBL-9276 tenant).
				//	If yes, then implement for products the same fix we did for Orders - https://github.com/skuvault-integrations/wooCommerceAccess/pull/66
				//	Below, we'd the date to "sortable" format (no Z) and then add "dates_are_gmt" if UTC. 
				//		Might be good to extract both fixes into common extensions methods - ToDateTimeStartFilterValue (format as “s” and round down to nearest minute) and ToDateTimeEndFilterValue (format as “s” and round up to nearest minute)
				//		{ dateFilter, startDate.RoundDateDownToTopOfMinute().ToString( "s" ) },
				{ dateFilter, startDateUtc.ToString( "o" ) }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters, pageSize, url, mark );
			return products;
		}

		public async Task< IEnumerable< WooCommerceVariation > > CollectVariationsByProductFromAllPagesAsync( int productId, int pageSize, string url, Mark mark )
		{
			var variations = new List< WooCommerceVariation >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );

				var wooCommerceVariations = ( await this.WooCommerceNetObjectV3.Product.Variations.GetAll( productId, pageFilter ).ConfigureAwait( false ) );

				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Variations Received: {0}" , wooCommerceVariations.ToJson() ) ) );

				var variationsWithinPage = wooCommerceVariations.Select( v => v.ToSvVariation() ).ToList();

				if( !variationsWithinPage.Any() )
					break;

				variations.AddRange( variationsWithinPage );
			}

			return variations;
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity, string url, Mark mark )
		{
			var updatedProduct = await this.WooCommerceNetObjectV3.Product.Update( productId, new WApiV3.Product { stock_quantity = quantity } );

			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Product Updated: {0}" , updatedProduct.ToJson() ) ) );

			return updatedProduct.ToSvProduct();
		}

		/// <summary>
		/// Full Inventory Sync by SKUs to WooCommerce
		/// Note: This method can be used for rare Full Inventory Sync by requesting the entire catalog from WooCommerce
		/// </summary>
		/// <param name="skusQuantities"></param>
		/// <param name="pageSize"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		public async Task< Dictionary< string, int > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark )
		{
			var productsToUpdate = new List< QuantityUpdate >();
			var variationsToUpdate = new Dictionary< ProductId, List< QuantityUpdate > >();
			await GetProductsAndVariationsToUpdateAsync( async filter => await GetNextProductPageAsync( filter, url, mark ), 
				async productId => await CollectVariationsByProductFromAllPagesAsync( productId, pageSize, url, mark ),
				skusQuantities, pageSize, productsToUpdate, variationsToUpdate );

			var variationsJson = variationsToUpdate.Select( x => new { ProductId = x.Key.Id, Variations = x.Value } ).ToJson();

			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "productsToUpdate: {0}. variationsToUpdate: {1}" , productsToUpdate.ToJson(), variationsJson ) ) );

			var updatedProducts = await UpdateProductsAsync( productsToUpdate, url, mark );
			var updatedVariations = ( await UpdateVariationsAsync( variationsToUpdate, url, mark ) ).ToDictionary( p => p.Sku, p => p.Quantity ?? 0 );
			return updatedProducts.Concat( updatedVariations ).ToDictionary( p => p.Key, p => p.Value );
		}

		/// <summary>
		/// Inventory Sync by SKUs to WooCommerce
		/// Note: This method can be used for regular Inventory Sync without requesting the entire catalog from WooCommerce
		/// </summary>
		/// <param name="skusQuantities"></param>
		/// <param name="pageSize"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		public async Task< Dictionary< string, int > > UpdateInventoryAsync( Dictionary< string, int > skusQuantities, int pageSize, string url, Mark mark )
		{
			var products = await GetProductsAsync( GetProductBySkuAsync, skusQuantities.Keys, pageSize, url, mark ).ConfigureAwait( false );
			// get inventory update requests separately for products and variations
			var productsToUpdate = CreateProductsVariationsInventoryUpdateRequests( products, skusQuantities, productsOnly: true );
			var variationsToUpdate = CreateVariationsInventoryUpdateRequests( products, skusQuantities );

			LogInventoryToUpdate( productsToUpdate, variationsToUpdate, url, mark );

			var updatedProducts = await UpdateProductsAsync( productsToUpdate, url, mark );
			var updatedVariations = ( await UpdateVariationsAsync( variationsToUpdate, url, mark ) ).ToDictionary( p => p.Sku, p => p.Quantity ?? 0 );
			return updatedProducts.Concat( updatedVariations ).ToDictionary( p => p.Key, p => p.Value );
		}
	
		public static async Task GetProductsAndVariationsToUpdateAsync( GetProductsAsyncDelegate getNextProductPageAsync, Func< int, Task< IEnumerable< WooCommerceVariation > > > getVariationsAsync, 
			Dictionary< string, int > skusQuantities, int pageSize, List< QuantityUpdate > productsToUpdate, Dictionary< ProductId, List< QuantityUpdate > > variationsToUpdate )
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

		public static void GetVariationsToUpdate( Dictionary< string, int > skusQuantities, IEnumerable< WooCommerceVariation > variations, int productId, Dictionary< ProductId,
			List< QuantityUpdate > > variationsToUpdate )
		{
			var productVariationsToUpdate = variations.Select( variation => new QuantityUpdate( variation, skusQuantities ) ).
				Where( quantityUpdate => quantityUpdate.IsUpdateNeeded ).ToList();

			if ( productVariationsToUpdate.Any() )
			{
				var productIdKey = new ProductId( productId );
				if ( variationsToUpdate.ContainsKey( productIdKey ) )
				{
					variationsToUpdate[ productIdKey ] = productVariationsToUpdate;
				}
				else
				{
					variationsToUpdate.Add( productIdKey, productVariationsToUpdate );
				}
			}
		}

		private async Task< List< WooCommerceProduct > > CollectProductsFromAllPagesAsync( Dictionary< string, string > productFilters, int pageSize, string url, Mark mark )
		{
			var products = new List< WooCommerceProduct >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = productFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var productsWithinPage = await GetNextProductPageAsync( combinedFilters, url, mark );
				if( !productsWithinPage.Any() )
					break;

				foreach( var productWithinPage in productsWithinPage )
				{
					if( productWithinPage.HasVariations && productWithinPage.Id.HasValue ) 
					{ 
						productWithinPage.Variations = await CollectVariationsByProductFromAllPagesAsync( productWithinPage.Id.Value, pageSize, url, mark );
					}
				}

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		private async Task< List< WooCommerceProduct > > GetNextProductPageAsync( Dictionary< string, string> filter, string url, Mark mark )
		{
			var productsWithinPage = await this.WooCommerceNetObjectV3.Product.GetAll( filter ).ConfigureAwait( false );

			//TODO PBL-9291 Log filter as well (see Orders sync for examples)
			//See WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, additionalInfo:
			//$"Orders Received on page {page}: {wooCommerceOrders.ToJson()}", queryStringParams: combinedFilters ) );
			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Products Received: {0}" , productsWithinPage.ToJson() ) ) );

			var svProductsWithinPage = productsWithinPage.Select( p => p.ToSvProduct() ).ToList();

			return svProductsWithinPage;
		}

		private async Task< Dictionary< string, int > > UpdateProductsAsync( IEnumerable< QuantityUpdate > productsToUpdate, string url, Mark mark )
		{
			var productsUpdateRequests = productsToUpdate.Select( productQuantity => new WApiV3.Product
			{
				id = productQuantity.Id, sku = productQuantity.Sku, stock_quantity = productQuantity.Quantity
			} ).ToList();

			return ( await UpdateProductsInSequentialBatchesAsync( productsUpdateRequests, BatchSize, url, mark ) ).Where( p => !string.IsNullOrWhiteSpace( p.Sku ) ).ToDictionary( p => p.Sku, p => p.Quantity ?? 0 );
		}

		private async Task< IEnumerable< WooCommerceVariation > > UpdateVariationsAsync( Dictionary< ProductId, List< QuantityUpdate > > variationsUpdateRequests, string url, Mark mark )
		{
			var result = new List< WooCommerceVariation >();
			var wooCommerceVariationBatch = new WooCommerceNET.Base.BatchObject< WApiV3.Variation >();
			foreach ( var variationsUpdateRequest in variationsUpdateRequests )
			{
				foreach ( var batch in new BatchList< QuantityUpdate >( variationsUpdateRequest.Value, BatchSize ) )
				{
					wooCommerceVariationBatch.update = batch.Select( v => 
						new WApiV3.Variation
						{
							id = v.Id, sku = v.Sku, stock_quantity = v.Quantity
						} ).ToList();

					var batchResult = await this.WooCommerceNetObjectV3.Product.Variations.UpdateRange( variationsUpdateRequest.Key.Id, wooCommerceVariationBatch );

					WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Variations Updated: {0}" , batchResult.ToJson() ) ) );

					result.AddRange( batchResult.update.Where( x => !string.IsNullOrWhiteSpace( x.sku ) ).Select( prV3 => prV3.ToSvVariation() ) );
				}
			}

			return result;
		}

		private async Task< IEnumerable< WooCommerceProduct > > UpdateProductsInSequentialBatchesAsync( IEnumerable< WApiV3.Product > productsUpdateRequests, int batchSize, string url, Mark mark )
		{
			var result = new List< WooCommerceProduct >();
			var wooCommerceProductBatch = new WApiV3.ProductBatch();

			foreach( var batch in new BatchList< WApiV3.Product >( productsUpdateRequests, batchSize ) )
			{
				var productsUpdateRequestBatch = batch.ToList();
				if( !productsUpdateRequestBatch.Any() )
					continue;
				wooCommerceProductBatch.update = productsUpdateRequestBatch;
				var batchResult = await this.WooCommerceNetObjectV3.Product.UpdateRange( wooCommerceProductBatch );

				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Products Updated: {0}" , batchResult.ToJson() ) ) );

				result.AddRange( batchResult.update.Select( prV3 => prV3.ToSvProduct() ) );
			}

			return result;
		}

		private static void LogInventoryToUpdate( List< QuantityUpdate > productsToUpdate, Dictionary< ProductId, List< QuantityUpdate > > variationsToUpdate, string url, Mark mark )
		{ 
			var variationsJson = variationsToUpdate.Select( x => new { ProductId = x.Key.Id, Variations = x.Value } ).ToJson();
			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "productsToUpdate: {0}. variationsToUpdate: {1}" , productsToUpdate.ToJson(), variationsJson ) ) );
		}
	}
}