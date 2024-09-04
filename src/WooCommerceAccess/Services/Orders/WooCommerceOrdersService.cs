using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.ApiServices;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Orders
{
	public class WooCommerceOrdersService : BaseService, IWooCommerceOrdersService
	{
		private readonly IOrdersApiService _ordersApiService;
		private readonly string _ordersApiUrl;

		public WooCommerceOrdersService( WooCommerceConfig config, Throttler throttler )
			: base( config, throttler )
		{
			this._ordersApiService = new OrdersApiService( base.WCObject.WooCommerceNetObjectV3 );
			this._ordersApiUrl = base.WCObject.WooCommerceNetObjectV3.Order.API.Url + base.WCObject.WooCommerceNetObjectV3.Order.APIEndpoint;
		}

		public async Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark )
		{
			return await base.SendRequestAsync< IEnumerable< WooCommerceOrder > >( this._ordersApiUrl, mark, async ( url, marker ) =>
			{
				return await  this._ordersApiService.GetOrdersAsync( startDateUtc, endDateUtc, base.Config.OrdersPageSize, url, marker );
			} );
		}
	}
}