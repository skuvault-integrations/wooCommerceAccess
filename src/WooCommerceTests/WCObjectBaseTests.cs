using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WooCommerceAccess.Models;
using WooCommerceAccess.Services;

namespace WooCommerceTests
{
	public class WCObjectBaseTests
	{
		private static readonly Randomizer _randomizer = new Randomizer();
		
		[ Test ]
		public void CreateProductsVariationsInventoryUpdateRequests_ReturnsAllRequests_WhenGivenProductsOnly()
		{
			var wooCommerceProducts = GenerateWooCommerceProductsVariations( productsOnly: true );
			var skusQuantities = GenerateSkusQuantities( wooCommerceProducts );

			var productRequests = WCObjectBase.CreateProductsVariationsInventoryUpdateRequests( wooCommerceProducts, skusQuantities, productsOnly: true );

			AssertProductsVariationsInventoryUpdateRequests( wooCommerceProducts, skusQuantities, productRequests );
		}

		[ Test ]
		public void CreateProductsVariationsInventoryUpdateRequests_ReturnsAllRequests_WhenGivenProductsAndVariations_andProductsOnlyIsFalse()
		{
			var wooCommerceProductsAndVariations = GenerateWooCommerceProductsVariations( productsOnly: false );
			var skusQuantities = GenerateSkusQuantities( wooCommerceProductsAndVariations );

			var productRequests = WCObjectBase.CreateProductsVariationsInventoryUpdateRequests( wooCommerceProductsAndVariations, skusQuantities, productsOnly: false );

			AssertProductsVariationsInventoryUpdateRequests( wooCommerceProductsAndVariations, skusQuantities, productRequests );
		}

		[ Test ]
		public void CreateProductsVariationsInventoryUpdateRequests_ReturnsProductsRequestsOnly_WhenGivenProductsAndVariations_andProductsOnlyIsTrue()
		{
			var wooCommerceProductsAndVariations = GenerateWooCommerceProductsVariations( productsOnly: false );
			var skusQuantities = GenerateSkusQuantities( wooCommerceProductsAndVariations );

			var productRequests = WCObjectBase.CreateProductsVariationsInventoryUpdateRequests( wooCommerceProductsAndVariations, skusQuantities, productsOnly: true );

			var wooCommerceProductsOnly = wooCommerceProductsAndVariations.Values.Where( a => a.ParentId == 0 ).ToDictionary( x => x.Sku, x => x );
			AssertProductsVariationsInventoryUpdateRequests( wooCommerceProductsOnly, skusQuantities, productRequests );
		}

		[ Test ]
		public void CreateVariationsInventoryUpdateRequests_ReturnsEmpty_WhenGivenProductsOnly()
		{
			var wooCommerceProducts = GenerateWooCommerceProductsVariations( productsOnly: true );
			var skusQuantities = GenerateSkusQuantities( wooCommerceProducts );

			var variationRequests = WCObjectBase.CreateVariationsInventoryUpdateRequests( wooCommerceProducts, skusQuantities );

			variationRequests.Should().BeEmpty();
		}

		[ Test ]
		public void CreateVariationsInventoryUpdateRequests_ReturnsAllRequests_WhenGivenVariationsOnly_andDifferentParentProductId()
		{
			var wooCommerceVariations = GenerateWooCommerceProductsVariations( variationsOnly: true );
			var skusQuantities = GenerateSkusQuantities( wooCommerceVariations );

			var variationRequests = WCObjectBase.CreateVariationsInventoryUpdateRequests( wooCommerceVariations, skusQuantities );

			wooCommerceVariations.Count.Should().Be( variationRequests.Count );
			AssertVariationsInventoryUpdateRequests( skusQuantities, variationRequests );
		}

		[ Test ]
		public void CreateVariationsInventoryUpdateRequests_ReturnsOneRequestWithMultiInventoryQuantities_WhenGivenVariationsOnly_withCommonParentProductId()
		{
			var wooCommerceVariations = GenerateWooCommerceProductsVariations( variationsOnly: true, parentProductId: _randomizer.Next() );
			var skusQuantities = GenerateSkusQuantities( wooCommerceVariations );

			var variationRequests = WCObjectBase.CreateVariationsInventoryUpdateRequests( wooCommerceVariations, skusQuantities );

			variationRequests.Count.Should().Be( 1 );
			AssertVariationsInventoryUpdateRequests( skusQuantities, variationRequests );
		}

		private static Dictionary< string, WooCommerceProduct > GenerateWooCommerceProductsVariations( int size = 2, bool productsOnly = false,
			bool variationsOnly = false, int parentProductId = 0 )
		{
			var productsOrVariations = new Dictionary< string, WooCommerceProduct >();
			
			for( var i = 0; i < size; i++ )
			{
				var parentId = ( i == 0 && !productsOnly ) || variationsOnly ? _randomizer.Next() : 0;
				if( parentProductId != 0 )
				{
					parentId = parentProductId;
				}

				var productOrVariation = new WooCommerceProduct 
				{
					Id = _randomizer.Next(),
					Sku = _randomizer.GetString(), 
					ParentId = parentId,
					Quantity = _randomizer.Next(),
					ManagingStock = true
				};

				productsOrVariations.Add( productOrVariation.Sku, productOrVariation );
			}

			return productsOrVariations;
		}

		private static Dictionary< string, int > GenerateSkusQuantities( Dictionary< string, WooCommerceProduct > productsOrVariations )
		{
			var skusQuantities = new Dictionary< string, int >();
			foreach( var sku in productsOrVariations.Keys )
			{
				skusQuantities.Add( sku, _randomizer.Next() );
			}

			return skusQuantities;
		}

		private static void AssertProductsVariationsInventoryUpdateRequests( Dictionary< string, WooCommerceProduct > productsOrVariations,
			Dictionary< string, int > skusQuantities, List< QuantityUpdate > requests )
		{
			requests.Count.Should().Be( productsOrVariations.Count );
			requests.Select( i => i.Sku ).All( productsOrVariations.ContainsKey );

			foreach( var request in requests )
			{
				skusQuantities[ request.Sku ].Should().Be( request.Quantity );
			}
		}

		private static void AssertVariationsInventoryUpdateRequests( Dictionary< string, int > skusQuantities, Dictionary< ProductId, List< QuantityUpdate > > requests )
		{
			foreach( var variationId in requests.Keys )
			{
				foreach( var skuQuantity in requests[ variationId ] )
				{
					skusQuantities[ skuQuantity.Sku ].Should().Be( skuQuantity.Quantity );
				}
			}
		}
	}
}