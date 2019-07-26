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

		public async Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc )
		{
			//TODO GUARD-120 Add paging. See products

			//TODO GUARD-120 Wouldn't filter[updated_at_min] and filter[updated_at_max] accomplish the same, but filter on the wooCommerce api side
			var orders = await this._legacyApiWCObject.GetOrders().ConfigureAwait( false );
			return orders.Where( order => order.updated_at >= startDateUtc && order.updated_at <= endDateUtc )
					 .Select( order => order.ToSvOrder() )
					 .ToArray();
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "filter[sku]", sku }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters );

			return products.
				// WooCommerce API returns any sku that contains requested sku
				FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< IEnumerable < WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated )
		{
			var dateFilter = includeUpdated ? "filter[updated_at_min]" : "filter[created_at_min]";

			var productFilters = new Dictionary< string, string >
			{
				{ dateFilter, productsStartUtc.ToString( "o" ) },
			};

			return await CollectProductsFromAllPagesAsync( productFilters );
		}

		private async Task< IEnumerable < WooCommerceProduct > > CollectProductsFromAllPagesAsync( Dictionary< string, string > productFilters )
		{
			var products = new List< WooCommerceProduct >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageFilter( new WooCommerceCommandConfig( page ) );
				var combinedFilters = productFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var productsWithinPage = ( await this._legacyApiWCObject.GetProducts( combinedFilters ).ConfigureAwait( false ) ).
					Select( p => p.ToSvProduct() ).ToList();
				if( !productsWithinPage.Any() )
					break;

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync( int productId, int quantity )
		{
			var updateProductRequest = new WLegacyApi.Product() { id = productId, stock_quantity = quantity };
			await this._legacyApiWCObject.UpdateProduct( productId, updateProductRequest );
			return updateProductRequest.ToSvProduct();
		}

		//TODO GUARD-118 Explore if will need to add paging, it only does 10 by default. See products
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
