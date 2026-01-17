using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;

namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// DTO otimizado para indexação de providers no módulo SearchProviders.
/// Contém todos os dados necessários para criar/atualizar um SearchableProvider.
/// </summary>
public sealed record ModuleProviderIndexingDto
{
    /// <summary>
    /// ID único do provider
    /// </summary>
    public required Guid ProviderId { get; init; }

    /// <summary>
    /// Nome do provider para exibição nos resultados de busca
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descrição/bio do provider
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Latitude da localização do provider
    /// </summary>
    public required double Latitude { get; init; }

    /// <summary>
    /// Longitude da localização do provider
    /// </summary>
    public required double Longitude { get; init; }

    /// <summary>
    /// IDs dos serviços oferecidos pelo provider
    /// </summary>
    public required IReadOnlyCollection<Guid> ServiceIds { get; init; }

    /// <summary>
    /// Avaliação média (0-5)
    /// </summary>
    public required decimal AverageRating { get; init; }

    /// <summary>
    /// Número total de avaliações
    /// </summary>
    public required int TotalReviews { get; init; }

    /// <summary>
    /// Tier de assinatura do provider
    /// </summary>
    public required ESubscriptionTier SubscriptionTier { get; init; }

    /// <summary>
    /// Cidade onde o provider está localizado
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Estado onde o provider está localizado
    /// </summary>
    public string? State { get; init; }

    /// <summary>
    /// Indica se o provider está ativo
    /// </summary>
    public required bool IsActive { get; init; }
}

