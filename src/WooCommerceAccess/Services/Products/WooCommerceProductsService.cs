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

		public Task< Dictionary< string, int > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, Mark mark )
		{
			return base.SendRequestAsync< Dictionary< string, int > >( this._serviceUrl, mark, ( url, marker ) =>
			{
				if( base.apiVersion == WooCommerceApiVersion.Legacy )
				{
					WooCommerceLogger.LogTrace( "Updating variation quantities in legacy is not supported" );
				}

				return base.WCObject.UpdateSkusQuantityAsync( skusQuantities, base.Config.ProductsPageSize, url, marker );
			} );
		}

		public Task< Dictionary< string, int > > UpdateInventoryAsync( Dictionary< string, int > skusQuantities, Mark mark )
		{
			return base.SendRequestAsync< Dictionary< string, int > >( this._serviceUrl, mark, ( url, marker ) =>
			{
				return base.WCObject.UpdateInventoryAsync( skusQuantities, base.Config.ProductsPageSize, url, marker );
			} );
		}

		public async Task< WooCommerceProduct > UpdateSkuQuantityAsync( string sku, int quantity, Mark mark )
		{
			var product = await this.GetProductBySkuAsync( sku, mark );

			if ( product?.Id == null )
				return null;

			return await base.SendRequestAsync< WooCommerceProduct >( this._serviceUrl, mark, ( url, marker ) =>
			{
				return base.WCObject.UpdateProductQuantityAsync( product.Id.Value, quantity, url, marker );
			} ).ConfigureAwait( false );
		}

		public Task< WooCommerceProduct > GetProductBySkuAsync( string sku, Mark mark )
		{
			return base.SendRequestAsync< WooCommerceProduct >( this._serviceUrl, mark, ( url, marker ) =>
			{
				return base.WCObject.GetProductBySkuAsync( sku, base.Config.ProductsPageSize, url, marker );
			});
		}

		public async Task< IEnumerable< WooCommerceProduct > > GetProductsAsync( DateTime startDateUtc, Mark mark )
		{
			return await SendRequestAsync( _serviceUrl, mark, ( url, marker ) => WCObject.GetProductsAsync( startDateUtc, base.Config.ProductsPageSize, url, marker ) );
		}
	}
}