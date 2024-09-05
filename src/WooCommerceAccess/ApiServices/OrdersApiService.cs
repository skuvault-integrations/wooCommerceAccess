using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
			const string dateFilterAfter = "modified_after";
			const string dateFilterBefore = "modified_before";
			var orderFilters = new Dictionary< string, string >
			{
				{ dateFilterAfter, startDateUtc.ToString( "o" ) },
				{ dateFilterBefore, endDateUtc.ToString( "o" ) }
			};
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

				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, payload: string.Format( "Orders Received on page {0}: {1}",
					page, wooCommerceOrders.ToJson() ) ) );

				var ordersWithinPage = wooCommerceOrders.Select( v => v.ToSvOrder() ).ToList();
				if( !ordersWithinPage.Any() )
					break;

				orders.AddRange( ordersWithinPage );
			}

			return orders;
		}
	}
}