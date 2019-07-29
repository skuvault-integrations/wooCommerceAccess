using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WooCommerceAccess.Models;
using WooCommerceNET.WooCommerce.Legacy;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class VariationMappersTests
	{
		[ Test ]
		public void MapLegacyV3VariationToSvVariation()
		{
			var variation = new WooCommerceNET.WooCommerce.Legacy.Variation
			{
				sku = "sku2",
				price = 23.4M,
				stock_quantity = 2,
				image = new ImageList
				{
					new Image
					{
						src = "asdlf"
					}
				},
				weight = "22.3",
				sale_price = 2.32M,
				regular_price = 1.38M,
				attributes = new AttributeList
				{
					new Attribute
					{
						name = "size",
						options = new List<string>
						{
							"small"
						}
					},
					new Attribute
					{
						name = "color",
						options = new List<string>
						{
							"red",
							"black"
						}
					}
				}
			};

			var svVariation = variation.ToSvVariation();

			Assert.AreEqual( variation.sku, svVariation.Sku  );
			Assert.AreEqual( variation.price, svVariation.Price  );
			Assert.AreEqual( variation.stock_quantity, svVariation.Quantity  );
			Assert.AreEqual( variation.image.Count, svVariation.Images.Count() );
			Assert.AreEqual( variation.image[0].src, svVariation.Images.First() );
			Assert.AreEqual( decimal.Parse( variation.weight ), svVariation.Weight );
			Assert.AreEqual( 1, svVariation.Attributes.Count );
			Assert.AreEqual( variation.attributes[0].name, svVariation.Attributes.First().Key );
			Assert.AreEqual( variation.attributes[0].options.First(), svVariation.Attributes.First().Value );
		}

		[ Test ]
		public void MapApiV3VariationToSvVariation()
		{
			var variation = new WooCommerceNET.WooCommerce.v3.Variation
			{
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
				}
			};

			var svVariation = variation.ToSvVariation();

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
		}
	}
}
