using NUnit.Framework;

namespace WooCommerceTests
{
	[ TestFixture( "WP4_7_WC_3_6_credentials.csv" ) ]
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	public class SystemStatusServiceTests : BaseTest
	{
		public SystemStatusServiceTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ Test ]
		public void GetStoreVersion()
		{
			var storeVersion = base.SystemStatusService.GetStoreVersion().Result;

			Assert.IsFalse( string.IsNullOrEmpty( storeVersion ) );
		}
	}
}
