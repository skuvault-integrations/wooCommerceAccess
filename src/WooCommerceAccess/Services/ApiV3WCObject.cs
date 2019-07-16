using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceNET;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services
{
	public sealed class ApiV3WCObject : IWCObject
	{
		private readonly WApiV3.WCObject _wcObjectApiV3;

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

			var orders = await this._wcObjectApiV3.Order.GetAll( requestParameters ).ConfigureAwait( false );

			return orders.Select( order => order.ToSvOrder() ).ToArray();
		}

		public async Task< WooCommerceProduct > GetProductBySkuAsync( string sku )
		{
			var requestParameters = new Dictionary< string, string >
			{
				{ "sku", sku }
			};

			var products = await this._wcObjectApiV3.Product.GetAll( requestParameters ).ConfigureAwait( false );
			return products.Select( prV3 => prV3.ToSvProduct() )
						// WooCommerce API returns any sku that contains requested sku
						.FirstOrDefault( product => product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		public async Task< WooCommerceProduct > UpdateProductQuantityAsync(int productId, int quantity)
		{
			var updatedProduct = await this._wcObjectApiV3.Product.Update( productId, new WApiV3.Product() { stock_quantity = quantity });
			return updatedProduct.ToSvProduct();
		}

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
