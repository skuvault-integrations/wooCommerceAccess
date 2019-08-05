using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WooCommerceAccess.Models;
using WooCommerceNET.WooCommerce.Legacy;
using WooCommerceNET.WooCommerce.v3;
using Attribute = WooCommerceNET.WooCommerce.Legacy.Attribute;
using Product = WooCommerceNET.WooCommerce.v3.Product;
using Variation = WooCommerceNET.WooCommerce.Legacy.Variation;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class ProductMapperTests
	{
		[ Test ]
		public void MapApiV3ProductToSvProduct()
		{
			var product = new Product
			{
				id = 1,
				sku = "testsku",
				name = "asdf",
				description = "alsdfkj",
				short_description = "flaskdfjsdlakfj",
				price = 12.30M,
				stock_quantity = 23,
				categories = new List< ProductCategoryLine >
				{
					new ProductCategoryLine
					{
						name = "bob"
					}
				},
				images = new List< ProductImage >
				{
					new ProductImage
					{
						src = "some Url"
					},
					new ProductImage
					{
						src = "another Url"
					}
				},
				weight = 2.1M,
				sale_price = 1.2M,
				regular_price = 2.1M,
				attributes = new List< ProductAttributeLine >
				{
					new ProductAttributeLine
					{
						name = "color",
						options = new List< string >
						{
							"red"
						}
					},
					new ProductAttributeLine
					{
						name = "size",
						options = new List< string >
						{
							"small",
							"large"
						}
					}
				},
				variations = new List< int >
				{
					1,
					2
				},
				date_created_gmt = new DateTime( 2099, 1, 1 ),
				date_modified_gmt = new DateTime( 2099, 11, 11 )
			};

			var svProduct = product.ToSvProduct();

			Assert.AreEqual( product.id, svProduct.Id );
			Assert.AreEqual( product.sku, svProduct.Sku );
			Assert.AreEqual( product.name, svProduct.Name );
			Assert.AreEqual( product.description, svProduct.Description );
			Assert.AreEqual( product.short_description, svProduct.ShortDescription );
			Assert.AreEqual( product.price, svProduct.Price );
			Assert.AreEqual( product.stock_quantity, svProduct.Quantity );
			Assert.AreEqual( product.categories[0].name, svProduct.Categories.First() );
			Assert.AreEqual( product.images.Count, svProduct.Images.Count() );
			Assert.AreEqual( product.images[0].src, svProduct.Images.First() );
			Assert.AreEqual( product.weight, svProduct.Weight );
			Assert.AreEqual( product.sale_price, svProduct.SalePrice );
			Assert.AreEqual( product.regular_price, svProduct.RegularPrice );
			Assert.AreEqual( 1, svProduct.Attributes.Count );
			Assert.AreEqual( product.attributes[0].name, svProduct.Attributes.First().Key );
			Assert.AreEqual( product.attributes[0].options.First(), svProduct.Attributes.First().Value );
			Assert.AreEqual( product.variations.Any(), svProduct.HasVariations );
			Assert.AreEqual( product.date_created_gmt, svProduct.CreatedDateUtc );
			Assert.AreEqual( product.date_modified_gmt, svProduct.UpdatedDateUtc );
		}

		[ Test ]
		public void MapLegacyV3ProductToSvProduct()
		{
			var legacyProduct = new WooCommerceNET.WooCommerce.Legacy.Product
			{
				id = 1,
				sku = "testsku",
				title = "asdf",
				description = "alsdfkj",
				short_description = "flaskdfjsdlakfj",
				price = 12.30M,
				stock_quantity = 23,
				categories = new List< object >
				{
					"asdf",
					"fdas"
				},
				images = new ImageList
				{
					new Image
					{
						src = "www.yes.com"
					},
					new Image
					{
						src = "www.maybe.com"
					}
				},
				weight = "2.1",
				sale_price = 1.2M,
				regular_price = 2.1M,
				attributes = new AttributeList
				{
					new Attribute
					{
						name = "color",
						options = new List< string >
						{
							"cyan"
						}
					},
					new Attribute
					{
						name = "size",
						options = new List< string >
						{
							"small",
							"large"
						}
					}
				},
				variations = new VariationList
				{
					variations = new List< Variation >
					{
						new Variation
						{
							sku = "some sku",
							stock_quantity = 12
						}
					}
				},
				created_at = new DateTime( 2099, 1, 1 ),
				updated_at = new DateTime( 2099, 11, 11 )
			};
			
			var svProduct = legacyProduct.ToSvProduct();

			Assert.AreEqual( legacyProduct.id, svProduct.Id );
			Assert.AreEqual( legacyProduct.sku, svProduct.Sku );
			Assert.AreEqual( legacyProduct.title, svProduct.Name );
			Assert.AreEqual( legacyProduct.description, svProduct.Description );
			Assert.AreEqual( legacyProduct.short_description, svProduct.ShortDescription );
			Assert.AreEqual( legacyProduct.price, svProduct.Price );
			Assert.AreEqual( legacyProduct.stock_quantity, svProduct.Quantity );
			Assert.AreEqual( legacyProduct.categories[0].ToString() , svProduct.Categories.First() );
			Assert.AreEqual( legacyProduct.images.Count , svProduct.Images.Count() );
			Assert.AreEqual( legacyProduct.images[0].src , svProduct.Images.First() );
			Assert.AreEqual( decimal.Parse( legacyProduct.weight ) , svProduct.Weight );
			Assert.AreEqual( legacyProduct.sale_price , svProduct.SalePrice );
			Assert.AreEqual( legacyProduct.regular_price , svProduct.RegularPrice );
			Assert.AreEqual( 1 , svProduct.Attributes.Count );
			Assert.AreEqual( legacyProduct.attributes[0].name , svProduct.Attributes.First().Key );
			Assert.AreEqual( legacyProduct.attributes[0].options.First() , svProduct.Attributes.First().Value );
			Assert.AreEqual( legacyProduct.variations.Any() , svProduct.Variations.Any() );
			Assert.AreEqual( legacyProduct.created_at , svProduct.CreatedDateUtc );
			Assert.AreEqual( legacyProduct.updated_at , svProduct.UpdatedDateUtc );
		}
	}
}
