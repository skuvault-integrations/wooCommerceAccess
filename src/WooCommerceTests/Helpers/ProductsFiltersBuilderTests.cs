using System;
using NUnit.Framework;
using WooCommerceAccess.Helpers;

namespace WooCommerceTests.Helpers
{
	public class ProductsFiltersBuilderTests
	{
		[TestCase(true)]
		[TestCase(false)]
		public void CreateProductStartDateTimeFilters_ShouldSetDatesAreGmtToTrue_WhenDateIsUtc(bool includeUpdated)
		{
			var startDate = new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc);

			var result = ProductsFiltersBuilder.CreateProductStartDateTimeFilters(startDate, includeUpdated);

			Assert.That(result[ProductsFiltersBuilder.DatesAreGmt], Is.EqualTo("1"));
		}

		[TestCase(DateTimeKind.Unspecified, true)]
		[TestCase(DateTimeKind.Local, true)]
		[TestCase(DateTimeKind.Unspecified, false)]
		[TestCase(DateTimeKind.Local, false)]
		public void CreateProductStartDateTimeFilters_ShouldNotSetDatesAreGmtToTrue_WhenDateIsNotUtc(
			DateTimeKind startDateKind, bool includeUpdated)
		{
			var startDate = new DateTime(2001, 1, 1, 1, 1, 1, startDateKind);

			var result = ProductsFiltersBuilder.CreateProductStartDateTimeFilters(startDate, includeUpdated: true);

			Assert.That(result.ContainsKey(ProductsFiltersBuilder.DatesAreGmt), Is.False);
		}

		[Test]
		public void CreateProductStartDateTimeFilters_ShouldFilterByModifiedDate_WhenIncludeUpdatedIsTrue()
		{
			var result = ProductsFiltersBuilder.CreateProductStartDateTimeFilters(DateTime.UtcNow, includeUpdated: true);

			Assert.Multiple(() => {
				Assert.That(result.ContainsKey("modified_after"), Is.True);
				Assert.That(result.ContainsKey("after"), Is.False);
			});
		}

		[Test]
		public void CreateProductStartDateTimeFilters_ShouldFilterByCreatedDate_WhenIncludeUpdatedIsFalse()
		{
			var result = ProductsFiltersBuilder.CreateProductStartDateTimeFilters(DateTime.UtcNow, includeUpdated: false);

			Assert.Multiple(() => {
				Assert.That(result.ContainsKey("after"), Is.True);
				Assert.That(result.ContainsKey("modified_after"), Is.False);
			});
		}
	}
}