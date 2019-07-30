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
	public sealed class LegacyV3WCObject : IWCObject
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

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku )
		{
			var requestParameters = new Dictionary< string, string >
			{
				{ "sku", sku }
			};

			var products = await this._legacyApiWCObject.GetProducts( requestParameters ).ConfigureAwait( false );
			return products.Select( legacyProduct => legacyProduct.ToSvProduct() )
						// WooCommerce API returns any sku that contains requested sku
						.FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity )
		{
			var updateProductRequest = new WLegacyApi.Product() { id = productId, stock_quantity = quantity };
			await this._legacyApiWCObject.UpdateProduct( productId, updateProductRequest );
			return updateProductRequest.ToSvProduct();
		}

		public async Task< IEnumerable< WooCommerceProduct > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities )
		{
			var result = new List< WooCommerceProduct >();

			foreach( var skuQuantity in skusQuantities )
			{
				var product = await this.GetProductBySkuAsync( skuQuantity.Key ).ConfigureAwait( false );

				if ( product != null )
				{
					var updatedProduct = await this.UpdateProductQuantityAsync( product.Id.Value, skuQuantity.Value ).ConfigureAwait( false );
					updatedProduct.Sku = skuQuantity.Key;
					result.Add( updatedProduct );
				}
			}

			return result.ToArray();
		}
	}
}
