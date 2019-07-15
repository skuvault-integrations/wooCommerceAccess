using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WooCommerceTests
{
	[ TestFixture( "WP4_1_WC_2_4_credentials.csv" ) ]
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	public class ProductTests : BaseTest
	{
		private const string testSku = "testsku";
		private const string testSku2 = "testsku2";

		public ProductTests( string shopCredentialsFileName ) : base( shopCredentialsFileName) { }

		[ Test ]
		public void GetProductBySku()
		{
			var product = base.ProductsService.GetProductBySkuAsync( testSku, CancellationToken.None ).Result;

			product.Should().NotBeNull();
			product.Sku.Should().Be( testSku );
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

			var updatedProducts = await base.ProductsService.UpdateSkusQuantityAsync( request, CancellationToken.None ).ConfigureAwait( false );

			updatedProducts.Length.Should().Be( request.Count );
			var updatedTestSku = updatedProducts.FirstOrDefault( pr => pr.Sku.Equals( testSku ) );
			updatedTestSku.Quantity.Should().Be( request[ testSku ] );
			var updatedTestSku2 = updatedProducts.FirstOrDefault( pr => pr.Sku.Equals( testSku2 ) );
			updatedTestSku2.Quantity.Should().Be( request[ testSku2 ] );
		}
	}
}
