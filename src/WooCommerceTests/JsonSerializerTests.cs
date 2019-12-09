using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class JsonSerializerTests
	{
		[ Test ]
		public void VariationsToUpdate_Dictionary_ToJson()
		{
			var productId = new ProductId( 3 );
			const string sku = "testsku";
			const int quantity = 213;
			var variation = new WooCommerceVariation
			{
				Sku = sku,
				Quantity = quantity
			};
			var skusQuantities = new Dictionary< string, int >
			{
				{
					sku, quantity
				}
			};
			IEnumerable< QuantityUpdate > quantityUpdate = new List< QuantityUpdate >
			{
				new QuantityUpdate( variation, skusQuantities )
			};
			var variationsToUpdate = new Dictionary< ProductId, IEnumerable< QuantityUpdate > >
			{
				{
					productId, quantityUpdate
				}
			};

			var json = variationsToUpdate.Select( x => new { ProductId = x.Key.Id, Variations = x.Value } ).ToJson();

			var deserialized = JsonConvert.DeserializeObject< IEnumerable< ProductVariationsForJson > >( json ).ToList();
			deserialized.Count.Should().Be( variationsToUpdate.Count );
			var firstItem = deserialized.First();
			firstItem.ProductId.Should().Be( productId.Id );
			var firstVariation = firstItem.Variations.First();
			firstVariation.Should().BeEquivalentTo( quantityUpdate.First() );
		}

		public struct ProductVariationsForJson
		{
			public int ProductId { get; set; }
			public IEnumerable< QuantityUpdate > Variations { get; set; }
		}
	}
}
