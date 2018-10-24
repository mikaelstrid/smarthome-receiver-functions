using System;

namespace Smarthome.Functions.Receivers.Models
{
    public class TemperatureHumidityReading
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset ReadAtUtc { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
