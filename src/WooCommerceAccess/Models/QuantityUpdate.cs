using CuttingEdge.Conditions;

namespace WooCommerceAccess.Models
{
	public class QuantityUpdate
	{
		public int Id { get; }
		public string Sku { get; }
		public int Quantity { get; }

		public QuantityUpdate( int id, string sku, int quantity )
		{
			Condition.Requires( id, "id" ).IsGreaterThan( 0 );
			Condition.Requires( sku, "sku" ).IsNotNullOrEmpty();
			Condition.Requires( quantity, "quantity" ).IsGreaterOrEqual( 0 );

			this.Id = id;
			this.Sku = sku;
			this.Quantity = quantity;
		}
	}
}
