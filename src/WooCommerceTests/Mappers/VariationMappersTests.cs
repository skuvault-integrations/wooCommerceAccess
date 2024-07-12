using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WooCommerceAccess.Models;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class VariationMappersTests
	{
		[ Test ]
		public void MapApiV3VariationToSvVariation()
		{
			var variation = new WooCommerceNET.WooCommerce.v3.Variation
			{
				id = 3,
				sku = "somesku",
				price = 23.4M,
				stock_quantity = 2,
				image = new VariationImage
				{
					src = "asdlf"
				},
				weight = 22.3M,
				sale_price = 2.32M,
				regular_price = 1.38M,
				attributes = new List< VariationAttribute >
				{
					new VariationAttribute
					{
						name = "size",
						option = "small"
					}
				},
				date_modified_gmt = new DateTime( 2099, 3, 3 ),
				date_created_gmt = new DateTime( 2099, 3, 4 ),
				manage_stock = false,
				description = "old macdonald had a dragon"
			};

			var svVariation = variation.ToSvVariation();

			Assert.AreEqual( variation.id, svVariation.Id );
			Assert.AreEqual( variation.sku, svVariation.Sku );
			Assert.AreEqual( variation.price, svVariation.Price );
			Assert.AreEqual( variation.stock_quantity, svVariation.Quantity );
			Assert.AreEqual( variation.image.src, svVariation.Images.First() );
			Assert.AreEqual( variation.weight, svVariation.Weight );
			Assert.AreEqual( variation.sale_price, svVariation.SalePrice );
			Assert.AreEqual( variation.regular_price, svVariation.RegularPrice );
			Assert.AreEqual( variation.attributes.Count, svVariation.Attributes.Count );
			Assert.AreEqual( variation.attributes[0].name, svVariation.Attributes.First().Key );
			Assert.AreEqual( variation.attributes[0].option, svVariation.Attributes.First().Value );
			Assert.AreEqual( variation.date_modified_gmt, svVariation.UpdatedDateUtc );
			Assert.AreEqual( variation.date_created_gmt, svVariation.CreatedDateUtc );
			Assert.AreEqual( variation.manage_stock, svVariation.ManagingStock );
			Assert.AreEqual( variation.description, svVariation.Description );
		}

		[Test]
		public void ToSvVariation_FirstAttributeIsUsed_WhenPassedAttributesWithTheSameNames()
		{
			// Arrange
			var variation = new WooCommerceNET.WooCommerce.v3.Variation
			{
				id = 3,
				sku = "somesku",
				attributes = new List<VariationAttribute>
				{
					new VariationAttribute
					{
						name = "size",
						option = "small"
					},
					new VariationAttribute
					{
						name = "size",
						option = "big"
					},
					new VariationAttribute
					{
						name = "color",
						option = "blue"
					}
				}
			};

			// Act
			var svVariation = variation.ToSvVariation();

			// Assert
			var svVariationAttributes = svVariation.Attributes.ToArray();
			Assert.AreEqual(variation.id, svVariation.Id);
			Assert.AreEqual(variation.sku, svVariation.Sku);
			Assert.AreEqual(2, svVariationAttributes.Length);
			Assert.AreEqual(variation.attributes[0].name, svVariationAttributes[0].Key);
			Assert.AreEqual(variation.attributes[0].option, svVariationAttributes[0].Value);
			Assert.AreEqual(variation.attributes[2].name, svVariationAttributes[1].Key);
			Assert.AreEqual(variation.attributes[2].option, svVariationAttributes[1].Value);
		}
	}
}
