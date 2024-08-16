using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WooCommerceAccess.Models;
using WooCommerceNET.WooCommerce.v2;
using Order = WooCommerceNET.WooCommerce.v3.Order;

namespace WooCommerceTests.Mappers
{
	public class OrderMapperTests
	{
		private static readonly Randomizer _randomizer = new Randomizer();
		
		[ Test ]
		public void ToSvOrder_ShouldMapFieldsCorrectly_WhenPopulatedWithStandardValues()
		{
			var order = CreateWooCommerceOrder();

			var result = order.ToSvOrder();

			Assert.Multiple(() =>
			{
				Assert.That( result.Id, Is.EqualTo( order.id ));
				Assert.That( result.Number, Is.EqualTo( order.number ) );
				AssertBuyerInfoMapped( result.BuyerInfo, order.billing );
				AssertLineItemsMapped( result.Items.Single(), order.line_items.Single() );
				AssertOrderAmountsMapped( result, order );
				AssertCouponLinesMapped( result.Coupons.Single(), order.coupon_lines.Single() );
				Assert.That( result.Currency, Is.EqualTo( order.currency ));
				Assert.That( result.Note, Is.EqualTo( order.customer_note ));
				AssertDatesMapped( result, order );
				AssertShippingAddressMapped( result.ShippingAddress, order.shipping );
				Assert.That( result.ShippingInfo.Common, Is.EqualTo( order.shipping_lines[ 0 ].method_title + "," + order.shipping_lines[ 1 ].method_title ) );
			} );
		}

		[ Test ]
		public void ToSvOrder_ReturnsWasPaidFalse_WhenPaidDateIsNull()
		{
			var order = CreateWooCommerceOrder();
			order.date_paid_gmt = null;

			var result = order.ToSvOrder();

			Assert.IsFalse( result.WasPaid );
		}
		
		[ Test ]
		public void ToSvOrder_ShouldDefaultNullOptionalFields()
		{
			var order = CreateWooCommerceOrder();
			order.total = null;
			order.total_tax = null;
			order.discount_total = null;
			order.line_items[ 0 ].id = null;
			order.line_items[ 0 ].product_id = null;
			order.line_items[ 0 ].total_tax = null;
			
			//Act
			var result = order.ToSvOrder();
			
			Assert.Multiple(() =>
			{
				Assert.That( result.Total, Is.EqualTo( default( decimal ) ) );
				Assert.That( result.TotalTax, Is.EqualTo( default( decimal ) ) );
				Assert.That( result.TotalDiscount, Is.EqualTo( default( decimal ) ) );
				var resultFirstLineItem = result.Items.Single();
				Assert.That( resultFirstLineItem.Id, Is.EqualTo( default( int ) ) );
				Assert.That( resultFirstLineItem.ProductId, Is.EqualTo( default( int ) ) );
				Assert.That( resultFirstLineItem.TotalTax, Is.EqualTo( default( decimal ) ) );
			} );
		}
		
		[ Test ]
		public void ToSvOrder_ShouldThrow_WhenLineItemQuantityIsNull()
		{
			var order = CreateWooCommerceOrder();
			order.line_items[ 0 ].quantity = null;
			
			// Act
			// Assert
			Assert.Throws< Exception >(() => order.ToSvOrder());
		}

		[ Test ]
		public void ToSvOrder_ShouldThrow_WhenLineItemPriceIsNull()
		{
			var order = CreateWooCommerceOrder();
			order.line_items[ 0 ].price = null;
			
			// Act
			// Assert
			Assert.Throws< Exception >(() => order.ToSvOrder());
		}

		private static Order CreateWooCommerceOrder()
		{
			return new Order
			{
				id = _randomizer.GetString(),
				number = _randomizer.GetString(),
				billing = new OrderBilling
				{
					company = _randomizer.GetString(),
					first_name = _randomizer.GetString(),
					last_name = _randomizer.GetString(),
					email = _randomizer.GetString(),
					phone = _randomizer.GetString()
				},
				line_items = new List<OrderLineItem>
				{
					new OrderLineItem
					{
						id = ( int )_randomizer.NextUInt(),
						product_id = ( int )_randomizer.NextUInt(),
						sku = _randomizer.GetString(),
						quantity = ( int )_randomizer.NextUInt(),
						price = ( int )_randomizer.NextUInt(),
						total_tax = ( int )_randomizer.NextUInt()
					}
				},
				coupon_lines = new List<OrderCouponLine>
				{
					new OrderCouponLine
					{
						code = _randomizer.GetString(),
						discount = _randomizer.NextDecimal()
					}
				},
				currency = _randomizer.GetString(),
				customer_note = _randomizer.GetString(),
				date_created_gmt = DateTime.UtcNow,
				date_modified_gmt = DateTime.UtcNow.AddMinutes( _randomizer.Next( 1, 10 ) ),
				date_paid_gmt = DateTime.UtcNow.AddMinutes( _randomizer.Next( 11, 20 ) ),
				total = _randomizer.NextDecimal(),
				total_tax = _randomizer.NextDecimal(),
				discount_total = _randomizer.NextDecimal(),
				shipping = new OrderShipping
				{
					address_1 = _randomizer.GetString(),
					address_2 = _randomizer.GetString(),
					city = _randomizer.GetString(),
					country = _randomizer.GetString(),
					postcode = _randomizer.GetString(),
					state = _randomizer.GetString()
				},
				shipping_lines = new List<OrderShippingLine>
				{
					new OrderShippingLine
					{
						method_title = _randomizer.GetString()
					},
					new OrderShippingLine
					{
						method_title = _randomizer.GetString()
					}
				}
			};
		}

		private static void AssertBuyerInfoMapped( WooCommerceBuyerInfo resultBuyerInfo, OrderBilling orderBilling )
		{
			Assert.That( resultBuyerInfo.Company, Is.EqualTo( orderBilling.company ) );
			Assert.That( resultBuyerInfo.FirstName, Is.EqualTo( orderBilling.first_name ) );
			Assert.That( resultBuyerInfo.LastName, Is.EqualTo( orderBilling.last_name ) );
			Assert.That( resultBuyerInfo.Email, Is.EqualTo( orderBilling.email ) );
			Assert.That( resultBuyerInfo.Phone, Is.EqualTo( orderBilling.phone ) );
		}

		private static void AssertLineItemsMapped( WooCommerceOrderItem resultFirstLineItem, OrderLineItem orderFirstLineItem )
		{
			Assert.That( resultFirstLineItem.Id, Is.EqualTo( orderFirstLineItem.id ) );
			Assert.That( resultFirstLineItem.ProductId, Is.EqualTo( orderFirstLineItem.product_id ) );
			Assert.That( resultFirstLineItem.Sku, Is.EqualTo( orderFirstLineItem.sku ) );
			Assert.That( resultFirstLineItem.Quantity, Is.EqualTo( orderFirstLineItem.quantity ) );
			Assert.That( resultFirstLineItem.Price, Is.EqualTo( orderFirstLineItem.price ) );
			Assert.That( resultFirstLineItem.TotalTax, Is.EqualTo( orderFirstLineItem.total_tax ) );
		}

		private static void AssertOrderAmountsMapped( WooCommerceOrder result, Order order )
		{
			Assert.That( result.TotalTax, Is.EqualTo( order.total_tax ) );
			Assert.That( result.Total, Is.EqualTo( order.total ) );
			Assert.That( result.TotalDiscount, Is.EqualTo( order.discount_total ) );
		}

		private static void AssertCouponLinesMapped( WooCommerceCouponLine resultFirstCouponLine,
			OrderCouponLine orderFirstCouponLine )
		{
			Assert.That( resultFirstCouponLine.Code, Is.EqualTo( orderFirstCouponLine.code ) );
			Assert.That( resultFirstCouponLine.Amount, Is.EqualTo( orderFirstCouponLine.discount ) );
		}

		private static void AssertDatesMapped( WooCommerceOrder result, Order order )
		{
			Assert.That( result.CreateDateUtc, Is.EqualTo( order.date_created_gmt ) );
			Assert.That( result.UpdateDateUtc, Is.EqualTo( order.date_modified_gmt ) );
			Assert.That( result.PaidDateUtc, Is.EqualTo( order.date_paid_gmt ) );
			Assert.That( result.WasPaid, Is.EqualTo( order.date_paid_gmt != null ) );
		}
		
		private static void AssertShippingAddressMapped( WooCommerceShippingAddress resultShippingAddress, OrderShipping orderShipping )
		{
			Assert.That( resultShippingAddress.AddressLine, Is.EqualTo( orderShipping.address_1 ) );
			Assert.That( resultShippingAddress.AddressLine2, Is.EqualTo( orderShipping.address_2 ) );
			Assert.That( resultShippingAddress.City, Is.EqualTo( orderShipping.city ) );
			Assert.That( resultShippingAddress.CountryCode, Is.EqualTo( orderShipping.country ) );
			Assert.That( resultShippingAddress.PostCode, Is.EqualTo( orderShipping.postcode ) );
			Assert.That( resultShippingAddress.State, Is.EqualTo( orderShipping.state ) );
		}
	}
}