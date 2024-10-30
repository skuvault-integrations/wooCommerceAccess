using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;

namespace WooCommerceAccess.Services.Orders
{
	public interface IWooCommerceOrdersService
	{
		Task< IEnumerable< WooCommerceOrder > > GetOrdersByModifiedDatesAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark );
		Task< IEnumerable< WooCommerceOrder > > GetOrdersByCreatedDatesAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark );

		Task<WooCommerceOrder> GetOrderByNumberAsync(string orderNumber, Mark mark);
	}
}
