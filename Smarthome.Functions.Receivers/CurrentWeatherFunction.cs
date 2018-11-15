using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SmartHome.Functions.Receivers.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using SmartHome.Functions.Receivers.Utilities;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace SmartHome.Functions.Receivers
{
    public static class CurrentWeatherFunction
    {
        private const string AppId = "f9e4db88204ce51d64b4f2aae72263d9";
        private const string City = "lindome";

        private static readonly HttpClient _httpClient = new HttpClient();

        [FunctionName("CurrentWeatherFunction")]
        public static async Task Run(
            [TimerTrigger("0 */10 * * * *")]TimerInfo myTimer,
            [Table("CurrentWeather")]ICollector<CurrentWeatherReport> tableBinding,
            [SignalR(HubName = "climate")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var readAtUtc = DateTimeOffset.UtcNow;
            log.LogInformation($"C# Timer trigger function 'CurrentWeatherFunction' executed at: {readAtUtc} (UTC)");

            var receivedCurrentWeatherData = await GetCurrentWeatherData(log);
            if (receivedCurrentWeatherData == null)
                return;

            var temperature = receivedCurrentWeatherData.main?.temp;
            var weather = receivedCurrentWeatherData.weather.FirstOrDefault();
            if (!temperature.HasValue || weather == null)
            {
                log.LogError("Weather data does not contain temperature or weather entry");
                return;
            }

            var successTableStorage = AddToTableStorage(tableBinding, log, temperature.Value, weather, readAtUtc);
            if (!successTableStorage)
                return;
            
            await SendToSignalR(signalRMessages, log, readAtUtc, temperature.Value, weather);
        }

        private static async Task<CurrentWeatherData> GetCurrentWeatherData(ILogger log)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={City},se&units=metric&APPID={AppId}&lang=se");
                if (!response.IsSuccessStatusCode)
                {
                    log.LogError($"Error when getting weather from API, {response.StatusCode.ToString()}");
                    return null;
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception when getting weather from API");
                return null;
            }


            CurrentWeatherData receivedCurrentWeatherData;
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                receivedCurrentWeatherData = JsonConvert.DeserializeObject<CurrentWeatherData>(content);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception when reading and deserializing received current weather data");
                return null;
            }

            return receivedCurrentWeatherData;
        }

        private static bool AddToTableStorage(ICollector<CurrentWeatherReport> tableBinding, ILogger log, double temperature, Weather weather, DateTimeOffset readAtUtc)
        {
            try
            {
                tableBinding.Add(
                    new CurrentWeatherReport
                    {
                        PartitionKey = City,
                        RowKey = readAtUtc.ToRowKey(),
                        Temperature = temperature,
                        ReadAtUtc = readAtUtc,
                        WeatherId = weather.id,
                        WeatherDescription = weather.description,
                        WeatherIcon = weather.icon
                    }
                );
            }
            catch (Exception e)
            {
                log.LogError(e, "Error when storing current weather in table");
                return false;
            }

            return true;
        }

        private static async Task<bool> SendToSignalR(IAsyncCollector<SignalRMessage> signalRMessages, ILogger log, DateTimeOffset readAtUtc, double temperature, Weather weather)
        {
            try
            {
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "notifyCurrentWeatherUpdated",
                        Arguments = new object[]
                        {
                            new
                            {
                                city = City,
                                timestampWest = DateTimeOffsetHelper.ConvertToWest(readAtUtc),
                                temperature,
                                weatherId = weather.id,
                                weatherDescription = weather.description,
                                weatherIcon = weather.icon
                            }
                        }
                    });
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, "Error when sending current weather to signalr");
                return false;
            }
        }


        private class CurrentWeatherData
        {
            public Coord coord { get; set; }
            public Weather[] weather { get; set; }
            public string _base { get; set; }
            public Main main { get; set; }
            public int visibility { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public int dt { get; set; }
            public Sys sys { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int cod { get; set; }
        }

        private class Coord
        {
            public double lon { get; set; }
            public double lat { get; set; }
        }

        private class Main
        {
            public double temp { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public double temp_min { get; set; }
            public double temp_max { get; set; }
        }

        private class Wind
        {
            public double speed { get; set; }
            public int deg { get; set; }
            public double gust { get; set; }
        }

        private class Clouds
        {
            public int all { get; set; }
        }

        private class Sys
        {
            public int type { get; set; }
            public int id { get; set; }
            public double message { get; set; }
            public string country { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
        }

        private class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }
    }
}
