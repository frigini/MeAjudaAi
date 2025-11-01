using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO completo do provider para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderDto
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
    /// Documento (CPF/CNPJ) do provider
    /// </summary>
    public required string Document { get; init; }

    /// <summary>
    /// Telefone do provider
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Tipo do provider
    /// </summary>
    public required EProviderType ProviderType { get; init; }

    /// <summary>
    /// Status de verificação
    /// </summary>
    public required EVerificationStatus VerificationStatus { get; init; }

    /// <summary>
    /// Data de criação
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Se o provider está ativo
    /// </summary>
    public required bool IsActive { get; init; }
}