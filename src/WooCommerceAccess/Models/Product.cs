using System;
using System.Collections.Generic;
using System.Linq;
using WooCommerceNET.WooCommerce.Legacy;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Models
{
	public class WooCommerceProduct
	{
		public int? Id { get; set; }
		public string Sku { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ShortDescription { get; set; }
		public decimal? Price { get; set; }
		public int? Quantity { get; set; }
		public IEnumerable< string > Categories { get; set; }
		public IEnumerable< string > Images { get; set; }
		public decimal? Weight { get; set; }
		public decimal? SalePrice { get; set; }
		public decimal? RegularPrice { get; set; }
		public Dictionary< string, string > Attributes { get; set; }
		public IEnumerable< WooCommerceVariation > Variations { get; set; }
		public bool HasVariations { get; set; }
		public DateTime? CreatedDateUtc { get; set; }
		public DateTime? UpdatedDateUtc { get; set; }
	}

	public static class ProductExtensions
	{
		public static WooCommerceProduct ToSvProduct( this WooCommerceNET.WooCommerce.v3.Product productV3 )
		{
			return new WooCommerceProduct()
			{
				Id = productV3.id,
				Sku = productV3.sku,
				Name = productV3.name,
				Description = productV3.description,
				ShortDescription = productV3.short_description,
				Price = productV3.price,
				Quantity = productV3.stock_quantity,
				Categories = productV3.categories?.Select( c => c.name ),
				Images = productV3.images?.Select( i => i.src ),
				Weight = productV3.weight,
				SalePrice = productV3.sale_price,
				RegularPrice = productV3.regular_price,
				Attributes = productV3.attributes.ToAttributeDictionary(),
				HasVariations = productV3.variations.Any(),
				CreatedDateUtc = productV3.date_created_gmt,
				UpdatedDateUtc = productV3.date_modified_gmt
			};
		}

		public static WooCommerceProduct ToSvProduct( this WooCommerceNET.WooCommerce.Legacy.Product legacyProduct )
		{
			return new WooCommerceProduct()
			{
				Id = legacyProduct.id,
				Sku = legacyProduct.sku,
				Name = legacyProduct.name,
				Description = legacyProduct.description,
				ShortDescription = legacyProduct.short_description,
				Price = legacyProduct.price,
				Quantity = legacyProduct.stock_quantity,
				Categories = legacyProduct.categories?.Select( c => c.ToString() ),
				Images = legacyProduct.images?.Select( i => i.src ),
				Weight = legacyProduct.weight.ToDecimal(),
				SalePrice = legacyProduct.sale_price,
				RegularPrice = legacyProduct.regular_price,
				Attributes = legacyProduct.attributes.ToAttributeDictionary(),
				Variations = legacyProduct.variations?.Select( v => v.ToSvVariation() ),
				HasVariations = legacyProduct.variations != null && legacyProduct.variations.Any(),
				CreatedDateUtc = legacyProduct.created_at,
				UpdatedDateUtc = legacyProduct.updated_at
			};
		}

		public static Dictionary< string, string > ToAttributeDictionary( this AttributeList attributeList )
		{
			var attributeLines = attributeList.Select( a => new ProductAttributeLine
			{
				name = a.name,
				options = a.options
			} ).ToList();
			return attributeLines.ToAttributeDictionary();
		}

		public static Dictionary< string, string > ToAttributeDictionary( this List< ProductAttributeLine > attributeLines )
		{
			return attributeLines.Where( a => a.options != null && a.options.Count == 1 ).
				ToDictionary( a => a.name, a => a.options.First() );
		}

		public static decimal ToDecimal( this string str )
		{
			decimal result;
			decimal.TryParse(str, out result);
			return result;
		}
	}
}
