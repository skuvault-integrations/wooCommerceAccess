﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Helpers;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceAccess.Services;
using WooCommerceAccess.Shared;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.ApiServices
{
	public interface IOrdersApiService
	{
		/// <summary>
		/// Get orders.
		/// </summary>
		/// <param name="startDateUtc"></param>
		/// <param name="endDateUtc"></param>
		/// <param name="pageSize"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize, string url, Mark mark );
	}

	public class OrdersApiService : IOrdersApiService
	{
		private readonly WApiV3.WCObject _wcObjectApiV3;
		
		public OrdersApiService(WApiV3.WCObject wcObjectApiV3)
		{
			this._wcObjectApiV3 = wcObjectApiV3;
		}
		
		public async Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, int pageSize, string url, Mark mark )
		{
			var orderFilters = OrdersFiltersBuilder.CreateModifiedDateRangeFilters(startDateUtc, endDateUtc);
			return await this.CollectOrdersFromAllPagesAsync( orderFilters, pageSize, url, mark ).ConfigureAwait( false );
		}

		private async Task<IEnumerable<WooCommerceOrder>> CollectOrdersFromAllPagesAsync(Dictionary<string, string> orderFilters,
			int pageSize, string url, Mark mark)
		{
			var orders = new List< WooCommerceOrder >();

			for( var page = 1; ; page++ )
			{
				var pageFilter = EndpointsBuilder.CreateGetPageAndLimitFilter( new WooCommerceCommandConfig( page, pageSize ) );
				var combinedFilters = orderFilters.Concat( pageFilter ).ToDictionary( f => f.Key, f => f.Value);
				var wooCommerceOrders = await this._wcObjectApiV3.Order.GetAll( combinedFilters ).ConfigureAwait( false );

				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, additionalInfo:
					$"Orders Received on page {page}: {wooCommerceOrders.ToJson()}", queryStringParams: combinedFilters ) );

				var ordersWithinPage = wooCommerceOrders.Select( v => v.ToSvOrder() ).ToList();
				if( !ordersWithinPage.Any() )
					break;

				orders.AddRange( ordersWithinPage );
			}

			return orders;
		}
	}
}