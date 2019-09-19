using CuttingEdge.Conditions;

namespace WooCommerceAccess.Configuration
{
	public class WooCommerceConfig
	{
		public string ShopUrl { get; private set; }
		public string ConsumerKey { get; private set; }
		public string ConsumerSecret { get; private set; }

		/// <summary>
		///	Request timeout
		/// </summary>
		public readonly int RequestTimeoutMs;

		/// <summary>
		///	Max retry attempts if we get network errors ( total time for attempts over 14 seconds)
		/// </summary>
		public readonly int RetryAttempts = 20;

		/// <summary>
		///	Throttling, max requests per interval
		/// </summary>
		public readonly int ThrottlingMaxRequestsPerRestoreInterval = 10;

		/// <summary>
		///	Throttling time interval in seconds
		/// </summary>
		public readonly int ThrottlingRestorePeriodInSeconds = 1;

		/// <summary>
		///	Default page size for products
		/// </summary>
		public int ProductsPageSize = 10;

    /// <summary>
		///	Default page size for orders
		/// </summary>
		public int OrdersPageSize = 10;

		public WooCommerceConfig( string shopUrl, string consumerKey, string consumerSecret )
		{
			Condition.Requires( shopUrl, "shopUrl" ).IsNotNullOrWhiteSpace();
			Condition.Requires( consumerKey, "consumerKey" ).IsNotNullOrWhiteSpace();
			Condition.Requires( consumerSecret, "consumerSecret" ).IsNotNullOrWhiteSpace();

			this.ShopUrl = shopUrl;
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
		}

		public WooCommerceConfig( string shopUrl )
		{
			Condition.Requires( shopUrl, "shopUrl" ).IsNotNullOrWhiteSpace();
			this.ShopUrl = shopUrl;
		}
	}

	public static class WooCommerceEndPoint
	{
		public static readonly string AuthenticationUrl = "/wc-auth/v1/authorize";
	}
}
