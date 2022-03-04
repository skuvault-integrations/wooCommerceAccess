﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;

namespace WooCommerceAccess.Services.Orders
{
	public interface IWooCommerceOrdersService
	{
		Task< IEnumerable< WooCommerceOrder > > GetOrdersAsync( DateTime startDateUtc, DateTime endDateUtc, Mark mark );
	}
}
