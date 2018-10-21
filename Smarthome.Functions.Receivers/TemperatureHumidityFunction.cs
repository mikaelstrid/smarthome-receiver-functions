using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smarthome.Functions.Receivers.Models;
using System;
using System.IO;

namespace Smarthome.Functions.Receivers
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
                RowKey = readAtUtc.Ticks.ToString("D19"),
                ReadAtUtc = readAtUtc,
                Temperature = input.Temperature,
                Humidity = input.Humidity
            };

            signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "update-temperature-humidity",
                    Arguments = new object[] { input.SensorId, readAtUtc, input.Temperature, input.Humidity }
                }).Wait();

            return new OkResult();
        }
        
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "climate")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        private class TemperatureHumidityInput
        {
            public string SensorId { get; set; }
            public double Temperature { get; set; }
            public double Humidity { get; set; }
        }
    }
}
