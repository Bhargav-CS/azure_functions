using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AzureFunctionsProject.Core.Interfaces;

namespace AzureFunctionsProject.SampleFunctions
{
    public class SampleFunction
    {
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;

        public SampleFunction(IAuthService authService, ILoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
        }

        [Function("SampleFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SampleFunction");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            logger.LogInformation("SampleFunction executed.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            var responseBody = new { message = "Hello from SampleFunction!" };
            await response.WriteAsJsonAsync(responseBody);

            return response;
        }
    }
}