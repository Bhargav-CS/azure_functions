using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using AzureFunctionsProject.Core.Interfaces;

namespace AzureFunctionsProject.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;

        public AuthService(IConfiguration configuration, ILoggingService loggingService)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
            _loggingService = loggingService;
        }

        public async Task<string> AuthenticateAsync(string email, string password)
        {
            try
            {
                _loggingService.LogInfo($"Authenticating user: {email}");

                var auth0Domain = _configuration["Auth0:Domain"];
                var clientId = _configuration["Auth0:ClientId"];
                var clientSecret = _configuration["Auth0:ClientSecret"];

                var requestBody = new
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    audience = $"https://{auth0Domain}/api/v2/",
                    grant_type = "password",
                    username = email,
                    password = password,
                    scope = "openid profile email"
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"https://{auth0Domain}/oauth/token", content);

                if (!response.IsSuccessStatusCode)
                {
                    _loggingService.LogWarning($"Auth0 authentication failed for user: {email}");
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                return responseObject?.access_token;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error occurred during authentication.", ex);
                return null;
            }
        }
    }
}