using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceAccess.Shared;
using WooCommerceNET;
using WLegacyApi = WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerceAccess.Services
{
	/// <inheritdoc />
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

		public string SystemStatusApiUrl => this._apiUrl + "system-status";

		/// <summary>
		/// We do not use Settings Legacy API but the field should be in the class to implements the IWCObject interface
		/// </summary>
		public string SettingsApiUrl => "";

		public async Task< string > GetStoreVersionAsync( string url, Mark mark )
		{
			var storeInfo = await this._legacyApiWCObject.GetStoreInfo().ConfigureAwait( false );

			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Legacy Store Info: {0}" , storeInfo.ToJson() ) ) );

			return storeInfo.wc_version;
		}

		public async Task< WooCommerceSettings > GetSettingsAsync( string url, Mark mark )
		{
			// get weight_unit from the store settings
			var storeInfo = await this._legacyApiWCObject.GetStoreInfo().ConfigureAwait( false );

			var settings = new WooCommerceSettings
			{
				Currency = storeInfo?.meta?.currency,
				WeightUnit = storeInfo?.meta?.weight_unit
			};

			return settings;
		}

		public Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize, string url, Mark mark )
		{
			var ordersFilters = new Dictionary< string, string >
			{
				{ "filter[updated_at_min]", startDateUtc.ToString( "o" ) },
				{ "filter[updated_at_max]", endDateUtc.ToString( "o" ) }
			};
			
			return CollectOrdersFromAllPagesAsync( ordersFilters, pageSize, url, mark );
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku, int pageSize, string url, Mark mark )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "filter[sku]", sku }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters, pageSize, url, mark );

			return products.
				// WooCommerce API returns any sku that contains requested sku
				FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< IEnumerable< WooCommerceProduct > > GetProductsAsync( DateTime startDateUtc, int pageSize, string url, Mark mark )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "filter[updated_at_min]", startDateUtc.ToString( "o" ) },
			};

			return await CollectProductsFromAllPagesAsync( productFilters, pageSize, url, mark );
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
			var productsToUpdate = await GetProductsToUpdateAsync( async filter => await GetNextProductPageAsync( filter, url, mark ), skusQuantities, pageSize );
			var updatedProducts = await UpdateProductsAsync( productsToUpdate, url, mark );
			return updatedProducts.ToDictionary( p => p.Key, p => p.Value );
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
			var productsAndVariationsToUpdate = CreateProductsVariationsInventoryUpdateRequests( products, skusQuantities );
			var updatedProducts = await UpdateProductsAsync( productsAndVariationsToUpdate, url, mark );
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

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity, string url, Mark mark )
		{
			var updateProductRequest = new WLegacyApi.Product() { id = productId, stock_quantity = quantity };
			var result = await this._legacyApiWCObject.UpdateProduct( productId, updateProductRequest );

			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Legacy Product Updated: {0}" , result ) ) );

			return updateProductRequest.ToSvProduct();
		}

		private async Task< IEnumerable< WooCommerceOrder > > CollectOrdersFromAllPagesAsync( Dictionary< string, string > ordersFilters, int pageSize, string url, Mark mark )
		{
			var orders = new List< WooCommerceOrder >();

			for (var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateLegacyApiV3GetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = ordersFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value );
				var ordersWithinPage = await this._legacyApiWCObject.GetOrders( combinedFilters ).ConfigureAwait( false );

				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Orders Received: {0}" , ordersWithinPage.ToJson() ) ) );

				if ( !ordersWithinPage.Any() )
					break;

				orders.AddRange( ordersWithinPage.Select( order => order.ToSvOrder() ).ToList() );
			}
			
			return orders;
		}

		private async Task< Dictionary< string, int > > UpdateProductsAsync( IEnumerable< QuantityUpdate > productsToUpdate, string url, Mark mark  )
		{
			var result = new Dictionary< string, int >();
			var updatedProductIds = new List< ProductId >();

			foreach( var productToUpdate in productsToUpdate )
			{
				var updatedProduct = await this.UpdateProductQuantityAsync( productToUpdate.Id, productToUpdate.Quantity, url, mark ).ConfigureAwait( false );

				if ( updatedProduct == null )
					continue;
				result.Add( productToUpdate.Sku, productToUpdate.Quantity );
				updatedProductIds.Add( new ProductId( productToUpdate.Id ));
			}
			return result;
		}

		private async Task< IEnumerable< WooCommerceProduct > > CollectProductsFromAllPagesAsync( Dictionary< string, string > productFilters, int pageSize, string url, Mark mark )
		{
			var products = new List< WooCommerceProduct >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateLegacyApiV3GetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = productFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var productsWithinPage = await GetNextProductPageAsync( combinedFilters, url, mark );
				if( !productsWithinPage.Any() )
					break;

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		private async Task< List< WooCommerceProduct > > GetNextProductPageAsync( Dictionary< string, string> filter, string url, Mark mark )
		{
			var productsWithinPage = await this._legacyApiWCObject.GetProducts( filter ).ConfigureAwait( false );

			WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Legacy Products Received: {0}" , productsWithinPage.ToJson() ) ) );
			
			var svProductsWithinPage = productsWithinPage.Select( p => p.ToSvProduct() ).ToList();

			return svProductsWithinPage;
		}
	}
}