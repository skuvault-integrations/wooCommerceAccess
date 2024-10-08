using System.Collections.Generic;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models.Configuration;

namespace WooCommerceAccess.Services
{
	public static class EndpointsBuilder
	{
		public static Dictionary< string, string > CreateGetPageAndLimitFilter( WooCommerceCommandConfig config )
		{
			return new Dictionary< string, string >
			{
				{ WooCommerceCommandEndpointName.Page.Name, config.Page.ToString() },
				{ WooCommerceCommandEndpointName.PerPage.Name, config.PerPage.ToString() },
				{ WooCommerceCommandEndpointName.OrderBy.Name, "id" } // added sorting by id as a temporarily fix for know wooCommerce issue https://github.com/woocommerce/woocommerce/issues/32715
			};
		}
	}
}