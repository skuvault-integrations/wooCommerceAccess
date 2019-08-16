using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WooCommerceAccess.Services;
using WooCommerceNET;

namespace WooCommerceTests
{
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	[ TestFixture( "WP4_7_WC_3_6_credentials.csv" ) ]
	public class VariationTests : BaseTest
	{
		public VariationTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ Test ]
		public async Task GetProductVariationsByProductId()
		{
			const int productId = 113;
			var apiV3WCObject = new ApiV3WCObject( new RestAPI( base.Config.ShopUrl + "wp-json/wc/v3/", base.Config.ConsumerKey, base.Config.ConsumerSecret ) );

			var productVariations = await apiV3WCObject.CollectVariationsByProductFromAllPagesAsync( productId, base.Config.ProductsPageSize );

			Assert.IsTrue( productVariations.Any() );
		}
	}
}