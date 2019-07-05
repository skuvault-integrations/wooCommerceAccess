using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.Authentication
{
	public class OAuthAppCredentials
	{
		public string AppName { get; private set; }
		public string Scope { get; private set; }
		public string TenantId { get; private set; }
		public string ReturnUrl { get; private set; }
		public string CallbackUrl { get; private set; }

		public OAuthAppCredentials(string appName, string scope, string tenantId, string returnUrl, string callbackUrl)
		{
			Condition.Requires( appName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( scope, "scope" ).IsNotNullOrWhiteSpace();
			Condition.Requires( tenantId, "tenantId" ).IsNotNullOrWhiteSpace();
			Condition.Requires( returnUrl, "returnUrl" ).IsNotNullOrWhiteSpace();
			Condition.Requires( callbackUrl, "callbackUrl" ).IsNotNullOrWhiteSpace();

			this.AppName = appName;
			this.Scope = scope;
			this.TenantId = tenantId;
			this.ReturnUrl = returnUrl;
			this.CallbackUrl = callbackUrl;
		}
	}

	public class WooCommerceAuthenticationService
	{
		private readonly OAuthAppCredentials _appCredentials;
		private readonly WooCommerceConfig _config;
		private readonly HttpClient _httpClient;
		private Func< string > _additionalLogInfo;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func< string > AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public WooCommerceAuthenticationService( OAuthAppCredentials appCredentials, WooCommerceConfig config )
		{
			Condition.Requires( appCredentials, "appCredentials" ).IsNotNull();
			Condition.Requires( config, "config" ).IsNotNull();

			this._appCredentials = appCredentials;
			this._config = config;
			this._httpClient = new HttpClient()
			{
				BaseAddress = new Uri( this._config.ShopUrl )
			};
		}

		public async Task< string > GetAuthenticationHtmlForm()
		{
			var mark = Mark.CreateNew();

			var htmlForm = await new ActionPolicy( this._config.RetryAttempts )
						.ExecuteAsync(async () =>
							{
								var payload = new Dictionary<string, string>
								{
									{ "app_name", this._appCredentials.AppName },
									{ "scope", this._appCredentials.Scope },
									{ "user_id", this._appCredentials.TenantId },
									{ "return_url", this._appCredentials.ReturnUrl },
									{ "callback_url", this._appCredentials.CallbackUrl }
								};

								var httpResponse = await this._httpClient.PostAsync( WooCommerceEndPoint.AuthenticationUrl, new FormUrlEncodedContent( payload ) ).ConfigureAwait( false );
								string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

								return content;
							}, 
							( timeSpan, retryCount ) =>
							{
								string retryDetails = Misc.CreateMethodCallInfo( WooCommerceEndPoint.AuthenticationUrl, mark, additionalInfo: this.AdditionalLogInfo() );
								WooCommerceLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
							},
							() => Misc.CreateMethodCallInfo( WooCommerceEndPoint.AuthenticationUrl, mark, additionalInfo: this.AdditionalLogInfo() ),
							WooCommerceLogger.LogTraceException);

			return htmlForm;
		}
	}
}
