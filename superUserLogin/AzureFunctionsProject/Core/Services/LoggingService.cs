using System;
using Microsoft.Extensions.Logging;
using AzureFunctionsProject.Core.Interfaces;

namespace AzureFunctionsProject.Core.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message) => _logger.LogInformation(message);
        public void LogWarning(string message) => _logger.LogWarning(message);
        public void LogError(string message, Exception ex = null) => _logger.LogError(ex, message);
    }
}