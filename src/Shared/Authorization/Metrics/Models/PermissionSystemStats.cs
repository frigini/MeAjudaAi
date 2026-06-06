using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Authorization.Metrics.Models;

/// <summary>
/// Estatísticas do sistema de permissões.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PermissionSystemStats
{
    public long TotalPermissionChecks { get; init; }
    public long TotalCacheHits { get; init; }
    public double CacheHitRate { get; init; }
    public int ActiveChecks { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
