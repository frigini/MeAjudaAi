using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Authorization.Metrics.Models;

/// <summary>
/// Estatísticas do sistema de permissões.
/// </summary>
public sealed class PermissionSystemStats
{
    public long TotalPermissionChecks { get; init; }
    public long TotalCacheHits { get; init; }
    public double CacheHitRate { get; init; }
    public int ActiveChecks { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
