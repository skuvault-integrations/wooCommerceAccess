using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Products
{
	public class WooCommerceProductsService : BaseService, IWooCommerceProductsService
	{
		private readonly string _serviceUrl;

		public WooCommerceProductsService( string consumerKey, string consumerSecret, WooCommerceConfig config, Throttler throttler )
			: base( consumerKey, consumerSecret, config, throttler )
		{
			_serviceUrl = base.WCObject.ProductApiUrl;
		}

		public Task< IEnumerable< WooCommerceProduct > > UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token )
		{
			return base.SendRequestAsync< IEnumerable< WooCommerceProduct > >( this._serviceUrl, () =>
			{
				return base.WCObject.UpdateSkusQuantityAsync( skusQuantities );
			} );
		}

		public async Task< WooCommerceProduct > UpdateSkuQuantityAsync( string sku, int quantity )
		{
			var product = await this.GetProductBySkuAsync( sku, CancellationToken.None );

			if ( product == null )
				return null;

			return await base.SendRequestAsync< WooCommerceProduct >( this._serviceUrl, () =>
			{
				return base.WCObject.UpdateProductQuantityAsync( product.Id.Value, quantity );
			} ).ConfigureAwait( false );
		}

		public Task< WooCommerceProduct > GetProductBySkuAsync( string sku, CancellationToken token )
		{
			return base.SendRequestAsync< WooCommerceProduct >( this._serviceUrl, () =>
			{
				return base.WCObject.GetProductBySkuAsync( sku );
			});
		}
	}
}