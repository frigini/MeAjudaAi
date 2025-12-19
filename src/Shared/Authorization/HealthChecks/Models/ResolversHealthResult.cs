namespace MeAjudaAi.Shared.Authorization.HealthChecks.Models;

/// <summary>
/// Resultado da verificação de saúde dos resolvers de módulos.
/// </summary>
internal sealed record ResolversHealthResult
{
    /// <summary>
    /// Indica se os resolvers estão funcionando corretamente.
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
    /// Número de resolvers registrados.
    /// </summary>
    public int ResolverCount { get; init; }
}
