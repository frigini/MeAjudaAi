using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Domain.Abstractions;

/// <summary>
/// Interface para comunicação com o provedor de pagamento externo (ex: Stripe).
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Cria uma nova assinatura no provedor de pagamento.
    /// </summary>
    /// <param name="providerId">Identificador do provider.</param>
    /// <param name="planId">Identificador do plano no provedor.</param>
    /// <param name="amount">Valor monetário da assinatura.</param>
    /// <param name="idempotencyKey">Chave de idempotência opcional.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resposta com dados da assinatura criada ou erro.</returns>
    Task<SubscriptionGatewayResponse> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela uma assinatura no provedor de pagamento.
    /// </summary>
    /// <param name="externalSubscriptionId">ID da assinatura no provedor externo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se cancelada com sucesso; false caso contrário.</returns>
    Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma sessão do portal de faturamento para o cliente.
    /// </summary>
    /// <param name="externalCustomerId">ID do cliente no provedor externo.</param>
    /// <param name="returnUrl">URL de retorno após acesso ao portal.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>URL do portal de faturamento ou null se falhou.</returns>
    Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken = default);
}
