using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using SmartHome.Functions.Receivers.Utilities;

namespace SmartHome.Functions.Receivers
{
    public static class NegotiateFunction
    {
        [FunctionName("negotiate")]
        public static IActionResult GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", "POST", "OPTIONS")]HttpRequest req,
            [SignalRConnectionInfo(HubName = "climate")]SignalRConnectionInfo connectionInfo)
        {
            req.FixCorsHeaders();
            return new OkObjectResult(connectionInfo);
        }
    }
}
