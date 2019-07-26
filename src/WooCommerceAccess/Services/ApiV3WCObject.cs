using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netco.Extensions;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceNET;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services
{
	public sealed class ApiV3WCObject : IWCObject
	{
		private readonly WApiV3.WCObject _wcObjectApiV3;
		private readonly int _pageSize = 10;

		public ApiV3WCObject( RestAPI restApi )
		{
			Condition.Requires( restApi, "restApi" ).IsNotNull();
			this._wcObjectApiV3 = new WApiV3.WCObject( restApi );
		}

		public string ProductApiUrl => this._wcObjectApiV3.Product.API.Url + this._wcObjectApiV3.Product.APIEndpoint;

		public string OrdersApiUrl => this._wcObjectApiV3.Order.API.Url + this._wcObjectApiV3.Order.APIEndpoint;

		public async Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc )
		{
			var requestParameters = new Dictionary< string, string >
			{
				{ "after", startDateUtc.ToString( "o" ) },
				{ "before", endDateUtc.ToString( "o" ) }
			};

			//TODO GUARD-120 Add paging. See products
			var orders = await this._wcObjectApiV3.Order.GetAll( requestParameters ).ConfigureAwait( false );

			return orders.Select( order => order.ToSvOrder() ).ToArray();
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku )
		{
			var productFilters = new Dictionary< string, string >
			{
				{ "sku", sku }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters );
			return products.
				// WooCommerce API returns any sku that contains requested sku
				FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< IEnumerable < WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated )
		{
			const string updatedAfter = "after";
			var productFilters = new Dictionary< string, string >
			{
				{ updatedAfter, productsStartUtc.ToString( "o" ) }
			};

			var products = await CollectProductsFromAllPagesAsync( productFilters );

			if ( !includeUpdated )
			{
				products = products.Where( p => p.CreatedDateUtc >= productsStartUtc ).ToList();
			}

			return products;
		}
		
		private async Task< List< WooCommerceProduct > > CollectProductsFromAllPagesAsync( Dictionary< string, string > productFilters )
		{
			var products = new List< WooCommerceProduct >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, _pageSize ) );
				var combinedFilters = productFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var productsWithinPage = ( await this._wcObjectApiV3.Product.GetAll( combinedFilters ).ConfigureAwait( false ) ).
					Select( p => p.ToSvProduct() ).ToList();
				if( !productsWithinPage.Any() )
					break;

				foreach( var productWithinPage in productsWithinPage )
				{
					if( productWithinPage.HasVariations && productWithinPage.Id.HasValue ) 
					{ 
						productWithinPage.Variations = await CollectProductVariationsFromAllPages( productWithinPage.Id.Value );
					}
				}

				products.AddRange( productsWithinPage );
			}

			return products;
		}

		private async Task< IEnumerable< WooCommerceVariation > > CollectProductVariationsFromAllPages( int productId )
		{
			var variations = new List< WooCommerceVariation >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, _pageSize ) );
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

		//TODO GUARD-118 Explore if will need to add paging, it only does 10 by default. See products
		public async Task< IEnumerable < WooCommerceProduct > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities )
		{
			var productBatch = new WApiV3.ProductBatch();
			var productsUpdateRequest = new List< WApiV3.Product >();
			productBatch.update = productsUpdateRequest;

			foreach( var skuQuantity in skusQuantities )
			{
				var product = await this.GetProductBySkuAsync( skuQuantity.Key ).ConfigureAwait( false );

				if ( product != null )
					productsUpdateRequest.Add( new WApiV3.Product() { id = product.Id, sku = skuQuantity.Key, stock_quantity = skuQuantity.Value } );
			}

			var result = await this._wcObjectApiV3.Product.UpdateRange( productBatch );
			return result.update.Select( prV3 => prV3.ToSvProduct() ).ToArray();
		}
	}
}
