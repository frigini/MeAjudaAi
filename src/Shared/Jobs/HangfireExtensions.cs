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
            if (!dashboardPath.StartsWith("/"))
            {
                dashboardPath = $"/{dashboardPath}";
                logger?.LogWarning("Dashboard path adjusted to start with '/': {DashboardPath}", dashboardPath);
            }
            logger?.LogInformation("Configuring Hangfire Dashboard at path: {DashboardPath}", dashboardPath);
            app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                StatsPollingInterval = 5000,
                DisplayStorageConnectionString = false
            });
        }

        return app;
    }
}
