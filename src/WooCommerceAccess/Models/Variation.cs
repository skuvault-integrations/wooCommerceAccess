using System.Collections.Generic;
using System.Linq;

namespace WooCommerceAccess.Models
{
	public class WooCommerceVariation
	{
		public string Sku { get; set; }
		public string Description { get; set; }
		public decimal? Price { get; set; }
		public int? Quantity { get; set; }
		public IEnumerable<string> Images { get; set; }
		public decimal? Weight { get; set; }
		public decimal? SalePrice { get; set; }
		public decimal? RegularPrice { get; set; }
		public Dictionary< string, string > Attributes { get; set; }
	}

	public static class VariationExtensions
	{
		public static WooCommerceVariation ToSvVariation( this WooCommerceNET.WooCommerce.Legacy.Variation legacyVariation )
		{
			return new WooCommerceVariation
			{
				Sku = legacyVariation.sku,
				Description = "",
				Price = legacyVariation.price,
				Quantity = legacyVariation.stock_quantity,
				Images = legacyVariation.image?.Select( i => i.src ),
				Weight = legacyVariation.weight?.ToDecimal(),
				SalePrice = legacyVariation.sale_price,
				RegularPrice = legacyVariation.regular_price,
				Attributes = legacyVariation.attributes?.ToAttributeDictionary()
			};
		}

		public static WooCommerceVariation ToSvVariation( this WooCommerceNET.WooCommerce.v3.Variation variationV3 )
		{
			var images = new List<string>();
			if (variationV3.image != null)
			{
				images.Add( variationV3.image.src );
			}

			return new WooCommerceVariation
			{
				Sku = variationV3.sku,
				Description = "",
				Price = variationV3.price,
				Quantity = variationV3.stock_quantity,
				Images = images,
				Weight = variationV3.weight,
				SalePrice = variationV3.sale_price,
				RegularPrice = variationV3.regular_price,
				Attributes = variationV3.attributes?.ToDictionary( a => a.name, a => a.option )
			};
		}
	}
}
