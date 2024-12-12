using System;
using NUnit.Framework;
using WooCommerceAccess.Helpers;

namespace WooCommerceTests.Helpers
{
    public class OrdersFiltersBuilderTests
    {
        [ Test ]
        public void CreateModifiedDateRangeFilters_ShouldSetDatesAreGmtToTrue_WhenBothDatesAreUtc()
        {
            var startDate = new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            var endDate = new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            
            var result = OrdersFiltersBuilder.CreateModifiedDateRangeFilters(startDate, endDate);
            
            Assert.That( result[OrdersFiltersBuilder.DatesAreGmt], Is.EqualTo( "1" ) );
        }
        
        [ TestCase( DateTimeKind.Utc, DateTimeKind.Unspecified ) ]
        [ TestCase( DateTimeKind.Unspecified, DateTimeKind.Utc ) ]
        [ TestCase( DateTimeKind.Unspecified, DateTimeKind.Unspecified ) ]
        [ TestCase( DateTimeKind.Utc, DateTimeKind.Local ) ]
        [ TestCase( DateTimeKind.Local, DateTimeKind.Utc ) ]
        [ TestCase( DateTimeKind.Local, DateTimeKind.Local ) ]
        public void CreateModifiedDateRangeFilters_ShouldNotSetDatesAreGmtToTrue_WhenOneDateIsNotUtc( DateTimeKind startDateKind, DateTimeKind endDateKind )
        {
            var startDate = new DateTime(2001, 1, 1, 1, 1, 1, startDateKind);
            var endDate = new DateTime(2001, 1, 1, 1, 1, 1, endDateKind);
            
            var result = OrdersFiltersBuilder.CreateModifiedDateRangeFilters(startDate, endDate);
            
            Assert.That( result.ContainsKey( OrdersFiltersBuilder.DatesAreGmt ), Is.False );
        }
    }
}