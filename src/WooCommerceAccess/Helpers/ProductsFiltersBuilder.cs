using System;
using System.Collections.Generic;

namespace WooCommerceAccess.Helpers
{
	public static class ProductsFiltersBuilder
	{
		internal const string DatesAreGmt = "dates_are_gmt";

		/// <summary>
		/// Create the filter to query for products by modified/created start date/time
		/// </summary>
		/// <param name="startDateUtc"></param>
		/// <param name="includeUpdated">If true, filter by both the created and updated dates. Otherwise, only created date</param>
		/// <returns></returns>
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