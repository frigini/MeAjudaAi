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
    private const string DashboardEnabledKey = "Hangfire:DashboardEnabled";
    private const string DashboardPathKey = "Hangfire:DashboardPath";
    private const string StatsPollingIntervalKey = "Hangfire:StatsPollingInterval";
    private const string DisplayConnectionStringKey = "Hangfire:DisplayStorageConnectionString";
    
    /// <summary>
    /// Configura o Hangfire Dashboard se habilitado na configuração.
    /// </summary>
    public static IApplicationBuilder UseHangfireDashboardIfEnabled(
        this IApplicationBuilder app, 
        IConfiguration configuration,
        ILogger? logger = null)
    {
        var dashboardEnabled = configuration.GetValue<bool>(DashboardEnabledKey, false);
        logger?.LogInformation("Hangfire Dashboard is {Status}", dashboardEnabled ? "enabled" : "disabled");
        
        if (dashboardEnabled)
        {
            var dashboardPath = configuration.GetValue<string>(DashboardPathKey, "/hangfire");
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
            
            var statsPollingInterval = configuration.GetValue<int>(StatsPollingIntervalKey, 5000);
            if (statsPollingInterval <= 0)
            {
                statsPollingInterval = 5000;
                logger?.LogWarning("Invalid StatsPollingInterval, using default: {Interval}", statsPollingInterval);
            }
            var displayConnectionString = configuration.GetValue<bool>(DisplayConnectionStringKey, false);
            
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
