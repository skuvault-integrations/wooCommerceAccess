using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Services;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	[ TestFixture( "credentials_VM_sandbox.csv" ) ]
	public class ProductTests : BaseTest
	{
		private const string testSku = "testsku";
		private string testSku2 = "testsku2";

		public ProductTests( string shopCredentialsFileName ) : base( shopCredentialsFileName) { }

		[ Test ]
		public void GetProductBySku()
		{
			var product = base.ProductsService.GetProductBySkuAsync( testSku, this.Mark ).Result;

			product.Should().NotBeNull();
			product.Sku.ToLower().Should().Be( testSku );
		}

		[ Test ]
		public void GetProductsCreatedUpdatedAfterAsync_CreateOnly()
		{
			var products = base.ProductsService.GetProductsCreatedUpdatedAfterAsync( DateTime.MinValue, false, this.Mark ).Result;

			products.Count().Should().NotBe( 0 );
		}

		[ Test ]
		public void GetProductsCreatedUpdatedAfterAsync_CreateAndUpdate()
		{
			var products = base.ProductsService.GetProductsCreatedUpdatedAfterAsync( DateTime.MinValue, true, this.Mark ).Result;

			products.Count().Should().NotBe( 0 );
		}

		[ Test ]
		public void GivenDuplicatedProductAttributesList_WhenToAttributeDictionaryCalled_ThenReceiveProductAttributesWithoutDuplicates()
		{
			// arrange
			var productAttributeList = new List< ProductAttributeLine >
			{
				new ProductAttributeLine
				{
					name = "Color",
					options = new List< string >
					{
						"Red"
					}
				},
				new ProductAttributeLine
				{
					name = "Size",
					options = new List< string >
					{
						"11"
					}
				},
				new ProductAttributeLine
				{
					name = "Color",
					options = new List< string >
					{
						"Red"
					}
				},
				new ProductAttributeLine
				{
					name = "Size",
					options = new List< string >
					{
						"11"
					}
				}
			};

			// act
			var productAttributes = productAttributeList.ToAttributeDictionary();

			// assert
			productAttributes.Count.Should().Be( 2 );
			productAttributes[ "Color" ].Should().Be( "Red" );
			productAttributes[ "Size" ].Should().Be( "11" );
		}

		[ Test ]
		public void GivenDuplicatedProductAttributesListWithDuplicatedNamesOnly_WhenToAttributeDictionaryCalled_ThenReceiveProductAttributesWithoutDuplicates()
		{
			// arrange
			var productAttributeList = new List< ProductAttributeLine >
			{
				new ProductAttributeLine
				{
					name = "Color",
					options = new List< string >
					{
						"Red"
					}
				},
				new ProductAttributeLine
				{
					name = "Size",
					options = new List< string >
					{
						"11"
					}
				},
				new ProductAttributeLine
				{
					name = "Color",
					options = new List< string >
					{
						"Orange"
					}
				},
				new ProductAttributeLine
				{
					name = "Size",
					options = new List< string >
					{
						"12"
					}
				}
			};

			// act
			var productAttributes = productAttributeList.ToAttributeDictionary();

			// assert
			productAttributes.Count.Should().Be( 2 );
			// it will distictBy attribute name taking first items
			productAttributes[ "Color" ].Should().Be( "Red" );
			productAttributes[ "Size" ].Should().Be( "11" );
		}

		[ Test ]
		public async Task UpdateSkuQuantity()
		{
			var mark = this.Mark;
			int newQuantity = new Random().Next( 1, 100 );
			var updatedProduct = await base.ProductsService.UpdateSkuQuantityAsync( testSku, newQuantity, mark );
			updatedProduct.Should().NotBeNull();
			updatedProduct.Quantity.Should().Be( newQuantity );
		}

		[ Test ]
		public async Task UpdateSkuQuantityToZero()
		{
			int newQuantity = new Random().Next( 1, 100 );
			await base.ProductsService.UpdateSkuQuantityAsync( testSku, newQuantity, this.Mark );

			var updatedProduct = await base.ProductsService.UpdateSkuQuantityAsync( testSku, 0, this.Mark );
			updatedProduct.Should().NotBeNull();
			updatedProduct.Quantity.Should().Be( 0 );
		}

		[ Test ]
		public async Task UpdateSkusQuantity()
		{
			testSku2 = "testsku3-red";

			var random = new Random();
			var request = new Dictionary< string, int >
			{
				{ testSku, random.Next( 1, 100 ) },
				{ testSku2, random.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateSkusQuantityAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestSku = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSku ) );
			updatedTestSku.Value.Should().Be( request[ testSku ] );
			var updatedTestSku2 = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSku2 ) );
			updatedTestSku2.Value.Should().Be( request[ testSku2 ] );
		}

		[ Test ]
		public async Task UpdateSkusQuantityForProductAndVariation()
		{
			var testSkuProduct = "testsku14";
			var testSkuVariation = "testsku14-2";

			var random = new Random();
			var request = new Dictionary< string, int >
			{
				{ testSkuProduct, random.Next( 1, 100 ) },
				{ testSkuVariation, random.Next( 1, 100 ) }
			};

			var updatedProducts = ( await base.ProductsService.UpdateSkusQuantityAsync( request, this.Mark ).ConfigureAwait( false ) ).ToList();

			updatedProducts.Count.Should().Be( request.Count );
			var updatedTestSku = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSkuProduct ) );
			updatedTestSku.Value.Should().Be( request[ testSkuProduct ] );
			var updatedTestSku2 = updatedProducts.FirstOrDefault( pr => pr.Key.Equals( testSkuVariation ) );
			updatedTestSku2.Value.Should().Be( request[ testSkuVariation ] );
		}

		[ Test ]
		public async Task GetProductsUsingByPagination()
		{
			base.Config.ProductsPageSize = 2;

			var products = await this.ProductsService.GetProductsCreatedUpdatedAfterAsync( DateTime.MinValue, true, this.Mark );
			products.Count().Should().BeGreaterOrEqualTo( 2 );

			base.Config.ProductsPageSize = 10;
		}
	}

	[ TestFixture ]
	public class ProductStaticTests 
	{
		private const string Testsku = "testsku";

		[ Test ]
		public async Task ApiV3WCObject_GetProductsAndVariationsToUpdateAsync()
		{
			var testsku2 = "testsku2";
			var skusQuantities = new Dictionary< string, int >
			{
				{ Testsku, 21 },
				{ testsku2, 23 }
			};
			const int pageSize = 10;
			var productsToUpdate = new List< QuantityUpdate >();

			await ApiV3WCObject.GetProductsAndVariationsToUpdateAsync( async filter => await GetNextProductPageAsync( filter, testsku2 ), 
				async productId => await Task.FromResult( new List< WooCommerceVariation >() ),
				skusQuantities, pageSize, productsToUpdate, new Dictionary< ProductId, List< QuantityUpdate > >() );

			Assert.AreEqual( 1, productsToUpdate.Count );
			var product = productsToUpdate.First();
			Assert.AreEqual( skusQuantities[ Testsku ], product.Quantity );
		}

		[ Test ]
		public async Task LegacyV3WCObject_GetProductsToUpdateAsync()
		{
			var testsku2 = "testsku2";
			var skusQuantities = new Dictionary< string, int >
			{
				{ Testsku, 21 },
				{ testsku2, 23 }
			};
			const int pageSize = 10;

			var productsToUpdate = await LegacyV3WCObject.GetProductsToUpdateAsync( async filter => await GetNextProductPageAsync( filter, testsku2 ), skusQuantities, pageSize );

			Assert.AreEqual( 1, productsToUpdate.Count );
			var product = productsToUpdate.First();
			Assert.AreEqual( skusQuantities[ Testsku ], product.Quantity );
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
					Sku = Testsku,
					Quantity = 2,
					ManagingStock = true
				},
				nonManageStockProduct
			};
		}

		[ Test ]
		public void WCObjectBase_GetProductToUpdate()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 3 } 
			};
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = true
			};
			var productsToUpdate = new List< QuantityUpdate >();

			WCObjectBase.GetProductToUpdate( skusQuantities, product, productsToUpdate );

			var first = productsToUpdate.FirstOrDefault();
			Assert.AreEqual( skusQuantities[ testsku ], first?.Quantity );
		}

		[ Test ]
		public void QuantityUpdate_NotManagingStock()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 3 } 
			};
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = false
			};
			
			var result = new QuantityUpdate( product, skusQuantities );

			Assert.IsFalse( result.IsUpdateNeeded );
		}

		[ Test ]
		public void QuantityUpdate_QuantityUnchanged()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 1 } 
			};
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( product, skusQuantities );

			Assert.IsFalse( result.IsUpdateNeeded );
		}

		[ Test ]
		public void QuantityUpdate_QuantityChanged()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 3 } 
			};
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( product, skusQuantities );

			Assert.IsTrue( result.IsUpdateNeeded );
			Assert.AreEqual( product.Id, result.Id );
			Assert.AreEqual( product.Sku, result.Sku );
			Assert.AreEqual( skusQuantities[ testsku ], result.Quantity );
		}

		[ Test ]
		public void QuantityUpdate_BlankQuantityInWooCommerce()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 3 } 
			};
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = testsku,
				Quantity = null,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( product, skusQuantities );

			Assert.IsTrue( result.IsUpdateNeeded );
			Assert.AreEqual( product.Id, result.Id );
			Assert.AreEqual( product.Sku, result.Sku );
			Assert.AreEqual( skusQuantities[ testsku ], result.Quantity );
		}

		[ Test ]
		public void QuantityUpdate_SkuNotFound()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 3 } 
			};
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = "another sku",
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( product, skusQuantities );

			Assert.IsFalse( result.IsUpdateNeeded );
		}

		[ Test ]
		public void QuantityUpdate_CaseInsensitiveSkuUpdatesInventory()
		{
			var skusQuantities = new Dictionary< string, int >
			{
				{ Testsku, 3 } 
			};
			var testSkuUpper = Testsku.ToUpper();
			var product = new WooCommerceProduct
			{
				Id = 1,
				Sku = testSkuUpper,
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( product, skusQuantities );

			Assert.IsTrue( result.IsUpdateNeeded );
			Assert.AreEqual( product.Id, result.Id );
			Assert.AreEqual( product.Sku.ToLower(), result.Sku.ToLower() );
			Assert.AreEqual( skusQuantities[ Testsku ], result.Quantity );
		}
	}
}
