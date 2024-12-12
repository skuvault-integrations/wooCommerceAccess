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
                //Sortable "s" format: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-sortable-s-format-specifier
                { dateFilterAfter, startDate.RoundDateDownToTopOfMinute().ToString( "s" ) },
                { dateFilterBefore, endDate.RoundDateUpToTopOfMinute().ToString( "s" ) }
            };
            if ( startDate.Kind == DateTimeKind.Utc && endDate.Kind == DateTimeKind.Utc )
            {
                orderFilters.Add( DatesAreGmt, "1" );
            }

            return orderFilters;
        }
    }
}