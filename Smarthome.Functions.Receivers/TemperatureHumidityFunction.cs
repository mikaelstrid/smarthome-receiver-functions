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
using Smarthome.Functions.Receivers.Utilities;

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
            FixCorsHeaders(req);

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
                RowKey = $"{DateTimeOffset.MaxValue.Ticks - readAtUtc.Ticks:D19}", //readAtUtc.Ticks.ToString("D19")
                ReadAtUtc = readAtUtc,
                Temperature = input.Temperature,
                Humidity = input.Humidity
            };

            signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "update-temperature-humidity",
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

        [FunctionName("negotiate")]
        public static IActionResult GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", "POST", "OPTIONS")]HttpRequest req,
            [SignalRConnectionInfo(HubName = "climate")]SignalRConnectionInfo connectionInfo)
        {
            FixCorsHeaders(req);
            return new OkObjectResult(connectionInfo);
        }

        private static void FixCorsHeaders(HttpRequest req)
        {
            // Azure function doesn't support CORS well, workaround it by explicitly return CORS headers
            if (req.Headers["Origin"].Count > 0)
            {
                if (req.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                {
                    req.HttpContext.Response.Headers.Remove("Access-Control-Allow-Origin");
                }

                req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", req.Headers["Origin"][0]);
            }

            if (req.Headers["Access-Control-Request-Headers"].Count > 0)
            {
                if (req.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
                {
                    req.HttpContext.Response.Headers.Remove("Access-Control-Allow-Headers");
                }

                req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers",
                    req.Headers["access-control-request-headers"][0]);
            }

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }

        private class TemperatureHumidityInput
        {
            public string SensorId { get; set; }
            public double Temperature { get; set; }
            public double Humidity { get; set; }
        }
    }
}
