using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.Search.Domain.Entities;

/// <summary>
/// Modelo de leitura para busca de provedores.
/// Entidade desnormalizada otimizada para consultas de geolocalização e ranking.
/// </summary>
public sealed class SearchableProvider : AggregateRoot<SearchableProviderId>
{
    /// <summary>
    /// Referência ao ID do provedor original no módulo Providers.
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// Nome do provedor para exibição nos resultados de busca.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Localização geográfica do provedor.
    /// </summary>
    public GeoPoint Location { get; private set; } = null!;

    /// <summary>
    /// Avaliação média das avaliações de clientes (0-5).
    /// </summary>
    public decimal AverageRating { get; private set; }

    /// <summary>
    /// Número total de avaliações recebidas.
    /// </summary>
    public int TotalReviews { get; private set; }

    /// <summary>
    /// Tier de assinatura atual que afeta o ranking de busca.
    /// </summary>
    public ESubscriptionTier SubscriptionTier { get; private set; }

    /// <summary>
    /// Lista de IDs de serviços que este provedor oferece.
    /// Armazenada como array para filtragem eficiente.
    /// </summary>
    public Guid[] ServiceIds { get; private set; } = Array.Empty<Guid>();

    /// <summary>
    /// Indica se o provedor está atualmente ativo e deve aparecer nos resultados de busca.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Descrição/bio do provedor para exibição nos resultados de busca.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Cidade onde o provedor está localizado.
    /// </summary>
    public string? City { get; private set; }

    /// <summary>
    /// Estado/província onde o provedor está localizado.
    /// </summary>
    public string? State { get; private set; }

    // Construtor privado para EF Core
    private SearchableProvider()
    {
    }

    private SearchableProvider(
        SearchableProviderId id,
        Guid providerId,
        string name,
        GeoPoint location,
        ESubscriptionTier subscriptionTier) : base(id)
    {
        ProviderId = providerId;
        Name = name;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        SubscriptionTier = subscriptionTier;
        AverageRating = 0;
        TotalReviews = 0;
        IsActive = true;
        ServiceIds = Array.Empty<Guid>();
    }

    /// <summary>
    /// Cria uma nova entrada de provedor pesquisável.
    /// </summary>
    public static SearchableProvider Create(
        Guid providerId,
        string name,
        GeoPoint location,
        ESubscriptionTier subscriptionTier = ESubscriptionTier.Free,
        string? description = null,
        string? city = null,
        string? state = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(location);

        var searchableProvider = new SearchableProvider(
            SearchableProviderId.New(),
            providerId,
            name.Trim(),
            location,
            subscriptionTier)
        {
            Description = description?.Trim(),
            City = city?.Trim(),
            State = state?.Trim()
        };

        return searchableProvider;
    }

    /// <summary>
    /// Atualiza as informações básicas do provedor.
    /// </summary>
    public void UpdateBasicInfo(string name, string? description, string? city, string? state)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
        City = city?.Trim();
        State = state?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Atualiza a localização do provedor.
    /// </summary>
    public void UpdateLocation(GeoPoint location)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        MarkAsUpdated();
    }

    /// <summary>
    /// Atualiza a avaliação do provedor com base em novos dados de avaliação.
    /// </summary>
    public void UpdateRating(decimal averageRating, int totalReviews)
    {
        if (averageRating < 0 || averageRating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(averageRating), "Rating must be between 0 and 5.");
        }

        if (totalReviews < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalReviews), "Total reviews cannot be negative.");
        }

        AverageRating = averageRating;
        TotalReviews = totalReviews;
        MarkAsUpdated();
    }

    /// <summary>
    /// Atualiza o tier de assinatura do provedor.
    /// </summary>
    public void UpdateSubscriptionTier(ESubscriptionTier tier)
    {
        SubscriptionTier = tier;
        MarkAsUpdated();
    }

    /// <summary>
    /// Atualiza a lista de serviços oferecidos pelo provedor.
    /// </summary>
    public void UpdateServices(Guid[] serviceIds)
    {
        ServiceIds = serviceIds?.ToArray() ?? Array.Empty<Guid>();
        MarkAsUpdated();
    }

    /// <summary>
    /// Ativa o provedor nos resultados de busca.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Desativa o provedor dos resultados de busca.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Calcula a distância até uma localização especificada em quilômetros.
    /// </summary>
    public double CalculateDistanceToInKm(GeoPoint targetLocation)
    {
        ArgumentNullException.ThrowIfNull(targetLocation);

        return Location.DistanceTo(targetLocation);
    }
}
