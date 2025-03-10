// File: AzureFunctionsProject/Administration/SuperAdminSetupFunction.cs

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using AzureFunctionsProject.Core.Interfaces;
using AzureFunctionsProject.Core.Models;
using System.Net;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Configuration;
using static BCrypt.Net.BCrypt;

namespace AzureFunctionsProject.Administration
{
    public class SuperAdminSetupRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime UpdatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class SuperAdminSetupFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILoggingService _loggingService;
        private readonly string _auth0Domain;
        private readonly string _auth0ClientId;
        private readonly string _auth0ClientSecret;
        private readonly string _auth0Audience;

        public SuperAdminSetupFunction(
            ITableStorageService tableStorageService,
            ILoggingService loggingService,
            IConfiguration configuration)
        {
            _tableStorageService = tableStorageService;
            _loggingService = loggingService;
            _auth0Domain = configuration["Auth0:Domain"];
            _auth0ClientId = configuration["Auth0:ClientId"];
            _auth0ClientSecret = configuration["Auth0:ClientSecret"];
            _auth0Audience = $"https://{_auth0Domain}/api/v2/";
        }

        [Function("SetupSuperAdminTenant")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post")] HttpRequestData req)
        {
            _loggingService.LogInfo("SetupSuperAdminTenant function triggered.");

            try
            {
                // Ensure tables exist
                await _tableStorageService.EnsureTablesExistAsync();

                // Parse request
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var setupRequest = JsonConvert.DeserializeObject<SuperAdminSetupRequest>(requestBody) ?? new SuperAdminSetupRequest
                {
                    Email = "superadmin@yourdomain.com",
                    Password = "ChangeMe123!",
                    FirstName = "Super",
                    LastName = "Admin",
                    Username = "superadmin"
                };

                // 1. Create SuperAdmin user in Auth0
                string auth0UserId = await CreateAuth0User(setupRequest);
                if (string.IsNullOrEmpty(auth0UserId))
                {
                    throw new Exception("Failed to create SuperAdmin in Auth0.");
                }

                _loggingService.LogInfo($"SuperAdmin user created with Auth0 ID: {auth0UserId}");

                // 2. Assign SUPER_ADMIN role in Auth0
                await AssignRoleToUser(auth0UserId, "SUPER_ADMIN");

                // 3. Create SUPER_ADMIN tenant in Azure Table Storage
                var tenantEntity = new TenantEntity
                {
                    PartitionKey = "SUPER_ADMIN", // TenantID
                    RowKey = "SuperAdmin", // TenantName
                    Subdomain = "admin",
                    PrimaryContactName = $"{setupRequest.FirstName} {setupRequest.LastName}",
                    ContactEmail = setupRequest.Email,
                    PlanOrTier = "SystemTenant",
                    Status = "Active",
                    ConfigSettings = JsonConvert.SerializeObject(new { IsSystemTenant = true }),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    SubscriptionStartDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    SubscriptionEndDate = DateTime.SpecifyKind(DateTime.UtcNow.AddYears(10), DateTimeKind.Utc) // Long validity
                };

                await _tableStorageService.UpsertTenantAsync(tenantEntity);

                _loggingService.LogInfo("SUPER_ADMIN tenant created in Azure Table Storage.");

                // 4. Store SuperAdmin user in Azure Table Storage
                var userEntity = new UserEntity
                {
                    PartitionKey = auth0UserId, // Use Auth0 User ID as PartitionKey
                    RowKey = "SUPER_ADMIN", // TenantID
                    Username = setupRequest.Username,
                    Email = setupRequest.Email,
                    FirstName = setupRequest.FirstName,
                    LastName = setupRequest.LastName,
                    RoleOrPermissions = "SUPER_ADMIN",
                    Status = "Active",
                    EmailVerified = true,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                };

                await _tableStorageService.UpsertUserAsync(userEntity);

                _loggingService.LogInfo("SuperAdmin user stored in Azure Table Storage.");

                // 5. Create TenantUsers mapping
                var tenantUserEntity = new TenantUserEntity
                {
                    PartitionKey = "SUPER_ADMIN", // TenantID
                    RowKey = auth0UserId, // Use Auth0 User ID as TenantUserID
                    Role = "SUPER_ADMIN",
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                };

                await _tableStorageService.UpsertTenantUserAsync(tenantUserEntity);

                _loggingService.LogInfo("TenantUser mapping created in Azure Table Storage.");

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new
                {
                    message = "SuperAdmin tenant and user setup successfully",
                    tenantId = "SUPER_ADMIN",
                    userId = auth0UserId,
                    username = setupRequest.Username
                });
                return response;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error during SuperAdmin tenant setup: {ex.Message}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }

        private async Task<string> CreateAuth0User(SuperAdminSetupRequest setupRequest)
        {
            try
            {
                var tokenClient = new AuthenticationApiClient(_auth0Domain);
                var tokenResponse = await tokenClient.GetTokenAsync(new ClientCredentialsTokenRequest
                {
                    ClientId = _auth0ClientId,
                    ClientSecret = _auth0ClientSecret,
                    Audience = _auth0Audience
                });

                var managementApiClient = new ManagementApiClient(tokenResponse.AccessToken, new Uri(_auth0Audience));

                var newUser = await managementApiClient.Users.CreateAsync(new UserCreateRequest
                {
                    Connection = "Username-Password-Authentication",
                    Email = setupRequest.Email,
                    Password = setupRequest.Password,
                    EmailVerified = true,
                    FirstName = setupRequest.FirstName,
                    LastName = setupRequest.LastName,
                    UserMetadata = new { Role = "SUPER_ADMIN" }
                });

                _loggingService.LogInfo($"SuperAdmin user created in Auth0 with ID: {newUser.UserId}");

                return newUser.UserId;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error creating SuperAdmin in Auth0: {ex.Message}");
                throw;
            }
        }

        private async Task AssignRoleToUser(string auth0UserId, string roleName)
        {
            try
            {
                var tokenClient = new AuthenticationApiClient(_auth0Domain);
                var tokenResponse = await tokenClient.GetTokenAsync(new ClientCredentialsTokenRequest
                {
                    ClientId = _auth0ClientId,
                    ClientSecret = _auth0ClientSecret,
                    Audience = _auth0Audience
                });

                var managementApiClient = new ManagementApiClient(tokenResponse.AccessToken, new Uri(_auth0Audience));

                var roles = await managementApiClient.Roles.GetAllAsync(new GetRolesRequest());
                var superAdminRole = roles.FirstOrDefault(r => r.Name == roleName);

                if (superAdminRole == null)
                {
                    _loggingService.LogError("SUPER_ADMIN role not found in Auth0");
                    return;
                }

                await managementApiClient.Users.AssignRolesAsync(auth0UserId, new AssignRolesRequest
                {
                    Roles = new[] { superAdminRole.Id }
                });

                _loggingService.LogInfo($"Role SUPER_ADMIN assigned to Auth0 User: {auth0UserId}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error assigning role to SuperAdmin in Auth0: {ex.Message}");
            }
        }
    }
}