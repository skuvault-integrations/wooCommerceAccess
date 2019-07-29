using System.Collections.Generic;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models.Configuration;

namespace WooCommerceAccess.Services
{
	public static class EndpointsBuilder
	{
		public static Dictionary< string, string > CreateGetPageFilter( WooCommerceCommandConfig config )
		{
			return new Dictionary< string, string >
			{
				{ WooCommerceCommandEndpointName.Page.Name, config.Page.ToString() }
			};
		}

		public static Dictionary< string, string > CreateGetPageAndLimitFilter( WooCommerceCommandConfig config )
		{
			return new Dictionary< string, string >
			{
				{ WooCommerceCommandEndpointName.Page.Name, config.Page.ToString() },
				{ WooCommerceCommandEndpointName.PerPage.Name, config.PerPage.ToString() },
			};
		}

		public static Dictionary< string, string > CreateLegacyApiV3GetPageAndLimitFilter( WooCommerceCommandConfig config )
		{
			return new Dictionary< string, string >
			{
				{ WooCommerceCommandEndpointName.Page.Name, config.Page.ToString() },
				{ WooCommerceCommandEndpointName.PerPageLegacyApiV3.Name, config.PerPage.ToString() },
			};
		}
	}
}