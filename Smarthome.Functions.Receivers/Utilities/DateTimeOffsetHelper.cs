﻿using System;
using System.Runtime.InteropServices;

namespace Smarthome.Functions.Receivers.Utilities
{
    public class DateTimeOffsetHelper
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