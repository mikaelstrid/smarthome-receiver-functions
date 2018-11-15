using System;

namespace SmartHome.Functions.Receivers.Models
{
    public class CurrentWeatherReport
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset ReadAtUtc { get; set; }
        public int WeatherId { get; set; }
        public string WeatherDescription { get; set; }
        public string WeatherIcon { get; set; }
        public double Temperature { get; set; }
    }
}