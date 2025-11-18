using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    /// Requer que AddHangfire tenha sido chamado anteriormente.
    /// </summary>
    public static IApplicationBuilder UseHangfireDashboardIfEnabled(
        this IApplicationBuilder app,
        IConfiguration configuration,
        ILogger? logger = null)
    {
        var dashboardEnabled = configuration.GetValue<bool>(DashboardEnabledKey, false);

        // Se dashboard não está habilitado, não faz nada
        if (!dashboardEnabled)
        {
            logger?.LogDebug("Hangfire Dashboard is disabled");
            return app;
        }

        // Verifica se Hangfire foi configurado verificando se o serviço está disponível
        try
        {
            var serviceProvider = app.ApplicationServices;
            var jobClient = serviceProvider.GetService(typeof(IBackgroundJobClient));

            if (jobClient == null)
            {
                logger?.LogWarning("Hangfire Dashboard is enabled but AddHangfire was not called. Skipping dashboard configuration.");
                return app;
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to check for Hangfire services. Skipping dashboard configuration.");
            return app;
        }

        logger?.LogInformation("Hangfire Dashboard is enabled");
        logger?.LogInformation("Hangfire Dashboard is enabled");

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

        return app;
    }
}
