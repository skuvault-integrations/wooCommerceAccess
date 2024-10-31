using System;
using NUnit.Framework;
using WooCommerceAccess.Helpers;

namespace WooCommerceTests.Helpers
{
    public class DateTimeHelpersTests
    {
        [Test]
        public void RoundDateDownToTopOfMinute_DoesNotRoundDown_WhenAlreadyTopOfMinute()
        {
            var dateTime = new DateTime(2000, 1, 1, 8, 8, 0);

            var result = dateTime.RoundDateDownToTopOfMinute();

            Assert.That(dateTime, Is.EqualTo(result));
        }

        [TestCase(1, 0)]
        [TestCase(0, 100)]
        [TestCase(3, 3)]
        public void RoundDateDownToTopOfMinute_RoundsDown_WhenNotAtTopOfMinute(int second, int millisecond)
        {
            var dateTime = new DateTime(2000, 1, 1, 8, 8, second, millisecond);

            var result = dateTime.RoundDateDownToTopOfMinute();

            AssertDates(result, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
                dateTime.Minute, expectedSecond: 0, expectedMillisecond: 0, dateTime.Kind);
        }

        [Test]
        public void RoundDateUpToTopOfMinute_DoesNotRoundUp_WhenAtTopOfMinute()
        {
            var dateTime = new DateTime(2000, 1, 1, 8, 8, 0);

            var result = dateTime.RoundDateUpToTopOfMinute();

            Assert.That(dateTime, Is.EqualTo(result));
        }
        
        [TestCase(1, 0)]
        [TestCase(0, 100)]
        [TestCase(3, 3)]
        public void RoundDateUpToTopOfMinute_RoundsUp_WhenNotAtTopOfMinute(int second, int millisecond)
        {
            var dateTime = new DateTime(2000, 1, 1, 8, 8, second, millisecond);

            var result = dateTime.RoundDateUpToTopOfMinute();

            AssertDates(result, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
                dateTime.Minute + 1, expectedSecond: 0, expectedMillisecond: 0, dateTime.Kind);
        }
        
        [Test]
        //Correctly crosses hour/day/month/year boundary
        public void RoundDateUpToTopOfMinute_RoundsUp_WhenNotAtTopOfMinute_andAtEndOfYear()
        {
            var dateTime = new DateTime(2000, 12, 31, 23, 59, 30);

            var result = dateTime.RoundDateUpToTopOfMinute();
            
            var addMinute = dateTime.AddMinutes(1);
            AssertDates(result, addMinute.Year, addMinute.Month, addMinute.Day, addMinute.Hour,
                addMinute.Minute, expectedSecond: 0, expectedMillisecond: 0, dateTime.Kind);
        }

        private static void AssertDates(DateTime actual, int expectedYear, int expectedMonth, int expectedDay,
            int expectedHour, int expectedMinute, int expectedSecond, int expectedMillisecond,
            DateTimeKind expectedKind)
        {
            Assert.Multiple(() =>
            {
                Assert.That(actual.Year, Is.EqualTo(expectedYear));
                Assert.That(actual.Month, Is.EqualTo(expectedMonth));
                Assert.That(actual.Day, Is.EqualTo(expectedDay));
                Assert.That(actual.Hour, Is.EqualTo(expectedHour));
                Assert.That(actual.Minute, Is.EqualTo(expectedMinute));
                Assert.That(actual.Second, Is.EqualTo(expectedSecond));
                Assert.That(actual.Millisecond, Is.EqualTo(expectedMillisecond));
                Assert.That(actual.Kind, Is.EqualTo(expectedKind));
            });
        }
    }
}