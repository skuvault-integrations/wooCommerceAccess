﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Orders
{
	public class WooCommerceOrdersService : BaseService, IWooCommerceOrdersService
	{
		private readonly string _serviceUrl;

		public WooCommerceOrdersService( WooCommerceConfig config, Throttler throttler )
			: base( config, throttler )
		{
			_serviceUrl = base.WCObject.OrdersApiUrl;
		}

		public Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark )
		{
			return base.SendRequestAsync< IEnumerable< WooCommerceOrder > >( this._serviceUrl, mark, ( url, marker ) =>
			{
				return base.WCObject.GetOrdersAsync( startDateUtc, endDateUtc, base.Config.OrdersPageSize, url, marker );
			} );
		}
	}
}