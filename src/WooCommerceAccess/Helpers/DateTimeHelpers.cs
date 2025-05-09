using System;

namespace WooCommerceAccess.Helpers
{
	internal static class DateTimeHelpers
	{
		//TODO PBL-9291 Add tests
		/// <summary>
		/// Convert the start date to "sortable" date/time and round down to nearest minute (Linnworks reported issues otherwise).
		/// See tickets PBL-9276, PBL-9291
		/// </summary>
		/// <param name="startDate"></param>
		/// <returns></returns>
		internal static string ToWooCommerceStartDateTime(this DateTime startDate)
		{
			return startDate.RoundDateDownToTopOfMinute().ToSortableDate();
		}

		//TODO PBL-9291 Add tests
		/// <summary>
		/// Convert the end date to "sortable" date/time and round up to the nearest minute (Linnworks reported issues otherwise).
		/// </summary>
		/// <param name="endDateTime"></param>
		/// <returns></returns>
		internal static string ToWooCommerceEndDateTime(this DateTime endDateTime)
		{
			return endDateTime.RoundDateUpToTopOfMinute().ToSortableDate();
		}

		/// <summary>
		/// Convert the date to "sortable" date/time format "s", since WooCommerce API sometimes doesn't filter correctly with the ISO format ("o").
		/// See tickets PBL-9276, PBL-9291
		/// Sortable format - <see href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#the-sortable-s-format-specifier"/>
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		private static string ToSortableDate(this DateTime dateTime)
		{
			return dateTime.ToString("s");
		}

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