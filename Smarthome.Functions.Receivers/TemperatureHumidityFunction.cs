using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Smarthome.Functions.Receivers
{
    public static class TemperatureHumidityFunction
    {
        [FunctionName("TemperatureHumidity")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            [Table("TemperatureHumidity")]out TemperatureHumidityOutput output,
            ILogger log)
        {
            output = null;

            string requestBody;
            try
            {
                requestBody = new StreamReader(req.Body).ReadToEnd();
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Could not read the body of the request");
                return new BadRequestObjectResult("Could not read the body of the request");
            }

            TemperatureHumidityInput input;
            try
            {
                input = JsonConvert.DeserializeObject<TemperatureHumidityInput>(requestBody);
            }
            catch (Exception e)
            {
                log.LogWarning(e, "The received json data ({Json}) could not be parsed", requestBody);
                return new BadRequestObjectResult("The received json data could not be parsed");
            }

            if (string.IsNullOrWhiteSpace(input.SensorId))
            {
                log.LogWarning("Sensor id missing");
                return new BadRequestObjectResult("Sensor id missing");
            }

            var readAt = DateTimeOffset.UtcNow;
            output = new TemperatureHumidityOutput
            {
                PartitionKey = input.SensorId,
                RowKey = readAt.ToString("yyyyMMddHHmmss"),
                ReadAt = readAt,
                Temperature = input.Temperature,
                Humidity = input.Humidity
            };

            return new OkResult();
        }

        private class TemperatureHumidityInput
        {
            public string SensorId { get; set; }
            public float Temperature { get; set; }
            public float Humidity { get; set; }
        }

        public class TemperatureHumidityOutput
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset ReadAt { get; set; }
            public float Temperature { get; set; }
            public float Humidity { get; set; }
        }
    }
}
