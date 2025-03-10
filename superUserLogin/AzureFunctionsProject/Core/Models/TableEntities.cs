// File: AzureFunctionsProject/Core/Models/TableEntities.cs

using System;
using Azure;
using Azure.Data.Tables;

namespace AzureFunctionsProject.Core.Models
{
    public class TenantEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // TenantID
        public string RowKey { get; set; } // TenantName
        public string Subdomain { get; set; }
        public string PrimaryContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string AddressDetails { get; set; }
        public string PlanOrTier { get; set; }
        public DateTime SubscriptionStartDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime SubscriptionEndDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string Status { get; set; }
        public string BillingInformation { get; set; }
        public string ConfigSettings { get; set; }
        public string DBConnectionString { get; set; }
        public string StorageContainerURL { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime UpdatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime? DeletedAt { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class UserEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // UserID
        public string RowKey { get; set; } // TenantID
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Changed from PasswordHash to Password
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string WhatsAppNumber { get; set; }
        public string RoleOrPermissions { get; set; }
        public string Status { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime LastLogin { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        public string ProfilePictureURL { get; set; }
        public string Locale { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime UpdatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class TenantUserEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // TenantID
        public string RowKey { get; set; } // TenantUserID
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime UpdatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}