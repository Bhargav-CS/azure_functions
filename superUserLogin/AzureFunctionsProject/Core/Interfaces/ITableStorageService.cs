// File: AzureFunctionsProject/Core/Interfaces/ITableStorageService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunctionsProject.Core.Models;

namespace AzureFunctionsProject.Core.Interfaces
{
    public interface ITableStorageService
    {
        Task EnsureTablesExistAsync();
        Task<TenantEntity> GetTenantAsync(string tenantId, string tenantName);
        Task<UserEntity> GetUserByEmailAsync(string email, string tenantId = null);
        Task<IEnumerable<UserEntity>> GetUsersByTenantAsync(string tenantId);
        Task<TenantUserEntity> GetTenantUserAsync(string tenantId, string userId);
        Task UpsertTenantAsync(TenantEntity tenant);
        Task UpsertUserAsync(UserEntity user);
        Task UpsertTenantUserAsync(TenantUserEntity tenantUser);
    }
}