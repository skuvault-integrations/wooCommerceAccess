using NUnit.Framework;
using Legacy = WooCommerceNET.WooCommerce.Legacy;
using WooCommerceAccess.Models;
using FluentAssertions;

namespace WooCommerceTests.Mappers
{
	[ TestFixture ]
	public class OrderMapperTests
	{
		[ Test ]
		public void MapLegacyOrderToSvOrder_DiscountsAndTaxes()
		{
			string couponCode1 = "freeStuff";
			decimal? couponAmount1 = 2.34m;
			string couponCode2 = "moarFreeStuff";
			decimal? couponAmount2 = 3.45m;
			var legacyOrder = new DefaultOrder()
			{
				total_discount = 1.23m,
				coupon_lines = new Legacy.CouponLineList
				{
					new Legacy.CouponLine
					{
						code = couponCode1,
						amount = couponAmount1
					},
					new Legacy.CouponLine
					{
						code = couponCode2,
						amount = couponAmount2
					}
				}
			};

			var result = legacyOrder.ToSvOrder();

			result.TotalDiscount.Should().Be( legacyOrder.total_discount );
			result.Coupons.Count.Should().Be( legacyOrder.coupon_lines.Count );
			result.Coupons[ 0 ].code.Should().Be( couponCode1 );
			result.Coupons[ 0 ].amount.Should().Be( couponAmount1 );
			result.Coupons[ 1 ].code.Should().Be( couponCode2 );
			result.Coupons[ 1 ].amount.Should().Be( couponAmount2 );
		}

		[ Test ]
		public void MapLegacyOrderToSvOrder_ItemDiscountsAndTaxes()
		{
			decimal? line1TotalTax = 0.12m;
			var legacyOrder = new DefaultOrder
			{
				line_items = new Legacy.LineItemList
				{
					new DefaultLineItem()
					{
						total_tax = line1TotalTax
					}
				}
			};

			var result = legacyOrder.ToSvOrder();

			result.Items[ 0 ].TotalTax.Should().Be( line1TotalTax );
		}
	}

	internal class DefaultOrder : Legacy.Order
	{
		public DefaultOrder()
		{
			base.id = 1;
			base.order_number = 1;
			base.total = 12.30m;
			base.total_shipping = 3.21m;
			base.line_items = new Legacy.LineItemList();
		}
	}

	internal class DefaultLineItem : Legacy.LineItem
	{
		public DefaultLineItem()
		{
			base.id = 1;
			base.quantity = 12;
			base.price = 1.23m;
		}
	}
}
