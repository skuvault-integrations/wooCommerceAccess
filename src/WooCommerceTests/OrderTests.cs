using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_VM_sandbox.csv" ) ]
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	// [ TestFixture( "credentials_TEN-IMHPurfume 358319-PBL-9226.csv" ) ]
	[ TestFixture( "credentials_TEN-SkinnyBox-357257-GUARD-3624.csv" ) ]
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
		public async Task GetOrdersAsync_Created()
		{
			//307660: created/updated 2024-10-31T12:59:55
			var startDateUtc = new DateTime(2024, 10, 31, 12, 58, 35, DateTimeKind.Utc);
			//"Number": "64092"
			// "CreateDateUtc": "2024-09-12T14:20:06",
			// "UpdateDateUtc": "2024-09-13T14:32:15",
			//BY MODIFIED
			// var orders = await this.OrdersService.GetOrdersByModifiedDatesAsync( startDateUtc, endDateUtc, this.Mark );
			//2024-09-12T08:20:06, 2024-09-12T23:30:15: Fail
			//2024-09-12T08:20:06, 2024-09-13T01:30:15: Fail
			//					,  2024-09-13T10:30:15: Fail
			//					,  2024-09-13T10:33:15 & after: Succeed (pos due to the 4hr time diff between UTC & local)
			//2024-09-13T10:31:15, 2024-09-13T10:33:15 & after: Succeed (pos due to the 4hr time diff between UTC & local)
			//2024-09-12T10:18:15, 2024-09-12T10:22:15: Fails because it's the created range
			//BY BEFORE, AFTER (created)
			//var orders = await this.OrdersService.GetOrdersByCreatedDatesAsync( startDateUtc, endDateUtc, this.Mark );
			//2024-09-12T10:18:15, 2024-09-12T10:22:15: Succeeds
			//2024-09-12T10:21:15, 2024-09-12T10:22:15: Fails (outside the range)
			// var startDateUtc = DateTime.Parse("2024-09-12T10:18:15"); //DateTime.UtcNow.AddDays( -1 );
			// var endDateUtc = DateTime.Parse("2024-09-12T10:22:15"); //DateTime.UtcNow;
			var endDateUtc = new DateTime(2024, 10, 31, 13, 00, 30, DateTimeKind.Utc);
			var orders = await this.OrdersService.GetOrdersByCreatedDatesAsync( startDateUtc, endDateUtc, this.Mark );
			
			orders.Count().Should().BeGreaterOrEqualTo( 1 );
			var order = orders.Where(x => x.Number == "307660").SingleOrDefault();
		}

		[ Explicit ]
		[ Test ]
		//Querying by modified is what we do in the trunk
		public async Task GetOrdersAsync_Modified()
		{
			//"Number": "66061"
			//307660: created/updated 2024-10-31T12:59:55
			var startDateUtc = new DateTime(2024, 10, 31, 12, 58, 45, DateTimeKind.Utc);
			
			//var startDateUtc = DateTime.Parse("2024-10-14T12:22:00");
			//306796 //10/11/2024 16:16:51' to '10/11/2024 16:29:03
			//var endDateUtc = DateTime.Parse("2024-10-11T16:29:03");
			
			// var endDateUtc = new DateTime(2024, 10, 12, 12, 47, 0, DateTimeKind.Utc);
			var endDateUtc = new DateTime(2024, 10, 31, 13, 01, 08, DateTimeKind.Utc);
			//var endDateUtc = DateTime.Parse("2024-10-14T16:23:00");
			var orders = await this.OrdersService.GetOrdersByModifiedDatesAsync( startDateUtc, endDateUtc, this.Mark );
			
			orders.Count().Should().BeGreaterOrEqualTo( 1 );
			var order = orders.Where(x => x.Number == "307660").SingleOrDefault();
		}

		[Explicit]
		[Test]
		public async Task GetOrderByNumberAsync()
		{
			var order = await this.OrdersService.GetOrderByNumberAsync("307660", this.Mark);
		}
		
		[ Explicit ]
		[ Test ]
		public async Task GetOrdersAsyncCloud()
		{
			// "Id": "3326",
			// "Number": "SKU2695END",
			// "CreateDateUtc": "2024-09-13T19:05:46",
			// "UpdateDateUtc": "2024-09-13T19:06:02",
			var startDateUtc = DateTime.Parse("2024-09-16T13:42:39");//DateTime.UtcNow.AddDays( -1 );
			var endDateUtc = DateTime.UtcNow; //DateTime.Parse("2024-09-16T13:29:00");
			//UPDATED
			//2024-09-13T19:05:02, 2024-09-13T19:07:02: True
			//2024-09-13T19:05:59, 2024-09-13T19:06:01: Fail, expected outside the update range
			//2024-09-13T19:05:39, 2024-09-13T19:06:00: Fail, inside the create range, but not the update range
			var orders = await this.OrdersService.GetOrdersByModifiedDatesAsync( startDateUtc, endDateUtc, this.Mark );
			
			orders.Count().Should().BeGreaterOrEqualTo( 1 );
			var order = orders.Where(x => int.Parse(x.Id) > 3329).SingleOrDefault();
		}

		[ Explicit ]
		[ Test ]
		public async Task GetOrdersAsync_ReturnsMultiplePages_WhenOrdersSpanMultiplePages()
		{
			base.Config.OrdersPageSize = 1;

			// Act
			var orders = await this.OrdersService.GetOrdersByModifiedDatesAsync( DateTime.UtcNow.AddMonths( -3 ), DateTime.UtcNow, this.Mark );

			orders.Count().Should().BeGreaterOrEqualTo( 3 );
		}
	}
}
