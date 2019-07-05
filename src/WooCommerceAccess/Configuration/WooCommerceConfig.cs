using CuttingEdge.Conditions;

namespace WooCommerceAccess.Configuration
{
	public class WooCommerceConfig
	{
		public string ShopUrl { get; private set; }

		/// <summary>
		///	Request timeout
		/// </summary>
		public readonly int RequestTimeoutMs;

		/// <summary>
		///	Max retry attempts if we get network errors ( total time for attempts over 14 seconds)
		/// </summary>
		public readonly int RetryAttempts = 10;

		/// <summary>
		///	Throttling, max requests per interval
		/// </summary>
		public readonly int ThrottlingMaxRequestsPerRestoreInterval = 10;

		/// <summary>
		///	Throttling time interval in seconds
		/// </summary>
		public readonly int ThrottlingRestorePeriodInSeconds = 1;

		public WooCommerceConfig( string shopUrl )
		{
			Condition.Requires( shopUrl, "shopUrl" );

			this.ShopUrl = shopUrl;
		}
	}

	public static class WooCommerceEndPoint
	{
		public static readonly string AuthenticationUrl = "/wc-auth/v1/authorize";
	}
}
