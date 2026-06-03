namespace MeAjudaAi.Contracts.Configuration;

/// <summary>
/// URLs de recursos externos.
/// </summary>
public sealed record ExternalResources
{
    /// <summary>
    /// URL da documentação/help center (se houver).
    /// </summary>
    public string? DocumentationUrl { get; init; }

    /// <summary>
    /// URL do suporte/support portal (se houver).
    /// </summary>
    public string? SupportUrl { get; init; }
}
