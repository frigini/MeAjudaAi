using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Domain.Entities;

/// <summary>
/// Entidade de junção many-to-many entre Provider e Service (do módulo ServiceCatalogs).
/// Representa os serviços que um provider oferece.
/// </summary>
public sealed class ProviderService
{
    /// <summary>
    /// ID do provider que oferece o serviço.
    /// </summary>
    public ProviderId ProviderId { get; private set; } = null!;

    /// <summary>
    /// ID do serviço do catálogo (referência ao módulo ServiceCatalogs).
    /// </summary>
    public Guid ServiceId { get; private set; }

    /// <summary>
    /// Data em que o provider adicionou este serviço.
    /// </summary>
    public DateTime AddedAt { get; private set; }

    /// <summary>
    /// Navigation property para o provider.
    /// </summary>
    public Provider? Provider { get; private set; }

    /// <summary>
    /// Construtor privado para EF Core.
    /// </summary>
    private ProviderService() { }

    /// <summary>
    /// Construtor interno para criação via métodos de domínio do Provider.
    /// </summary>
    internal ProviderService(ProviderId providerId, Guid serviceId)
    {
        ProviderId = providerId ?? throw new ArgumentNullException(nameof(providerId));

        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty.", nameof(serviceId));

        ServiceId = serviceId;
        AddedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma nova associação entre provider e serviço.
    /// </summary>
    public static ProviderService Create(ProviderId providerId, Guid serviceId)
    {
        return new ProviderService(providerId, serviceId);
    }
}
