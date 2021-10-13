using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WooCommerceAccess.Models;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class WooCommerceOrderTests
	{
		[ TestCase( "TEST@-0103", TestName = "Create_OrderWithLettersSymbolsNumbers_ReturnsAlphaNumericOrderNumber" ) ]
		[ TestCase( "TEST0103", TestName = "Create_OrderWithLetterNumbers_ReturnsAlphaNumericOrderNumber" ) ]
		[ TestCase( "0102", TestName = "Create_OrderWithOnlyNumbers_ReturnsNumericOrderNumber" ) ]
		public void Create_AlphaNumericOrderNumber( string wooCommerceOrderNumber )
		{
			var orderIdAlphaNumeric = new WooCommerceOrder
			{
				Id = wooCommerceOrderNumber,
				Number = wooCommerceOrderNumber,
				CreateDateUtc = DateTime.UtcNow,
				UpdateDateUtc = DateTime.UtcNow,
				Status = "pending",
				Currency = "USD",
				Total = 5,
				Note = string.Empty,
				WasPaid = false,
				TotalTax = 0,
				TotalDiscount = 0
			};

			Assert.AreEqual( orderIdAlphaNumeric.Number, wooCommerceOrderNumber );
		}
	}
}
