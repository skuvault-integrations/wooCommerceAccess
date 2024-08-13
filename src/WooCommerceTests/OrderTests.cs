using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_VM_sandbox.csv" ) ]
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class OrderTests : BaseTest
	{
		public OrderTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ SetUp ]
		public void Init()
		{
			//Reset to default since some tests change it
			base.Config.OrdersPageSize = 10;
		}

		[ Explicit ]
		[ Test ]
		public async Task GetOrdersAsync()
		{
			var orders = await this.OrdersService.GetOrdersAsync( DateTime.UtcNow.AddMonths( -3 ), DateTime.UtcNow, this.Mark );
			
			orders.Count().Should().BeGreaterOrEqualTo( 1 );
		}

		[ Explicit ]
		[ Test ]
		public async Task GetOrdersAsync_ReturnsMultiplePages_WhenOrdersSpanMultiplePages()
		{
			base.Config.OrdersPageSize = 1;

			// Act
			var orders = await this.OrdersService.GetOrdersAsync( DateTime.UtcNow.AddMonths( -3 ), DateTime.UtcNow, this.Mark );

			orders.Count().Should().BeGreaterOrEqualTo( 3 );
		}
	}
}
