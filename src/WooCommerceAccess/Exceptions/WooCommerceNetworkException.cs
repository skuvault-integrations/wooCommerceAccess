using System;

namespace WooCommerceAccess.Exceptions
{
	public class WooCommerceNetworkException : WooCommerceException
	{
		public WooCommerceNetworkException( string message, Exception innerException ) : base( message, innerException) { }
	}
}
