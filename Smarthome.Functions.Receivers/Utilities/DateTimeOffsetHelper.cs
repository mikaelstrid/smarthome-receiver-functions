using System;
using System.Runtime.InteropServices;

namespace SmartHome.Functions.Receivers.Utilities
{
    public static class DateTimeOffsetHelper
    {
        private static readonly TimeZoneInfo WestTimeZoneInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time") :
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

        public static DateTime ConvertToWest(DateTimeOffset dateTimeOffset)
        {
            var westDateTimeOffset = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeOffset, WestTimeZoneInfo.Id);
            return westDateTimeOffset.DateTime;
        }
    }
}