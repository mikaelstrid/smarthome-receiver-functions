using Microsoft.AspNetCore.Http;

namespace SmartHome.Functions.Receivers.Utilities
{
    public static class HttpRequestExtensions
    {
        public static void FixCorsHeaders(this HttpRequest request)
        {
            // Azure function doesn't support CORS well, workaround it by explicitly return CORS headers
            if (request.Headers["Origin"].Count > 0)
            {
                if (request.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                {
                    request.HttpContext.Response.Headers.Remove("Access-Control-Allow-Origin");
                }

                request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", request.Headers["Origin"][0]);
            }

            if (request.Headers["Access-Control-Request-Headers"].Count > 0)
            {
                if (request.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
                {
                    request.HttpContext.Response.Headers.Remove("Access-Control-Allow-Headers");
                }

                request.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers",
                    request.Headers["access-control-request-headers"][0]);
            }

            request.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
    }
}
