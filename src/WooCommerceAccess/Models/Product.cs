namespace WooCommerceAccess.Models
{
	public class Product
	{
		public int? Id { get; set; }
		public string Sku { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ShortDescription { get; set; }
		public decimal? Price { get; set; }
		public int? Quantity { get; set; }
	}

	public static class ProductExtensions
	{
		public static Product ToProduct( this WooCommerceNET.WooCommerce.v3.Product productV3 )
		{
			return new Product()
			{
				Id = productV3.id,
				Sku = productV3.sku,
				Name = productV3.name,
				Description = productV3.description,
				ShortDescription = productV3.short_description,
				Price = productV3.price,
				Quantity = productV3.stock_quantity
			};
		}
	}
}
