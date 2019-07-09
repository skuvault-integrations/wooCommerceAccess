using CuttingEdge.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services
{
	public class BaseService
	{
		protected readonly string ConsumerKey;
		protected readonly string ConsumerSecret;
		protected readonly WooCommerceConfig Config;
		protected readonly Throttler Throttler;

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
		}
	}
}
