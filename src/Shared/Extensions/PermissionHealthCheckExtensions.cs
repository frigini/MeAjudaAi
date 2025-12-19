using MeAjudaAi.Shared.Authorization.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Extensões para facilitar o registro do health check de permissões.
/// </summary>
public static class PermissionHealthCheckExtensions
{
    /// <summary>
    /// Tags para o health check do sistema de permissões.
    /// </summary>
    private static readonly string[] HealthCheckTags = ["permissions", "authorization", "security"];

    /// <summary>
    /// Adiciona o health check do sistema de permissões.
    /// </summary>
    public static IServiceCollection AddPermissionSystemHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PermissionSystemHealthCheck>(
                "permission_system",
                HealthStatus.Degraded,
                HealthCheckTags);

        return services;
    }
}
