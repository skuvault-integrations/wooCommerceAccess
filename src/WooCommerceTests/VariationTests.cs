using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WooCommerceAccess.Models;
using WooCommerceAccess.Services;
using WooCommerceNET;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	[ TestFixture( "credentials_VM_sandbox.csv" ) ]
	public class VariationTests : BaseTest
	{
		private ApiV3WCObject _apiV3WcObject;
		public VariationTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		private string _serviceUrl;

		[ SetUp ]
		public void Initialize()
		{
			this._serviceUrl = base.Config.ShopUrl + ApiBasePath.V3;
			this._apiV3WcObject = new ApiV3WCObject( new RestAPI( this._serviceUrl, base.Config.ConsumerKey, base.Config.ConsumerSecret ) );
		}

		[ Test ]
		public async Task GetProductVariationsByProductId()
		{
			const int productId = 113;

			var productVariations = await _apiV3WcObject.CollectVariationsByProductFromAllPagesAsync( productId, base.Config.ProductsPageSize, this._serviceUrl, this.Mark );

			Assert.IsTrue( productVariations.Any() );
		}

		[ Test ]
		public async Task UpdateSkusQuantityAsync_ProductWithOver100Variations()
		{
			const int productIdWith100PlusVariations = 1272;
			var mark = this.Mark;
			var productVariations = await _apiV3WcObject.CollectVariationsByProductFromAllPagesAsync( productIdWith100PlusVariations, base.Config.ProductsPageSize, this._serviceUrl, mark );
			var random = new Random();
			var skusQuantitiesUpdate = productVariations.ToDictionary( v => v.Sku, v => (v.Quantity ?? default(int)) + random.Next(1, 10) );

			var skuQuantities = await this.ProductsService.UpdateSkusQuantityAsync( skusQuantitiesUpdate, mark );

			skuQuantities.Should().Equal( skusQuantitiesUpdate );
		}
	}

	[ TestFixture ]
	public class VariationStaticTests
	{
		private const string Testsku = "testsku";

		[ Test ]
		public void ApiV3WCObject_GetVariationsToUpdate()
		{
			var skusQuantities = new Dictionary< string, int >
			{
				{ Testsku, 21 },
				{ "testsku2", 23 }
			};
			const int productIdNew = 2;
			const int productIdExisting = 1;
			var variationsToUpdate = new Dictionary< ProductId, List< QuantityUpdate > >
			{
				{ new ProductId( productIdExisting ), 
					new List< QuantityUpdate >()
				}
			};
			const string nonManagedSku = "testsku2";

			ApiV3WCObject.GetVariationsToUpdate( skusQuantities, CollectVariationsByProductFromAllPagesAsync( nonManagedSku, 11 ), productIdNew, variationsToUpdate );

			Assert.AreEqual( productIdExisting, variationsToUpdate.First().Key.Id );
			var secondProduct = variationsToUpdate.Skip( 1 ).First();
			Assert.AreEqual( 1, secondProduct.Value.Count() );
			var newVariation = secondProduct.Value.First();
			Assert.AreEqual( skusQuantities.First().Key, newVariation.Sku );
			Assert.AreEqual( skusQuantities.First().Value, newVariation.Quantity );
		}
		
		private IEnumerable< WooCommerceVariation > CollectVariationsByProductFromAllPagesAsync( string nonManagedSku, int nonManagedQty )
		{
			return new List< WooCommerceVariation >
			{
				new WooCommerceVariation
				{
					Id = 1,
					Sku = Testsku,
					Quantity = 1,
					ManagingStock = true
				},
				new WooCommerceVariation
				{
					Id = 2,
					Sku = nonManagedSku,
					Quantity = nonManagedQty,
					ManagingStock = false
				}
			};
		}

		[ Test ]
		public void QuantityUpdate_NotManagingStock()
		{
			var testsku = Testsku;
			var skusQuantities = new Dictionary< string, int >
			{
				{ testsku, 3 } 
			};
			var variation = new WooCommerceVariation
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = false
			};
			
			var result = new QuantityUpdate( variation, skusQuantities );

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
			var variation = new WooCommerceVariation
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( variation, skusQuantities );

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
			var variation = new WooCommerceVariation
			{
				Id = 1,
				Sku = testsku,
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( variation, skusQuantities );

			Assert.IsTrue( result.IsUpdateNeeded );
			Assert.AreEqual( variation.Id, result.Id );
			Assert.AreEqual( variation.Sku, result.Sku );
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
			var variation = new WooCommerceVariation
			{
				Id = 1,
				Sku = testsku,
				Quantity = null,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( variation, skusQuantities );

			Assert.IsTrue( result.IsUpdateNeeded );
			Assert.AreEqual( variation.Id, result.Id );
			Assert.AreEqual( variation.Sku, result.Sku );
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
			var variation = new WooCommerceVariation
			{
				Id = 1,
				Sku = "another sku",
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( variation, skusQuantities );

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
			var variation = new WooCommerceVariation
			{
				Id = 1,
				Sku = testSkuUpper,
				Quantity = 1,
				ManagingStock = true
			};
			
			var result = new QuantityUpdate( variation, skusQuantities );

			Assert.IsTrue( result.IsUpdateNeeded );
			Assert.AreEqual( variation.Id, result.Id );
			Assert.AreEqual( variation.Sku.ToLower(), result.Sku.ToLower() );
			Assert.AreEqual( skusQuantities[ Testsku ], result.Quantity );
		}

		[ Test ]
		public void ApiV3WCObject_GetVariationsToUpdateWithDuplicatedProductId_ShouldPullMoreRecent()
		{
			var productId = 1;
			int quantity1 = 10, quantity2 = 5;
			var skusQuantities = new Dictionary< string, int >
			{
				{ Testsku, quantity2 }
			};
			var existingVariations = new List< WooCommerceVariation >
			{
				new WooCommerceVariation
				{
					Sku = Testsku,
					Quantity = quantity1,
					Id = 1,
					ManagingStock = true
				}
			};
			var variationsToUpdate = new Dictionary< ProductId, List< QuantityUpdate > >
			{
				{ 
					new ProductId( productId ),
					new List< QuantityUpdate >()
				}
			};
			
			ApiV3WCObject.GetVariationsToUpdate( skusQuantities, existingVariations, productId, variationsToUpdate );

			var variationsToUpdateItem = variationsToUpdate.First();
			Assert.AreEqual( productId, variationsToUpdateItem.Key.Id );
			Assert.AreEqual( skusQuantities.First().Key, variationsToUpdateItem.Value.First().Sku );
			Assert.AreEqual( quantity2, variationsToUpdateItem.Value.First().Quantity );
		}
	}
}