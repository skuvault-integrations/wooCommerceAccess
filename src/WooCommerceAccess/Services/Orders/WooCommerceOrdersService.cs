using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.ApiClients;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Orders
{
	public class WooCommerceOrdersService : BaseService, IWooCommerceOrdersService
	{
		private readonly IOrdersApiService _ordersApiService;

		public WooCommerceOrdersService( WooCommerceConfig config, Throttler throttler )
			: base( config, throttler )
		{
			this._ordersApiService = new OrdersApiService( base.WCObject.WooCommerceNetObjectV3 );
		}

		public Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark )
		{
			return base.SendRequestAsync< IEnumerable< WooCommerceOrder > >( this._ordersApiService.OrdersApiUrl, mark, ( url, marker ) =>
			{
				return this._ordersApiService.GetOrdersAsync( startDateUtc, endDateUtc, base.Config.OrdersPageSize, url, marker );
			} );
		}
	}
}