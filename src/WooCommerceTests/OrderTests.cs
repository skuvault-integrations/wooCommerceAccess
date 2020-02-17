using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WooCommerceTests
{
	[ TestFixture( "WP4_7_WC_3_6_credentials.csv" ) ]
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	public class OrderTests : BaseTest
	{
		public OrderTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ Test ]
		public async Task GetOrders()
		{
			var orders = await this.OrdersService.GetOrdersAsync( DateTime.UtcNow.AddMonths( -3 ), DateTime.UtcNow );
			orders.Should().NotBeNullOrEmpty();
			orders.Count().Should().BeGreaterOrEqualTo( 1 );
		}

		[ Test ]
		public async Task GetOrderUsingByPagination()
		{
			base.Config.OrdersPageSize = 1;

			var orders = await this.OrdersService.GetOrdersAsync( DateTime.UtcNow.AddMonths( -3 ), DateTime.UtcNow );
			orders.Should().NotBeNullOrEmpty();
			orders.Count().Should().BeGreaterOrEqualTo( 3 );

			base.Config.OrdersPageSize = 10;
		}
	}
}
