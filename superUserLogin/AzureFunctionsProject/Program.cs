using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AzureFunctionsProject.Core.Interfaces;
using AzureFunctionsProject.Core.Services;
using AzureFunctionsProject.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<ITableStorageService, TableStorageService>();
        services.AddLogging();
    })
    .Build();

host.Run();
