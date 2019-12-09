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
	[ TestFixture( "WP5_2_WC_3_6_credentials.csv" ) ]
	[ TestFixture( "WP4_7_WC_3_6_credentials.csv" ) ]
	public class VariationTests : BaseTest
	{
		private ApiV3WCObject _apiV3WcObject;
		public VariationTests( string shopCredentialsFileName ) : base( shopCredentialsFileName ) { }

		[ SetUp ]
		public void Initialize()
		{
			this._apiV3WcObject = new ApiV3WCObject( new RestAPI( base.Config.ShopUrl + "wp-json/wc/v3/", base.Config.ConsumerKey, base.Config.ConsumerSecret ) );
		}

		[ Test ]
		public async Task GetProductVariationsByProductId()
		{
			const int productId = 113;

			var productVariations = await _apiV3WcObject.CollectVariationsByProductFromAllPagesAsync( productId, base.Config.ProductsPageSize );

			Assert.IsTrue( productVariations.Any() );
		}

		[ Test ]
		public async Task UpdateSkusQuantityAsync_ProductWithOver100Variations()
		{
			const int productIdWith100PlusVariations = 1272;
			var productVariations = await _apiV3WcObject.CollectVariationsByProductFromAllPagesAsync( productIdWith100PlusVariations, base.Config.ProductsPageSize );
			var random = new Random();
			var skusQuantitiesUpdate = productVariations.ToDictionary( v => v.Sku, v => (v.Quantity ?? default(int)) + random.Next(1, 10) );

			var skuQuantities = await this.ProductsService.UpdateSkusQuantityAsync( skusQuantitiesUpdate );

			skuQuantities.Should().Equal( skusQuantitiesUpdate );
		}
	}

	[ TestFixture ]
	public class VariationStaticTests
	{
		private const string Testsku = "testsku";

		[ Test ]
		public async Task ApiV3WCObject_GetVariationsToUpdate()
		{
			var skusQuantities = new Dictionary< string, int >
			{
				{ Testsku, 21 },
				{ "testsku2", 23 }
			};
			const int productIdNew = 2;
			const int productIdExisting = 1;
			var variationsToUpdate = new Dictionary< ProductId, IEnumerable< QuantityUpdate > >
			{
				{ new ProductId( productIdExisting ), 
					new List< QuantityUpdate >()
				}
			};
			const string nonManagedSku = "testsku2";

			ApiV3WCObject.GetVariationsToUpdate( skusQuantities, await CollectVariationsByProductFromAllPagesAsync( nonManagedSku, 11 ), productIdNew, variationsToUpdate );

			Assert.AreEqual( productIdExisting, variationsToUpdate.First().Key.Id );
			var secondProduct = variationsToUpdate.Skip( 1 ).First();
			Assert.AreEqual( 1, secondProduct.Value.Count() );
			var newVariation = secondProduct.Value.First();
			Assert.AreEqual( skusQuantities.First().Key, newVariation.Sku );
			Assert.AreEqual( skusQuantities.First().Value, newVariation.Quantity );
		}
		
		private async Task< IEnumerable< WooCommerceVariation > > CollectVariationsByProductFromAllPagesAsync( string nonManagedSku, int nonManagedQty )
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
	}
}