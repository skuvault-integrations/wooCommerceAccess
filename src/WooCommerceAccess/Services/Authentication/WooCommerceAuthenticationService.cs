using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Shared;

[ assembly: InternalsVisibleTo( "WooCommerceTests" ) ]

namespace WooCommerceAccess.Services.Authentication
{
	public class WooCommerceOAuthAppCredentials
	{
		public string AppName { get; private set; }
		public string Scope { get; private set; }
		public string ReturnUrl { get; private set; }
		public string CallbackUrl { get; private set; }

		public WooCommerceOAuthAppCredentials( string appName, string scope, string returnUrl, string callbackUrl )
		{
			Condition.Requires( appName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( scope, "scope" ).IsNotNullOrWhiteSpace();
			Condition.Requires( returnUrl, "returnUrl" ).IsNotNullOrWhiteSpace();
			Condition.Requires( callbackUrl, "callbackUrl" ).IsNotNullOrWhiteSpace();

			this.AppName = appName;
			this.Scope = scope;
			this.ReturnUrl = returnUrl;
			this.CallbackUrl = callbackUrl;
		}
	}

	public class WooCommerceAuthenticationService
	{
		private readonly WooCommerceOAuthAppCredentials _appCredentials;
		private readonly WooCommerceConfig _config;

		public WooCommerceAuthenticationService( WooCommerceOAuthAppCredentials appCredentials, WooCommerceConfig config )
		{
			Condition.Requires( appCredentials, "appCredentials" ).IsNotNull();
			Condition.Requires( config, "config" ).IsNotNull();

			this._appCredentials = appCredentials;
			this._config = config;
		}

		public string GetAuthenticationUrl( long tenantId )
		{
			var requestParameters = new Dictionary< string, string >
								{
									{ "app_name", this._appCredentials.AppName },
									{ "scope", this._appCredentials.Scope },
									{ "user_id", tenantId.ToString() },
									{ "return_url", this._appCredentials.ReturnUrl },
									{ "callback_url", this._appCredentials.CallbackUrl }
								};
			string encodedParameters = string.Join( "&", requestParameters.Select( item => $"{ item.Key }={ Misc.EscapeUriData( item.Value ) }" ) );

			return WooCommerceEndPoint.AuthenticationUrl + "?" + encodedParameters;
		}

		internal async Task< string > GetAuthenticationHtmlForm( long tenantId )
		{
			var httpClient = new HttpClient()
			{
				BaseAddress = new Uri( this._config.ShopUrl )
			};
			var url = this.GetAuthenticationUrl( tenantId );
			var httpResponse = await httpClient.GetAsync( url ).ConfigureAwait( false );
			return await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );
		}
	}
}
