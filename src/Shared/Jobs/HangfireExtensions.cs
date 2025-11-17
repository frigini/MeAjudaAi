using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

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
        IConfiguration configuration)
    {
        var dashboardEnabled = configuration.GetValue<bool>("Hangfire:DashboardEnabled", false);
        if (dashboardEnabled)
        {
            var dashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
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
