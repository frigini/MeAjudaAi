using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Jobs;

/// <summary>
/// Extensões para configuração do Hangfire Dashboard.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Configura o Hangfire Dashboard se habilitado na configuração.
    /// </summary>
    public static IApplicationBuilder UseHangfireDashboardIfEnabled(
        this IApplicationBuilder app, 
        IConfiguration configuration,
        ILogger? logger = null)
    {
        var dashboardEnabled = configuration.GetValue<bool>("Hangfire:DashboardEnabled", false);
        logger?.LogInformation("Hangfire Dashboard is {Status}", dashboardEnabled ? "enabled" : "disabled");
        
        if (dashboardEnabled)
        {
            var dashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
            if (string.IsNullOrWhiteSpace(dashboardPath))
            {
                dashboardPath = "/hangfire";
                logger?.LogWarning("Dashboard path was empty, using default: {DashboardPath}", dashboardPath);
            }
            if (!dashboardPath.StartsWith("/"))
            {
                dashboardPath = $"/{dashboardPath}";
                logger?.LogWarning("Dashboard path adjusted to start with '/': {DashboardPath}", dashboardPath);
            }
            
            var statsPollingInterval = configuration.GetValue<int>("Hangfire:StatsPollingInterval", 5000);
            var displayConnectionString = configuration.GetValue<bool>("Hangfire:DisplayStorageConnectionString", false);
            
            logger?.LogInformation("Configuring Hangfire Dashboard at path: {DashboardPath}", dashboardPath);
            app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                StatsPollingInterval = statsPollingInterval,
                DisplayStorageConnectionString = displayConnectionString
            });
        }

        return app;
    }
}
