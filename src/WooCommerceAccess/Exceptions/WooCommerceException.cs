using System;
using System.Collections.Generic;
using System.Text;

namespace WooCommerceAccess.Exceptions
{
	public class WooCommerceException : Exception
	{
		public WooCommerceException( string message, Exception innerException ) : base( message, innerException ) { }
		public WooCommerceException( string message ) : this ( message, null) { }
	}
}
