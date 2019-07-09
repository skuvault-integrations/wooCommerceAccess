using System;
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
		public WooCommerceProductsService( string consumerKey, string consumerSecret, WooCommerceConfig config, Throttler throttler )
			: base( consumerKey, consumerSecret, config, throttler )
		{ }

		public Task UpdateSkusQuantityAsync(Dictionary<string, int> skusQuantities, CancellationToken token)
		{
			//TODO GUARD-118 Implement
			throw new NotImplementedException();
		}

		public Task<Product> GetProductBySku(string sku, CancellationToken token)
		{
			//TODO GUARD-118 Implement
			throw new NotImplementedException();
		}
	}
}
