using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WooCommerceTests
{
	[ TestFixture( "WP4_1_WC_2_4_credentials.csv" ) ]
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	[ TestFixture( "WP4_7_WC_3_6_credentials.csv" ) ]
	public class ProductTests : BaseTest
	{
		private const string testSku = "testsku3";
		private const string testSku2 = "testsku2";

		public ProductTests( string shopCredentialsFileName ) : base( shopCredentialsFileName) { }

		[ Test ]
		public void GetProductBySku()
		{
			var product = base.ProductsService.GetProductBySkuAsync( testSku ).Result;

			product.Should().NotBeNull();
			product.Sku.ToLower().Should().Be( testSku );
		}

		[ Test ]
		public void GetProductsCreatedUpdatedAfterAsync_CreateOnly()
		{
			var products = base.ProductsService.GetProductsCreatedUpdatedAfterAsync( DateTime.MinValue, false ).Result;

			products.Count().Should().NotBe( 0 );
		}

		[ Test ]
		public void GetProductsCreatedUpdatedAfterAsync_CreateAndUpdate()
		{
			var products = base.ProductsService.GetProductsCreatedUpdatedAfterAsync( DateTime.MinValue, true ).Result;

			products.Count().Should().NotBe( 0 );
		}

		[ Test ]
		public async Task UpdateSkuQuantity()
		{
			int newQuantity = new Random().Next( 1, 100 );
			var updatedProduct = await base.ProductsService.UpdateSkuQuantityAsync( testSku, newQuantity );
			updatedProduct.Should().NotBeNull();
			updatedProduct.Quantity.Should().Be( newQuantity );
		}

		[ Test ]
		public async Task UpdateSkuQuantityToZero()
		{
			int newQuantity = new Random().Next( 1, 100 );
			await base.ProductsService.UpdateSkuQuantityAsync( testSku, newQuantity );

			var updatedProduct = await base.ProductsService.UpdateSkuQuantityAsync( testSku, 0 );
			updatedProduct.Should().NotBeNull();
			updatedProduct.Quantity.Should().Be( 0 );
		}

		[ Test ]
		public async Task UpdateSkusQuantity()
		{
			var random = new Random();
			var request = new Dictionary< string, int >
			{
				{ testSku, random.Next( 1, 100 ) },
				{ testSku2, random.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateSkusQuantityAsync( request ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count().Should().Be( request.Count );
			var updatedTestSku = updatedProducts.FirstOrDefault( pr => pr.Sku.Equals( testSku ) );
			updatedTestSku?.Quantity.Should().Be( request[ testSku ] );
			var updatedTestSku2 = updatedProducts.FirstOrDefault( pr => pr.Sku.Equals( testSku2 ) );
			updatedTestSku2?.Quantity.Should().Be( request[ testSku2 ] );
		}

		[ Test ]
		public async Task GetProductsUsingByPagination()
		{
			base.Config.ProductsPageSize = 2;

			var products = await this.ProductsService.GetProductsCreatedUpdatedAfterAsync( DateTime.MinValue, true );
			products.Count().Should().BeGreaterOrEqualTo( 2 );

			base.Config.ProductsPageSize = 10;
		}
	}
}
