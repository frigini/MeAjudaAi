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
    /// Nome do serviço (desnormalizado para facilitar exibição).
    /// </summary>
    public string ServiceName { get; private set; } = string.Empty;

    /// <summary>
    /// Data em que o provider adicionou este serviço.
    /// </summary>
    public DateTime AddedAt { get; private set; }

    /// <summary>
    /// Navigation property para o provider.
    /// </summary>
    public Provider? Provider { get; }

    /// <summary>
    /// Construtor privado para EF Core.
    /// </summary>
    private ProviderService() { }

    /// <summary>
    /// Construtor interno para criação via métodos de domínio do Provider.
    /// </summary>
    internal ProviderService(ProviderId providerId, Guid serviceId, string serviceName)
    {
        ProviderId = providerId ?? throw new ArgumentNullException(nameof(providerId));

        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId cannot be empty.", nameof(serviceId));

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("ServiceName cannot be empty.", nameof(serviceName));

        ServiceId = serviceId;
        ServiceName = serviceName.Trim();
        AddedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma nova associação entre provider e serviço.
    /// </summary>
    public static ProviderService Create(ProviderId providerId, Guid serviceId, string serviceName)
    {
        return new ProviderService(providerId, serviceId, serviceName);
    }
}
