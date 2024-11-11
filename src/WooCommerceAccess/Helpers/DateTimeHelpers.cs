using System;

namespace WooCommerceAccess.Helpers
{
    internal static class DateTimeHelpers
    {
        /// <summary>
        /// Round the date down to the nearest top of minute
        /// </summary>
        /// <remarks>Per Linnworks, for some clients orders aren't returned when query by date/times with non-zero seconds/ms</remarks>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal static DateTime RoundDateDownToTopOfMinute(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);
        }

        /// <summary>
        /// Round the date up to the nearest top of minute
        /// </summary>
        /// <remarks>Per Linnworks, for some clients orders aren't returned when query by date/times with non-zero seconds/ms</remarks>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal static DateTime RoundDateUpToTopOfMinute(this DateTime dateTime)
        {
            if (dateTime.Second == 0 && dateTime.Millisecond == 0)
            {
                return dateTime;
            }
            var addOneMinute = dateTime.AddMinutes(1);
            return new DateTime(addOneMinute.Year, addOneMinute.Month, addOneMinute.Day, addOneMinute.Hour, addOneMinute.Minute, 0, addOneMinute.Kind);
        }
    }
}