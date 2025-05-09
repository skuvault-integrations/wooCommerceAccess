using System;
using System.Collections.Generic;

namespace WooCommerceAccess.Helpers
{
	public static class ProductsFiltersBuilder
	{
		private const string DatesAreGmt = "dates_are_gmt";

		//TODO PBL-9291 Add unit tests
		internal static Dictionary<string, string> CreateProductStartDateTimeFilters(DateTime startDateUtc,
			bool includeUpdated)
		{
			var dateFilter = includeUpdated ? "modified_after" : "after";
			var productFilters = new Dictionary<string, string>
			{
				{ dateFilter, startDateUtc.ToWooCommerceStartDateTime() }
			};

			if (startDateUtc.Kind == DateTimeKind.Utc)
			{
				productFilters.Add(DatesAreGmt, "1");
			}

			return productFilters;
		}
	}
}