using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using AzureFunctionsProject.Core.Interfaces;
using AzureFunctionsProject.Core.Models;

namespace Authentication
{
    public class AuthFunction
    {
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;

        public AuthFunction(IAuthService authService, ILoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
        }

        [Function("SuperUserLogin")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _loggingService.LogInfo("SuperUserLogin function triggered.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var loginRequest = JsonConvert.DeserializeObject<LoginRequest>(requestBody);
            var token = await _authService.AuthenticateAsync(loginRequest.Email, loginRequest.Password);
            
            var response = req.CreateResponse(token != null ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Unauthorized);
            await response.WriteStringAsync(JsonConvert.SerializeObject(new { token }));
            return response;
        }
    }
}