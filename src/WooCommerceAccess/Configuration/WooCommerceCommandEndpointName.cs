namespace WooCommerceAccess.Configuration
{
	public class WooCommerceCommandEndpointName
	{
		public static readonly WooCommerceCommandEndpointName Page = new WooCommerceCommandEndpointName( "page" );
		public static readonly WooCommerceCommandEndpointName PerPage = new WooCommerceCommandEndpointName( "per_page" );

		private WooCommerceCommandEndpointName( string name )
		{
			this.Name = name;
		}

		public string Name { get; private set; }
	}
}