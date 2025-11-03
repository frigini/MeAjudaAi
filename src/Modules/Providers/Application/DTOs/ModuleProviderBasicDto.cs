using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO com informações básicas do provider para comunicação entre módulos
/// TEMP: Este é um DTO temporário que deve ser removido quando o namespace for resolvido
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
    public required EProviderType ProviderType { get; init; }

    /// <summary>
    /// Status de verificação
    /// </summary>
    public required EVerificationStatus VerificationStatus { get; init; }

    /// <summary>
    /// Se o provider está ativo
    /// </summary>
    public required bool IsActive { get; init; }
}
