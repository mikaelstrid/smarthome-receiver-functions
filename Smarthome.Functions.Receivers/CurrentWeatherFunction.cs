using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SmartHome.Functions.Receivers.Models;

namespace SmartHome.Functions.Receivers
{
    public static class CurrentWeatherFunction
    {
        [FunctionName("CurrentWeatherFunction")]
        public static void Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Table("CurrentWeather")]out CurrentWeatherReport output,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            output = null;
        }
    }
}
