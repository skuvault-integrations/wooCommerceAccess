using System;
using System.Collections.Generic;

namespace WooCommerceAccess.Helpers
{
    public static class OrdersFiltersBuilder
    {
        public const string DatesAreGmt = "dates_are_gmt";
        
        public static Dictionary<string, string> CreateModifiedDateRangeFilters( DateTime startDate, DateTime endDate )
        {
            const string dateFilterAfter = "modified_after";
            const string dateFilterBefore = "modified_before";
            var orderFilters = new Dictionary< string, string >
            {
                { dateFilterAfter, startDate.ToWooCommerceStartDateTime() },
                { dateFilterBefore, endDate.ToWooCommerceEndDateTime() }
            };
            if ( startDate.Kind == DateTimeKind.Utc && endDate.Kind == DateTimeKind.Utc )
            {
                orderFilters.Add( DatesAreGmt, "1" );
            }

            return orderFilters;
        }
    }
}