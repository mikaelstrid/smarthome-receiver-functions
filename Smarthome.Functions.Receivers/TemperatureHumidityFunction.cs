using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartHome.Functions.Receivers.Models;
using SmartHome.Functions.Receivers.Utilities;

namespace SmartHome.Functions.Receivers
{
    public static class TemperatureHumidityFunction
    {
        [FunctionName("TemperatureHumidity")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            [Table("TemperatureHumidity")]out TemperatureHumidityReading output,
            [SignalR(HubName = "climate")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            req.FixCorsHeaders();

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

            var readAtUtc = DateTimeOffset.UtcNow;
            output = new TemperatureHumidityReading
            {
                PartitionKey = input.SensorId,
                RowKey = readAtUtc.ToRowKey(),
                ReadAtUtc = readAtUtc,
                Temperature = input.Temperature,
                Humidity = input.Humidity
            };

            signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "notifyTemperatureHumidityUpdated",
                    Arguments = new object[] { new
                        {
                            sensorId = input.SensorId,
                            timestampWest = DateTimeOffsetHelper.ConvertToWest(readAtUtc),
                            temperature = input.Temperature,
                            humidity = input.Humidity
                        }
                    }
                }).Wait();

            return new OkResult();
        }

        private class TemperatureHumidityInput
        {
            public string SensorId { get; set; }
            public double Temperature { get; set; }
            public double Humidity { get; set; }
        }
    }
}
