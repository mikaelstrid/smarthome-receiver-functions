using System;
using System.Runtime.InteropServices;

namespace Smarthome.Functions.Receivers.Utilities
{
    public class DateTimeHelper
    {
        private static readonly TimeZoneInfo WestTimeZoneInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time") :
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

        public static DateTime ParseWestToUtc(string s)
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(s), WestTimeZoneInfo);
        }

        public static DateTime ConvertWestToUtc(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, WestTimeZoneInfo);
        }

        public static DateTime? ConvertWestToUtc(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return TimeZoneInfo.ConvertTimeToUtc(dateTime.Value, WestTimeZoneInfo);
        }

        public static DateTime ConvertUtcToWest(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime, WestTimeZoneInfo);
        }

        public static DateTime? ConvertUtcToWest(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return TimeZoneInfo.ConvertTime(dateTime.Value, WestTimeZoneInfo);
        }

        public static DateTimeOffset ConvertUtcToWestOffset(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), WestTimeZoneInfo);
        }

        public static DateTimeOffset? ConvertUtcToWestOffset(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime.Value), WestTimeZoneInfo);
        }
    }
}