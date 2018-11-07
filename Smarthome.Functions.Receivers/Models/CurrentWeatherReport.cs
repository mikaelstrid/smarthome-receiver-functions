using System;
// ReSharper disable InconsistentNaming

namespace SmartHome.Functions.Receivers.Models
{
    public class CurrentWeatherReport
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset ReadAtUtc { get; set; }
        //public string Data { get; set; }
        public int WeatherId { get; set; }
        //public string WeatherMain { get; set; }
        public string WeatherDescription { get; set; }
        public string WeatherIcon { get; set; }
        public double MainTemp { get; set; }
        public double MainPressure { get; set; }
        public double MainHumidity { get; set; }
        public double WindSpeed { get; set; }
        //public double WinDeg { get; set; }
        public double CloudsAll { get; set; }
        //public double Rain3h { get; set; }
        //public double Snow3h { get; set; }
        public DateTimeOffset SunriseUtc { get; set; }
        public DateTimeOffset SunsetUtc { get; set; }
    }
}