using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
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
			var orders = await this._legacyApiWCObject.GetOrders().ConfigureAwait( false );
			return orders.Where( order => order.updated_at >= startDateUtc && order.updated_at <= endDateUtc )
					 .Select( order => order.ToSvOrder() )
					 .ToArray();
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
