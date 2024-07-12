using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Models.Configuration;
using WooCommerceAccess.Services;
using WooCommerceAccess.Shared;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.ApiClients
{
	public interface IOrdersApiClient
	{
		/// <summary>
		/// Get orders by filter, one page at a time
		/// </summary>
		/// <param name="orderFilters"></param>
		/// <param name="pageSize"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		Task<IEnumerable<WooCommerceOrder>> CollectOrdersFromAllPagesAsync(Dictionary<string, string> orderFilters,
			int pageSize, string url, Mark mark);
	}

	//TODO Could potentially make this class more generic (ApiPaginationHelper?)
	//to also replace CollectVariationsByProductFromAllPagesAsync, which is nearly identical.
	//Would only have to pass in the this._wcObjectApiV3...Get* delegate, return converter, and a text label for logs. 
	public class OrdersApiClient : IOrdersApiClient
	{
		private readonly WApiV3.WCObject _wcObjectApiV3;
		
		public OrdersApiClient(WApiV3.WCObject wcObjectApiV3)
		{
			this._wcObjectApiV3 = wcObjectApiV3;
		}
		
		//TODO Add unit tests
		public async Task<IEnumerable<WooCommerceOrder>> CollectOrdersFromAllPagesAsync(Dictionary<string, string> orderFilters,
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