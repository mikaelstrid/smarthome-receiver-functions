using System;

namespace SmartHome.Functions.Receivers.Utilities
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToRowKey(this DateTimeOffset value) => $"{DateTimeOffset.MaxValue.Ticks - value.Ticks:D19}";
    }
}
