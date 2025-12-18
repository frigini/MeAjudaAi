namespace MeAjudaAi.Shared.Authorization.HealthChecks.Models;

/// <summary>
/// Resultado interno de verificação de saúde.
/// </summary>
internal sealed record InternalHealthCheckResult(bool IsHealthy, string Issue)
{
    /// <summary>
    /// Status textual da verificação.
    /// </summary>
    public string Status => IsHealthy ? "healthy" : "unhealthy";
}
