using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WooCommerceAccess.Services;
using WooCommerceAccess.Shared;
using WApiV3 = WooCommerceNET.WooCommerce.v3;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class BatchListTests
	{
		[ Test ]
		public void BatchList_SplitEmptyList()
		{
			var listEmpty = new List< WApiV3.Product >();

			var batchList = new BatchList< WApiV3.Product >( listEmpty, 10 );

			Assert.IsEmpty( batchList );
		}

		[ Test ]
		public void BatchList_SplitListLessThanBatchSize()
		{
			var list = new List< WApiV3.Product >
			{
				new WApiV3.Product
				{
					name = "AA"
				},
				new WApiV3.Product(),
				new WApiV3.Product()
			};

			var batchList = new BatchList< WApiV3.Product >( list, 5 ).ToList();
			var firstBatch = batchList.First().ToList();
			var secondBatch = batchList.Skip( 1 ).FirstOrDefault();

			Assert.AreEqual( list.Count, firstBatch.Count() );
			Assert.AreEqual( list.First().name, firstBatch.First().name );
			Assert.IsNull( secondBatch );
		}

		[ Test ]
		public void BatchList_SplitMultiBatchList()
		{
			const int batchSize = 2;
			var list = new List< WApiV3.Product >
			{
				new WApiV3.Product
				{
					name = "asd"
				},
				new WApiV3.Product(),

				new WApiV3.Product(),
				new WApiV3.Product(),

				new WApiV3.Product
				{
					name = "lkj"
				}
			};

			var batchList = new BatchList< WApiV3.Product >( list, batchSize ).ToList();

			var firstBatch = batchList.First().ToList();
			var secondBatch = batchList.Skip( 1 ).First().ToList();
			var thirdBatch = batchList.Skip( 2 ).First().ToList();
			var fourthBatch = batchList.Skip( 3 ).FirstOrDefault();

			Assert.AreEqual( batchSize, firstBatch.Count );
			Assert.AreEqual( list.First().name, firstBatch.First().name );
			Assert.AreEqual( batchSize, secondBatch.Count );
			Assert.AreEqual( list.Count - batchSize * 2, thirdBatch.Count );
			Assert.AreEqual( list.Last().name, thirdBatch.First().name );
			Assert.IsNull( fourthBatch );
		}
	}
}
