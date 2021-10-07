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
		[ TestCase( "TEST@-0103", ExpectedResult = true, TestName = "Create_OrderWithLettersSymbolsNumbers_ReturnsTrue" ) ]
		[ TestCase( "TEST0103", ExpectedResult = true, TestName = "Create_OrderWithLetterNumbers_ReturnsTrue" ) ]
		[ TestCase( "0102", ExpectedResult = false, TestName = "Create_OrderWithOnlyNumbers_ReturnsFalse" ) ]
		public bool Create_OrderIdWithSymbolsAndLetters( string orderId )
		{
			var orderWithLetters = new WooCommerceOrder
			{
				Id = orderId,
				Number = "0001",
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

			int thrownAway;

			return !int.TryParse( orderWithLetters.Id, out thrownAway );
		}
	}
}
