using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class InventoryTests : BaseTest
	{
		
		private const string TestProductSku = "testSku4";
		private const string TestVariationProductSku = "testsku4-med";
		private readonly Randomizer _randomizer = new Randomizer();

		public InventoryTests( string shopCredentialsFileName ) : base( shopCredentialsFileName) { }
		
		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsProductsWithUpdatedQuantity_WhenProductIsSpecified()
		{
			var request = new Dictionary< string, int >
			{
				{ TestProductSku, _randomizer.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestProduct = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( TestProductSku ) );
			updatedTestProduct.Value.Should().Be( request[ TestProductSku ] );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsVariationProductWithUpdatedQuantity_WhenVariationProductIsSpecified()
		{
			var request = new Dictionary< string, int >
			{
				{ TestVariationProductSku, _randomizer.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedVariationProduct = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( TestVariationProductSku ) );
			updatedVariationProduct.Value.Should().Be( request[ TestVariationProductSku ] );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsEmpty_WhenProductHasIncorrectSku()
		{
			var request = new Dictionary< string, int >
			{
				{ _randomizer.GetString( 5 ), _randomizer.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( 0 );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsProductWithNegativeQuantity_WhenProductHasNegativeQuantity()
		{
			var request = new Dictionary< string, int >
			{
				{ TestProductSku, _randomizer.Next( -100, -1 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestProduct = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( TestProductSku ) );
			updatedTestProduct.Value.Should().Be( request[ TestProductSku ] );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsProductWithZeroQuantity_WhenProductHasZeroQuantity()
		{
			var request = new Dictionary< string, int >
			{
				{ TestProductSku, 0 }
			};

			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestProduct = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( TestProductSku ) );
			updatedTestProduct.Value.Should().Be( request[ TestProductSku ] );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsAllProductsWithUpdatedQuantities_WhenProductAndVariationProductAreSpecified()
		{
			var request = new Dictionary< string, int >
			{
				{ TestProductSku, _randomizer.Next( 1, 100 ) },
				{ TestVariationProductSku, _randomizer.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestProduct = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( TestProductSku ) );
			updatedTestProduct.Value.Should().Be( request[ TestProductSku ] );
			var updatedVariationProduct = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( TestVariationProductSku ) );
			updatedVariationProduct.Value.Should().Be( request[ TestVariationProductSku ] );
		}

		[ Explicit ]
		[ Test ]
		public async Task UpdateInventoryAsync_ShouldReturnsEmpty_WhenProductsListIsEmpty()
		{
			var updatedProducts = ( await base.ProductsService.UpdateInventoryAsync( new Dictionary< string, int >(), this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( 0 );
		}
	}
}
