using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WooCommerceAccess.Models;
using WooCommerceNET.WooCommerce.v3;
using Product = WooCommerceNET.WooCommerce.v3.Product;

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
				date_created_gmt_value = "2099/01/01",
				date_modified_gmt_value = "2099/11/11"
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
			Assert.AreEqual( 2, svProduct.Attributes.Count );
			Assert.AreEqual( product.attributes[0].name, svProduct.Attributes.First().Key );
			Assert.AreEqual( product.attributes[0].options.First(), svProduct.Attributes.First().Value );
			Assert.AreEqual( product.variations.Any(), svProduct.HasVariations );
			Assert.AreEqual( product.date_created_gmt, svProduct.CreatedDateUtc );
			Assert.AreEqual( product.date_modified_gmt, svProduct.UpdatedDateUtc );
		}
	}
}
