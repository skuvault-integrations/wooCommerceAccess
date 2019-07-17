using CuttingEdge.Conditions;
using System;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Exceptions;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;
using WooCommerceNET;

namespace WooCommerceAccess.Services
{
	public class BaseService
	{
		protected readonly string ConsumerKey;
		protected readonly string ConsumerSecret;
		protected readonly WooCommerceConfig Config;
		protected readonly Throttler Throttler;
		protected IWCObject WCObject { get; private set; }
		protected Func< string > _additionalLogInfo;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func< string > AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( string consumerKey, string consumerSecret, WooCommerceConfig config, Throttler throttler )
		{
			Condition.Requires( consumerKey, "consumerKey" );
			Condition.Requires( consumerSecret, "consumerSecret" );
			Condition.Requires( config, "config" ).IsNotNull();
			Condition.Requires( throttler, "throttler" ).IsNotNull();

			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
			this.Config = config;
			this.Throttler = throttler;

			this.InitWcObject();
		}

		private void InitWcObject()
		{
			var apiVersion = new WooCommerceApiVersionDetector( this.Config.ShopUrl, this.Config.RetryAttempts ).DetectApiVersion().Result;
			
			if ( apiVersion == WooCommerceApiVersion.Unknown )
				throw new WooCommerceException("Unsupported WordPress and WooCommerce version!");

			string apiUrl = apiVersion == WooCommerceApiVersion.V3 ? "wp-json/wc/v3/" : "wc-api/v3";
			var restApi = new RestAPI( this.Config.ShopUrl + apiUrl, this.ConsumerKey, this.ConsumerSecret );
			
			if ( apiVersion == WooCommerceApiVersion.Legacy )
				this.WCObject = new LegacyV3WCObject( restApi );
			else
				this.WCObject = new ApiV3WCObject( restApi );
		}

		protected Task< T > SendRequestAsync< T >( string url, Func< Task< T > > processor )
		{
			var mark = Mark.CreateNew();

			return Throttler.ExecuteAsync( () =>
			{
				return new ActionPolicy( Config.RetryAttempts )
						.ExecuteAsync( async () => {
						      WooCommerceLogger.LogStarted( Misc.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );
							var entity = await processor().ConfigureAwait( false );
							WooCommerceLogger.LogEnd( Misc.CreateMethodCallInfo (url, mark, methodResult: entity.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );

							return entity;
					     },
						( timeSpan, retryCount ) =>
						{
							string retryDetails = Misc.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							WooCommerceLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => Misc.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						WooCommerceLogger.LogTraceException );
			} );
		}
	}
}
