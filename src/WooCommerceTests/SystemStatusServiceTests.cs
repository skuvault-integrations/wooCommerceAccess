using NUnit.Framework;

namespace WooCommerceTests
{
	//[ TestFixture( "credentials_VM_sandbox.csv" ) ]
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class SystemStatusServiceTests : BaseTest
	{
		public SystemStatusServiceTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ Test ]
		public void GetStoreVersion()
		{
			var storeVersion = base.SystemStatusService.GetStoreVersionAsync( this.Mark ).Result;

			Assert.IsFalse( string.IsNullOrEmpty( storeVersion ) );
		}
	}
}
