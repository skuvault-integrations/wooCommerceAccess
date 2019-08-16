using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Services;
using WooCommerceNET;

namespace WooCommerceTests
{
	[ TestFixture( "WP4_1_WC_2_4_credentials.csv" ) ]
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	[ TestFixture( "WP4_7_WC_3_6_credentials.csv" ) ]
	public class ProductTests : BaseTest
	{
		private const string testSku = "testsku";
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

		//TODO GUARD-164 Run for legacy v4.1 and verify that it gets everything and updates
		//Try existing and non-existing sku's, products/variations with managed stock and not
		//Products with and without variations
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

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestSku = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSku ) );
			updatedTestSku.Value.Should().Be( request[ testSku ] );
			var updatedTestSku2 = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSku2 ) );
			updatedTestSku2.Value.Should().Be( request[ testSku2 ] );
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

	[ TestFixture ]
	public class ProductStaticTests 
	{
		[ Test ]
		public async Task WCObjectBase_GetVariationsToUpdate()
		{
			Func< int, Task< IEnumerable< WooCommerceVariation > > > getVariationsAsync = 
				async prodId => await CollectVariationsByProductFromAllPagesAsync();
			var skusQuantities = new Dictionary< string, int >
			{
				{ "testsku", 21 },
				{ "testsku2", 23 }
			};
			const int productIdNew = 2;
			const int productIdExisting = 1;
			var variationsToUpdate = new Dictionary< ProductId, IEnumerable< QuantityUpdate > >
			{
				{ new ProductId( productIdExisting ), 
					new List< QuantityUpdate >
					{
						new QuantityUpdate( 1, "testsku99", 5 )
					}
				}
			};

			await WCObjectBase.GetVariationsToUpdate( getVariationsAsync, skusQuantities, productIdNew, variationsToUpdate );

			Assert.AreEqual( productIdExisting, variationsToUpdate.First().Key.Id );
			var secondProduct = variationsToUpdate.Skip( 1 ).First();
			Assert.AreEqual( 1, secondProduct.Value.Count() );
			var newVariation = secondProduct.Value.First();
			Assert.AreEqual( skusQuantities.First().Key, newVariation.Sku );
			Assert.AreEqual( skusQuantities.First().Value, newVariation.Quantity );
		}
		
		private async Task< IEnumerable< WooCommerceVariation > > CollectVariationsByProductFromAllPagesAsync()
		{
			return new List< WooCommerceVariation >
			{
				new WooCommerceVariation
				{
					Id = 1,
					Sku = "testsku",
					Quantity = 1,
					ManagingStock = true
				},
				new WooCommerceVariation
				{
					Id = 2,
					Sku = "testsku2",
					Quantity = 2,
					ManagingStock = false
				}
			};
		}

		[ Test ]
		public async Task ApiV3WCObject_GetProductsAndVariationsToUpdateAsync()
		{
			var testsku = "testsku";
			var testsku2 = "testsku2";
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 21 },
				{ testsku2, 23 }
			};
			const int pageSize = 10;
			var productsToUpdate = new List< QuantityUpdate >
			{
				new QuantityUpdate( 1, "testsku99", 5 )
			};

			await ApiV3WCObject.GetProductsAndVariationsToUpdateAsync( async filter => await GetNextProductPageAsync( filter, testsku2 ), 
				async productId => await Task.FromResult( new List< WooCommerceVariation >() ),
				skusQuantities, pageSize, productsToUpdate, new Dictionary< ProductId, IEnumerable< QuantityUpdate > >() );

			Assert.AreEqual( 2, productsToUpdate.Count );
			var secondProduct = productsToUpdate.Skip( 1 ).First();
			Assert.AreEqual( skusQuantities[ testsku ], secondProduct.Quantity );
		}

		private async Task< List< WooCommerceProduct > > GetNextProductPageAsync( Dictionary< string, string > filter, string sku )
		{
			if ( filter[ WooCommerceCommandEndpointName.Page.Name ] != "1" ) 
				return new List< WooCommerceProduct >();

			var nonManageStockProduct = new WooCommerceProduct
			{
				Id = 3,
				Sku = sku,
				Quantity = 3,
				ManagingStock = false
			};

			return new List< WooCommerceProduct >
			{
				new WooCommerceProduct
				{
					Id = 2,
					Sku = "testsku",
					Quantity = 2,
					ManagingStock = true
				},
				nonManageStockProduct
			};

		}
	}
}
