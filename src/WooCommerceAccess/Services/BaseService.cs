using CuttingEdge.Conditions;
using System;
using System.Runtime.CompilerServices;
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
		protected readonly WooCommerceConfig Config;
		protected readonly Throttler Throttler;
		protected IWCObject WCObject { get; private set; }
		protected Func< string > _additionalLogInfo;
		protected WooCommerceApiVersion apiVersion { get; private set; }

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func< string > AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( WooCommerceConfig config, Throttler throttler )
		{
			Condition.Requires( config, "config" ).IsNotNull();
			Condition.Requires( throttler, "throttler" ).IsNotNull();

			this.Config = config;
			this.Throttler = throttler;

			this.InitWcObject();
		}

		private void InitWcObject()
		{
			apiVersion = new WooCommerceApiVersionDetector( this.Config.ShopUrl, this.Config.RetryAttempts, this.Config.DefaultUserAgentHeader ).DetectApiVersion().Result;
			
			if ( apiVersion == WooCommerceApiVersion.Unknown )
				throw new WooCommerceException( "Unsupported WordPress and WooCommerce version!" );
			
			var legacyApiWcObject = new LegacyV3WCObject( new RestAPI( this.Config.ShopUrl + "wc-api/v3", this.Config.ConsumerKey, this.Config.ConsumerSecret, authorizedHeader: false ) );
				
			if ( apiVersion == WooCommerceApiVersion.V3 )
				this.WCObject = new ApiV3WCObject( new RestAPI( this.Config.ShopUrl + "wp-json/wc/v3/", this.Config.ConsumerKey, this.Config.ConsumerSecret, authorizedHeader: false ), legacyApiWcObject );
			else
				this.WCObject = legacyApiWcObject;
		}

		protected Task< T > SendRequestAsync< T >( string url, Mark mark, Func< string, Mark, Task< T > > processor )
		{
			return Throttler.ExecuteAsync( () =>
			{
				return new ActionPolicy( Config.RetryAttempts )
						.ExecuteAsync( async () => {
							WooCommerceLogger.LogStarted( Misc.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );
							var entity = await processor( url, mark ).ConfigureAwait( false );
							WooCommerceLogger.LogEnd( Misc.CreateMethodCallInfo (url, mark, methodResult: entity.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );

							return entity;
						},
						( timeSpan, retryCount ) =>
						{
							string retryDetails = Misc.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							WooCommerceLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => 
						Misc.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						WooCommerceLogger.LogTraceException );
			} );
		}
	}
}
