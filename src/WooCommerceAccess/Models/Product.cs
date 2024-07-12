using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using WooCommerceAccess.Shared;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Models
{
	public class WooCommerceProduct
	{
		public WooCommerceProduct()
		{
			this.Variations = new List< WooCommerceVariation >();
		}

		/// <summary>
		/// It can be either product_id or variation_id, depending on which matches the filter in the query
		/// </summary>
		public int? Id { get; set; }
		/// <summary>
		/// It can be a parent product id (if the entity is a variation of the product) or 0 (if this is product itself)
		/// </summary>
		public int ParentProductId { get; set; }
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
		public bool? ManagingStock { get; set; }
	}

	public static class ProductExtensions
	{
		public static WooCommerceProduct ToSvProduct( this WooCommerceNET.WooCommerce.v3.Product productV3 )
		{
			return new WooCommerceProduct()
			{
				Id = productV3.id,
				ParentProductId = productV3.parent_id ?? 0,
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
				Attributes = productV3.attributes?.ToAttributeDictionary(),
				HasVariations = productV3.variations != null && productV3.variations.Any(),
				CreatedDateUtc = productV3.date_created_gmt,
				UpdatedDateUtc = productV3.date_modified_gmt,
				ManagingStock = productV3.manage_stock
			};
		}

		public static Dictionary< string, string > ToAttributeDictionary( this List< ProductAttributeLine > attributeLines )
		{
			return attributeLines.Where( a => !string.IsNullOrWhiteSpace( a.name ) && a.options != null && a.options.Count > 0 )
				.DistinctBy( a => a.name )
				.ToDictionary( a => a.name, a => a.options.First() );
		}

		public static decimal ToDecimal( this string str )
		{
			decimal result;
			decimal.TryParse(str, out result);
			return result;
		}
	}
	
	public struct ProductId
	{
		public int Id { get; }

		public ProductId( int id )
		{
			Condition.Requires( id, "id" ).IsGreaterThan( 0 );
			Id = id;
		}
	}
}
