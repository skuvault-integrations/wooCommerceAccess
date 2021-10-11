using System;
using System.Collections.Generic;
using System.Linq;
using WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerceAccess.Models
{
	public class WooCommerceOrder
	{
		public string Id { get; set; }
		public string Number { get; set; }
		public DateTime? CreateDateUtc { get; set; }
		public DateTime? UpdateDateUtc { get; set; }
		public string Status { get; set; }
		public string Currency { get; set; }
		public decimal Total { get; set; }
		public string Note { get; set; }
		public bool WasPaid { get; set; }
		public DateTime? PaidDateUtc { get; set; }
		public WooCommerceShippingInfo ShippingInfo { get; set; }
		public WooCommerceShippingAddress ShippingAddress { get; set; }
		public WooCommerceBuyerInfo BuyerInfo { get; set; }
		public WooCommerceOrderItem[] Items { get; set; }
		public decimal? TotalDiscount { get; set; }
		public IEnumerable< WooCommerceCouponLine > Coupons { get; set; }
		public decimal? TotalTax { get; set; }
	}

	public class WooCommerceCouponLine
	{
		public string Code { get; set; }
		public decimal? Amount { get; set; }
	}

	public class WooCommerceOrderItem
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string Sku { get; set; }
		public decimal? TotalTax { get; set; }
	}

	public class WooCommerceShippingAddress
	{
		public string AddressLine { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string PostCode { get; set; }
		public string CountryCode { get; set; }
	}

	public class WooCommerceShippingInfo
	{
		public string Common { get; set; }
		public decimal ShippingCost { get; set; }
		public string ShippingClass { get; set; }
	}

	public class WooCommerceBuyerInfo
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Company { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
	}

	public static class OrderExtensions
	{
		public static WooCommerceOrder ToSvOrder( this WooCommerceNET.WooCommerce.Legacy.Order legacyOrder )
		{
			var order = new WooCommerceOrder()
			{
				Id = legacyOrder.id,
				Number = legacyOrder.order_number.ToString(),
				CreateDateUtc = legacyOrder.created_at,
				UpdateDateUtc = legacyOrder.updated_at,
				Status = legacyOrder.status,
				Currency = legacyOrder.currency,
				Total = legacyOrder.total.Value,
				Note = legacyOrder.note,
				TotalDiscount = legacyOrder.total_discount,
				Coupons = legacyOrder.coupon_lines?.Select( c => new WooCommerceCouponLine { Code = c.code, Amount = c.amount } ),
				TotalTax = legacyOrder.total_tax
			};

			if ( legacyOrder.payment_details != null )
			{
				order.WasPaid = legacyOrder.payment_details.paid.Value;
			}

			order.ShippingInfo = new WooCommerceShippingInfo() { ShippingCost = legacyOrder.total_shipping.Value };
			if ( legacyOrder.shipping_lines != null )
				order.ShippingInfo.Common = string.Join( ",", legacyOrder.shipping_lines.Select( line => line.method_title ) );

			if ( legacyOrder.shipping_address != null )
				order.ShippingAddress = new WooCommerceShippingAddress()
				{
					AddressLine = legacyOrder.shipping_address.address_1,
					AddressLine2 = legacyOrder.shipping_address.address_2,
					City = legacyOrder.shipping_address.city,
					CountryCode = legacyOrder.shipping_address.country,
					PostCode = legacyOrder.shipping_address.postcode,
					State = legacyOrder.shipping_address.state
				};

			if ( legacyOrder.billing_address != null )
				order.BuyerInfo = new WooCommerceBuyerInfo()
				{
					Company = legacyOrder.billing_address.company,
					FirstName = legacyOrder.billing_address.first_name,
					LastName = legacyOrder.billing_address.last_name,
					Email = legacyOrder.billing_address.email,
					Phone = legacyOrder.billing_address.phone
				};

			var items = new List< WooCommerceOrderItem >();
			foreach( var lineItem in legacyOrder.line_items )
				items.Add( new WooCommerceOrderItem()
				{
					Id = lineItem.id.Value,
					Sku = lineItem.sku,
					Quantity = lineItem.quantity.Value,
					Price = lineItem.price.Value,
					TotalTax = lineItem.total_tax
				} );

			order.Items = items.ToArray();

			return order;
		}

		public static WooCommerceOrder ToSvOrder( this WooCommerceNET.WooCommerce.v3.Order orderV3 )
		{
			var order = new WooCommerceOrder()
			{
				Id = orderV3.id,
				Number = orderV3.order_key,
				CreateDateUtc = orderV3.date_created_gmt,
				UpdateDateUtc = orderV3.date_modified_gmt,
				Status = orderV3.status,
				Currency = orderV3.currency,
				Total = orderV3.total.Value,
				Note = orderV3.customer_note,
				WasPaid = orderV3.date_paid_gmt != null,
				PaidDateUtc = orderV3.date_paid_gmt
			};

			if ( orderV3.shipping != null )
				order.ShippingAddress = new WooCommerceShippingAddress()
				{
					AddressLine = orderV3.shipping.address_1,
					AddressLine2 = orderV3.shipping.address_2,
					City = orderV3.shipping.city,
					CountryCode = orderV3.shipping.country,
					PostCode = orderV3.shipping.postcode,
					State = orderV3.shipping.state
				};
			
			order.ShippingInfo = new WooCommerceShippingInfo() { ShippingCost = orderV3.shipping_total.Value };
			if ( orderV3.shipping_lines != null )
			{
				order.ShippingInfo.Common = string.Join( ",", orderV3.shipping_lines.Select( line => line.method_title ) );
			}

			if ( orderV3.billing != null )
				order.BuyerInfo = new WooCommerceBuyerInfo()
				{
					Company = orderV3.billing.company,
					FirstName = orderV3.billing.first_name,
					LastName = orderV3.billing.last_name,
					Email = orderV3.billing.email,
					Phone = orderV3.billing.phone
				};

			var items = new List< WooCommerceOrderItem >();
			foreach( var lineItem in orderV3.line_items )
				items.Add( new WooCommerceOrderItem()
				{
					Id = lineItem.id.Value,
					ProductId = lineItem.product_id.Value,
					Sku = lineItem.sku,
					Quantity = int.Parse( lineItem.quantity.Value.ToString() ),
					Price = lineItem.price.Value
				} );

			order.Items = items.ToArray();

			return order;
		}
	}
}
