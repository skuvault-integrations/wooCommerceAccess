using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Products
{
	public class WooCommerceProductsService : BaseService, IWooCommerceProductsService
	{
		private readonly string _serviceUrl;

		public WooCommerceProductsService( WooCommerceConfig config, Throttler throttler )
			: base( config, throttler )
		{
			_serviceUrl = base.WCObject.ProductApiUrl;
		}

		public Task< Dictionary< string, int > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities )
		{
			return base.SendRequestAsync< Dictionary< string, int > >( this._serviceUrl, () =>
			{
				if( base.apiVersion == WooCommerceApiVersion.Legacy )
				{
					WooCommerceLogger.LogTrace("Updating variation quantities in legacy is not supported");
				}

				return base.WCObject.UpdateSkusQuantityAsync( skusQuantities, base.Config.ProductsPageSize );
			} );
		}

		public async Task< WooCommerceProduct > UpdateSkuQuantityAsync( string sku, int quantity )
		{
			var product = await this.GetProductBySkuAsync( sku );

			if ( product?.Id == null )
				return null;

			return await base.SendRequestAsync< WooCommerceProduct >( this._serviceUrl, () =>
			{
				return base.WCObject.UpdateProductQuantityAsync( product.Id.Value, quantity );
			} ).ConfigureAwait( false );
		}

		public Task< WooCommerceProduct > GetProductBySkuAsync( string sku )
		{
			return base.SendRequestAsync< WooCommerceProduct >( this._serviceUrl, () =>
			{
				return base.WCObject.GetProductBySkuAsync( sku, base.Config.ProductsPageSize );
			});
		}

		public async Task< IEnumerable< WooCommerceProduct > > GetProductsCreatedUpdatedAfterAsync( DateTime productsStartUtc, bool includeUpdated )
		{
			return await SendRequestAsync( _serviceUrl, () => WCObject.GetProductsCreatedUpdatedAfterAsync( productsStartUtc, includeUpdated, base.Config.ProductsPageSize ) );
		}
	}
}