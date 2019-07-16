using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Orders
{
	public class WooCommerceOrdersService : BaseService, IWooCommerceOrdersService
	{
		private readonly string _serviceUrl;

		public WooCommerceOrdersService( string consumerKey, string consumerSecret, WooCommerceConfig config, Throttler throttler )
			: base( consumerKey, consumerSecret, config, throttler )
		{
			_serviceUrl = base.WCObject.OrdersApiUrl;
		}

		public Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc )
		{
			return base.SendRequestAsync< IEnumerable< WooCommerceOrder > >( this._serviceUrl, () =>
			{
				return base.WCObject.GetOrdersAsync( startDateUtc, endDateUtc );
			} );
		}
	}
}