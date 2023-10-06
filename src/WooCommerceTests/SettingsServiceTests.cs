using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class SettingsServiceTests : BaseTest
	{
		public SettingsServiceTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ Test ]
		public async Task GetWooCommerceSettings()
		{
			var settings = await base.SettingsService.GetSettingsAsync( this.Mark );

			settings.WeightUnit.Should().Be( "kg" );
			settings.Currency.Should().Be( "USD" );
		}
	}
}
