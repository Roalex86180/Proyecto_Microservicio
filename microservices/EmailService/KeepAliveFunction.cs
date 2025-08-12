using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EmailService.Functions
{
    public class KeepAliveFunction
    {
        private readonly ILogger<KeepAliveFunction> _logger;

        public KeepAliveFunction(ILogger<KeepAliveFunction> logger)
        {
            _logger = logger;
        }

        [Function("KeepAlive")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "keepalive")] HttpRequestData req)
        {
            _logger.LogInformation("Keep-alive request received.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("EmailService is running.");

            return response;
        }
    }
}