using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class InventoryTests : BaseTest
	{
		private const string testSku = "testsku";
		private string testSku2 = "testsku2";

		public InventoryTests( string shopCredentialsFileName ) : base( shopCredentialsFileName) { }
		
		[ Test ]
		public async Task UpdateSkusQuantity()
		{
			//testSku2 = "testsku3-red";

			var random = new Random();
			var request = new Dictionary< string, int >
			{
				//{ "test31091", random.Next( 1, 100 ) },
				{ "testSku4", random.Next( 1, 100 ) },
				{ "testsku4-med", random.Next( 1, 100 ) },
				{ "testsku4-sm", random.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateSkusQuantityAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			//updatedProducts.Count.Should().Be( request.Count );
			//var updatedTestSku = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSku ) );
			//updatedTestSku.Value.Should().Be( request[ testSku ] );
			//var updatedTestSku2 = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSku2 ) );
			//updatedTestSku2.Value.Should().Be( request[ testSku2 ] );
		}
	}
}
