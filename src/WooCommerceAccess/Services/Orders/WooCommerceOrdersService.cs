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

		public async Task< IEnumerable< WooCommerceOrder > > GetOrdersByModifiedDatesAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark )
		{
			return await base.SendRequestAsync< IEnumerable< WooCommerceOrder > >( this._ordersApiUrl, mark, async ( url, marker ) =>
			{
				return await  this._ordersApiService.GetOrdersByModifiedDatesAsync( startDateUtc, endDateUtc, base.Config.OrdersPageSize, url, marker ).ConfigureAwait( false );
			} ).ConfigureAwait( false );
		}

		public async Task<IEnumerable<WooCommerceOrder>> GetOrdersByCreatedDatesAsync(DateTime startDateUtc, DateTime endDateUtc, Mark mark)
		{
			return await base.SendRequestAsync< IEnumerable< WooCommerceOrder > >( this._ordersApiUrl, mark, async ( url, marker ) =>
			{
				return await  this._ordersApiService.GetOrdersByCreatedDatesAsync( startDateUtc, endDateUtc, base.Config.OrdersPageSize, url, marker ).ConfigureAwait( false );
			} ).ConfigureAwait( false );
		}

		public async Task< WooCommerceOrder > GetOrderByNumberAsync( string orderNumber, Mark mark )
		{
			return await base.SendRequestAsync( this._ordersApiUrl, mark, async ( url, marker ) 
				=> await this._ordersApiService.GetOrderByNumberAsync( orderNumber ).ConfigureAwait( false )).ConfigureAwait( false );
		}
	}
}