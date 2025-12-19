namespace MeAjudaAi.Shared.Authorization.HealthChecks.Models;

/// <summary>
/// Resultado da verificação de saúde de performance do sistema de permissões.
/// </summary>
internal sealed record PerformanceHealthResult
{
    /// <summary>
    /// Indica se a performance está dentro dos limites aceitáveis.
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// Status textual da verificação.
    /// </summary>
    public string Status { get; init; } = "";

    /// <summary>
    /// Descrição do problema, se houver.
    /// </summary>
    public string Issue { get; init; } = "";

    /// <summary>
    /// Taxa de acerto do cache (0.0 a 1.0).
    /// </summary>
    public double CacheHitRate { get; init; }

    /// <summary>
    /// Número de verificações ativas no momento.
    /// </summary>
    public int ActiveChecks { get; init; }
}
