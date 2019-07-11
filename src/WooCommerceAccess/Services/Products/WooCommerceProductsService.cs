using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Exceptions;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;
using WooCommerceNET.Base;
using WooCommerceV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services.Products
{
	public class WooCommerceProductsService : BaseService, IWooCommerceProductsService
	{
		private readonly string _serviceUrl;

		public WooCommerceProductsService( string consumerKey, string consumerSecret, WooCommerceConfig config, Throttler throttler )
			: base( consumerKey, consumerSecret, config, throttler )
		{
			_serviceUrl = base.WCObject.Product.API.Url + base.WCObject.Product.APIEndpoint;
		}

		public async Task< Product[] > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token )
		{
			var productBatch = new WooCommerceV3.ProductBatch();
			var productsUpdateRequest = new List< WooCommerceV3.Product >();
			productBatch.update = productsUpdateRequest;

			foreach( var skuQuantity in skusQuantities )
			{
				var product = await this.GetProductBySkuAsync( skuQuantity.Key, CancellationToken.None ).ConfigureAwait( false );

				if ( product != null )
					productsUpdateRequest.Add( new WooCommerceV3.Product() { id = product.Id, sku = skuQuantity.Key, stock_quantity = skuQuantity.Value } );
			}

			var result = await base.SendRequestAsync< BatchObject< WooCommerceV3.Product > >( this._serviceUrl, () =>
			{
				return base.WCObject.Product.UpdateRange( productBatch );
			} ).ConfigureAwait( false );

			return result.update.Select( prV3 => prV3.ToProduct() ).ToArray();
		}

		public async Task< Product > UpdateSkuQuantityAsync( string sku, int quantity )
		{
			var product = await this.GetProductBySkuAsync( sku, CancellationToken.None );

			if ( product == null )
				return null;

			return await base.SendRequestAsync< Product >( this._serviceUrl, async () =>
			{
				var updatedProduct = await base.WCObject.Product.Update(product.Id.Value, new WooCommerceV3.Product() { stock_quantity = quantity });
				return updatedProduct.ToProduct();
			} );
		}

		public Task< Product > GetProductBySkuAsync( string sku, CancellationToken token )
		{
			var requestParameters = new Dictionary< string, string >
			{
				{ "sku", sku }
			};

			return base.SendRequestAsync< Product >( this._serviceUrl, async () =>
			{
				var products = await base.WCObject.Product.GetAll( requestParameters ).ConfigureAwait( false );
				return products.Select( prV3 => prV3.ToProduct() ).FirstOrDefault();
			});
		}
	}
}