using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace WooCommerceAccess.Shared
{
	public static class Misc
	{
		public static string CreateMethodCallInfo( string url = "", Mark mark = null, string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			string serviceEndPoint = null;
			string requestParameters = null;

			if ( !string.IsNullOrEmpty( url ) )
			{
				Uri uri = new Uri( url );

				serviceEndPoint = uri.LocalPath;
				requestParameters = uri.Query;
			}

			var str = string.Format(
				"{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: '{2}', {3} {4}{5}{6}}}",
				memberName,
				mark ?? Mark.Blank(),
				string.IsNullOrWhiteSpace( serviceEndPoint ) ? string.Empty : serviceEndPoint,
				string.IsNullOrWhiteSpace( requestParameters ) ? string.Empty : ", RequestParameters: " + requestParameters,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
			);
			return str;
		}
	}
}
