using System.Collections.Generic;
using System.Linq;

namespace WooCommerceAccess.Models
{
	public class QuantityUpdate
	{
		public int Id { get; }
		public string Sku { get; }
		public int Quantity { get; }
		public bool IsUpdateNeeded { get; }

		public QuantityUpdate() 
		{
		}

		public QuantityUpdate( WooCommerceVariation variation, Dictionary< string, int > skusQuantities ) 
		{
			this.IsUpdateNeeded = false;
			if ( variation.Id == null || string.IsNullOrWhiteSpace( variation.Sku ) ||
			    variation.ManagingStock == null || variation.ManagingStock != true ) 
				return;
			var quantityUpdate = skusQuantities.FirstOrDefault( s => s.Key.ToLower() == variation.Sku.ToLower() );
			var skuFound = !string.IsNullOrWhiteSpace( quantityUpdate.Key );
			if ( skuFound && ( variation.Quantity.HasValue 
				? quantityUpdate.Value != variation.Quantity : quantityUpdate.Value >= 0 ) )
			{
				IsUpdateNeeded = true;
				Id = variation.Id.Value;
				Sku = variation.Sku;
				Quantity = quantityUpdate.Value;
			}
		}

		public QuantityUpdate( WooCommerceProduct product, Dictionary< string, int > skusQuantities ) 
		{
			this.IsUpdateNeeded = false;
			if ( product.Id == null || string.IsNullOrWhiteSpace( product.Sku ) ||
			     product.ManagingStock == null || product.ManagingStock != true ) 
				return;
			var quantityUpdate = skusQuantities.FirstOrDefault( s => s.Key.ToLower() == product.Sku.ToLower() );
			var skuFound = !string.IsNullOrWhiteSpace( quantityUpdate.Key );
			if ( skuFound && ( product.Quantity.HasValue 
				? quantityUpdate.Value != product.Quantity : quantityUpdate.Value >= 0 ) )
			{
				IsUpdateNeeded = true;
				Id = product.Id.Value;
				Sku = product.Sku;
				Quantity = quantityUpdate.Value;
			}
		}
	}
}
