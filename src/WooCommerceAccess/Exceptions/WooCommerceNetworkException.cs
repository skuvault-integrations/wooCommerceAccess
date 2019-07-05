using System;
using System.Collections.Generic;
using System.Text;

namespace WooCommerceAccess.Exceptions
{
	public class WooCommerceNetworkException : WooCommerceException
	{
		public WooCommerceNetworkException( string message, Exception innerException ) : base( message, innerException) { }
	}
}
