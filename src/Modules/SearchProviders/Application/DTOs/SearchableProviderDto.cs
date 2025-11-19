using MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representando um provedor pesquisável nos resultados de busca.
/// </summary>
public sealed record SearchableProviderDto
{
    /// <summary>
    /// Identificador único do provedor.
    /// </summary>
    public required Guid ProviderId { get; init; }

    /// <summary>
    /// Nome do provedor.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descrição/bio do provedor.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Coordenadas geográficas.
    /// </summary>
    public required LocationDto Location { get; init; }

    /// <summary>
    /// Avaliação média dos clientes (0-5).
    /// </summary>
    public decimal AverageRating { get; init; }

    /// <summary>
    /// Número total de avaliações.
    /// </summary>
    public int TotalReviews { get; init; }

    /// <summary>
    /// Tier de assinatura.
    /// </summary>
    public ESubscriptionTier SubscriptionTier { get; init; }

    /// <summary>
    /// Lista de IDs de serviços oferecidos por este provedor.
    /// </summary>
    public IReadOnlyList<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Distância da localização de busca em quilômetros.
    /// Preenchido somente ao buscar por localização.
    /// </summary>
    public double? DistanceInKm { get; init; }

    /// <summary>
    /// Cidade onde o provedor está localizado.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Estado/província onde o provedor está localizado.
    /// </summary>
    public string? State { get; init; }
}
