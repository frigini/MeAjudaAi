namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// DTO com informações básicas do provider para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderBasicDto
{
    /// <summary>
    /// ID único do provider
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Nome do provider
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Email do provider
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Tipo do provider
    /// </summary>
    public required string ProviderType { get; init; }

    /// <summary>
    /// Status de verificação
    /// </summary>
    public required string VerificationStatus { get; init; }

    /// <summary>
    /// Se o provider está ativo
    /// </summary>
    public required bool IsActive { get; init; }
}

