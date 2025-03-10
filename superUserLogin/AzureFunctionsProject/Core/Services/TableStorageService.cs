// File: AzureFunctionsProject/Services/TableStorageService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using AzureFunctionsProject.Core.Interfaces;
using AzureFunctionsProject.Core.Models;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionsProject.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILoggingService _loggingService;
        
        public TableStorageService(IConfiguration configuration, ILoggingService loggingService)
        {
            string connectionString = configuration["AzureTableStorage:ConnectionString"];
            _tableServiceClient = new TableServiceClient(connectionString);
            _loggingService = loggingService;
        }

        public async Task EnsureTablesExistAsync()
        {
            try
            {
                await _tableServiceClient.CreateTableIfNotExistsAsync("Tenants");
                await _tableServiceClient.CreateTableIfNotExistsAsync("Users");
                await _tableServiceClient.CreateTableIfNotExistsAsync("TenantUsers");
                _loggingService.LogInfo("Tables created or verified successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error ensuring tables exist: {ex.Message}");
                throw;
            }
        }

        public async Task<TenantEntity> GetTenantAsync(string tenantId, string tenantName)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Tenants");
                return await tableClient.GetEntityAsync<TenantEntity>(tenantId, tenantName);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error retrieving tenant: {ex.Message}");
                return null;
            }
        }

        public async Task<UserEntity> GetUserByEmailAsync(string email, string tenantId = null)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Users");
                
                // Query by email
                var query = tableClient.QueryAsync<UserEntity>(user => user.Email == email);
                
                // If tenantId is specified, filter by it
                if (!string.IsNullOrEmpty(tenantId))
                {
                    var users = (await query.ToListAsync()).Where(user => user.RowKey == tenantId).ToList();
                    return users.FirstOrDefault();
                }
                else
                {
                    var users = await query.ToListAsync();
                    return users.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error retrieving user by email: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<UserEntity>> GetUsersByTenantAsync(string tenantId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Users");
                var query = tableClient.QueryAsync<UserEntity>(user => user.RowKey == tenantId);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error retrieving users by tenant: {ex.Message}");
                return Enumerable.Empty<UserEntity>();
            }
        }

        public async Task<TenantUserEntity> GetTenantUserAsync(string tenantId, string userId)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("TenantUsers");
                return await tableClient.GetEntityAsync<TenantUserEntity>(tenantId, userId);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error retrieving tenant user: {ex.Message}");
                return null;
            }
        }

        public async Task UpsertTenantAsync(TenantEntity tenant)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Tenants");
                await tableClient.UpsertEntityAsync(tenant);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error upserting tenant: {ex.Message}");
                throw;
            }
        }

        public async Task UpsertUserAsync(UserEntity user)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Users");
                await tableClient.UpsertEntityAsync(user);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error upserting user: {ex.Message}");
                throw;
            }
        }

        public async Task UpsertTenantUserAsync(TenantUserEntity tenantUser)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("TenantUsers");
                await tableClient.UpsertEntityAsync(tenantUser);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error upserting tenant user: {ex.Message}");
                throw;
            }
        }
    }

    // Extension method to make it easier to work with async enumerables
    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items)
        {
            var results = new List<T>();
            await foreach (var item in items)
            {
                results.Add(item);
            }
            return results;
        }
    }
}