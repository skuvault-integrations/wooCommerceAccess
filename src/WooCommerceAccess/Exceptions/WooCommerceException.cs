using System;

namespace WooCommerceAccess.Exceptions
{
	public class WooCommerceException : Exception
	{
		public WooCommerceException( string message, Exception innerException ) : base( message, innerException ) { }
		public WooCommerceException( string message ) : this ( message, null) { }
	}
}
