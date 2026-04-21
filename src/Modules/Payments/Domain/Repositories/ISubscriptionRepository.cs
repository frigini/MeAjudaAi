using MeAjudaAi.Modules.Payments.Domain.Entities;

namespace MeAjudaAi.Modules.Payments.Domain.Repositories;

/// <summary>
/// Repositório para operações de persistência de assinaturas de prestadores.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Busca uma assinatura pelo seu identificador único.
    /// </summary>
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca a assinatura ativa de um prestador.
    /// </summary>
    Task<Subscription?> GetActiveByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca a assinatura mais recente de um prestador.
    /// </summary>
    Task<Subscription?> GetLatestByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca uma assinatura pelo seu identificador externo (ex: Stripe ID).
    /// </summary>
    Task<Subscription?> GetByExternalIdAsync(string externalSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova assinatura ao repositório.
    /// </summary>
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma assinatura existente.
    /// </summary>
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
