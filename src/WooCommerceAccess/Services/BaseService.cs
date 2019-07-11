using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Exceptions;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerceAccess.Services
{
	public class BaseService
	{
		protected readonly string ConsumerKey;
		protected readonly string ConsumerSecret;
		protected readonly WooCommerceConfig Config;
		protected readonly Throttler Throttler;
		protected readonly WCObject WCObject;
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

			// TODO: detect working API version
			var restApi = new RestAPI( config.ShopUrl + "wp-json/wc/v3/", consumerKey, consumerSecret );
			this.WCObject = new WCObject( restApi );
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
