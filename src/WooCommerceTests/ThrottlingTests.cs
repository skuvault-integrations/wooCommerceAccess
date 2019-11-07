using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WooCommerceAccess.Throttling;
using DateTime = System.DateTime;

namespace WooCommerceTests
{
	public class ThrottlingTests
	{
		private List< DateTime > throttleInstances;

		[ Test ]
		public async Task Throttler()
		{
			throttleInstances = new List< DateTime >();
			const int maxRetryCount = 2;
			const int quotaRestoreTimeInSeconds = 1;
			var throttler = new Throttler( 2, quotaRestoreTimeInSeconds, maxRetryCount );

			try
			{
				await throttler.ExecuteAsync( FuncToThrottleException );
			}
			catch ( Exception )
			{
			}

			throttleInstances.Count.Should().Be( maxRetryCount + 1 );
			var lastAttemptTime = throttleInstances[0];
			var delays = throttleInstances.Select( i =>
			{
				var delay = i - lastAttemptTime;
				lastAttemptTime = i;
				return delay;
			}).Where( d => d.TotalMilliseconds > 0 ).ToList();

			delays.Count.Should().Be( maxRetryCount );
			delays.Min().TotalSeconds.Should().BeGreaterOrEqualTo( 5 );
			delays.Min().TotalSeconds.Should().BeLessThan( 5 + 1 );
			delays.Max().TotalSeconds.Should().BeGreaterOrEqualTo( 5 + 20 * ( maxRetryCount - 1 ) );
			delays.Max().TotalSeconds.Should().BeLessThan( 5 + 20 * ( maxRetryCount - 1 ) + 1 );
		}

		private Task< string > FuncToThrottleException()
		{
			var now = DateTime.Now;
			throttleInstances.Add( now );
			Console.WriteLine( now );
			throw new Exception();
		}
	}
}
