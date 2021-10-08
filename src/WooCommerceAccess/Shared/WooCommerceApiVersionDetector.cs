using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WooCommerceAccess.Services;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Shared
{
	internal sealed class WooCommerceApiVersionDetector
	{
		private readonly string _shopUrl;
		private readonly int _retryAttempts;
		private readonly HttpClient _httpClient;
		private const string JsonApiV3DescriptionUrl = "/wp-json/wc/v3/";
		private const string ProductsLegacyApiV3Url = "/wc-api/v3/products";

		public WooCommerceApiVersionDetector( string shopUrl, int retryAttempts, string userAgentHeader )
		{
			Condition.Requires( shopUrl, "shopUrl" ).IsNotNullOrWhiteSpace();
			Condition.Requires( retryAttempts, "retryAttempts" ).IsGreaterThan( 0 );
			Condition.Requires( userAgentHeader, "userAgentHeader" ).IsNotNullOrWhiteSpace();

			this._shopUrl = shopUrl.TrimEnd( '/' );
			this._retryAttempts = retryAttempts;
			this._httpClient = new HttpClient();
			this._httpClient.DefaultRequestHeaders.Add( "User-Agent", userAgentHeader );
		}

		public async Task< WooCommerceApiVersion > DetectApiVersion()
		{
			if ( await this.TryGetV3JsonApiDescription().ConfigureAwait( false ) )
				return WooCommerceApiVersion.V3;
			else
			{
				if ( await this.TryGetEntitiesWithLegacyApiV3().ConfigureAwait( false ) )
					return WooCommerceApiVersion.Legacy;
			}
			
			return WooCommerceApiVersion.Unknown;
		}

		private async Task< bool > TryGetV3JsonApiDescription()
		{
			var mark = Mark.CreateNew();
			var url = this._shopUrl + JsonApiV3DescriptionUrl;

			try
			{
				return await new ActionPolicy( this._retryAttempts ).ExecuteAsync< bool >( async () =>
				{
					var response = await this._httpClient.GetAsync( url ).ConfigureAwait( false );

					if( response.StatusCode == System.Net.HttpStatusCode.OK )											
						return true;					
					
					WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, methodResult: response.ToJson() ) );
					return false;
				},
				( timeSpan, retryCount ) => { 
					string retryDetails = Misc.CreateMethodCallInfo( url, mark );
					WooCommerceLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
				},
				() => Misc.CreateMethodCallInfo( url, mark ),
				WooCommerceLogger.LogTraceException ).ConfigureAwait( false );
			}
			catch( Exception err )
			{
				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, errors: err.Message ) );
				return false;
			}
		}

		private async Task< bool > TryGetEntitiesWithLegacyApiV3()
		{
			var mark = Mark.CreateNew();
			var url = this._shopUrl + ProductsLegacyApiV3Url;

			try
			{
				return await new ActionPolicy( this._retryAttempts ).ExecuteAsync< bool >( async () =>
				{
					var response = await this._httpClient.GetAsync( url ).ConfigureAwait( false );
					var content = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
					
					var isLegacyApi = response.StatusCode == System.Net.HttpStatusCode.NotFound
							&& !string.IsNullOrWhiteSpace( content ) 
							&& content.Contains( "api_authentication_error" );

					if( isLegacyApi )
						return true;
					
					WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, methodResult: response.ToJson() ) );
					return false;					
				},
				( timeSpan, retryCount ) => { 
					string retryDetails = Misc.CreateMethodCallInfo( url, mark );
					WooCommerceLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
				},
				() => Misc.CreateMethodCallInfo( url, mark ),
				WooCommerceLogger.LogTraceException ).ConfigureAwait( false );
			}
			catch( Exception err )
			{
				WooCommerceLogger.LogTrace( Misc.CreateMethodCallInfo( url, mark, errors: err.Message ) );
				return false;
			}
		}
	}

	public enum WooCommerceApiVersion { Unknown, Legacy, V3 }
}
