using CuttingEdge.Conditions;

namespace WooCommerceAccess.Models.Configuration
{
	public class WooCommerceCommandConfig
	{
		public int Page { get; private set; }
		public int PerPage { get; private set; }

		public WooCommerceCommandConfig( int page, int perPage )
			: this( page )
		{
			Condition.Requires( page, "page" ).IsGreaterThan( 0 );
			Condition.Requires( perPage, "per_page" ).IsGreaterThan( 0 );

			this.PerPage = perPage;
		}

		public WooCommerceCommandConfig( int page )
		{
			Condition.Requires( page, "page" ).IsGreaterThan( 0 );

			this.Page = page;
		}
	}
}