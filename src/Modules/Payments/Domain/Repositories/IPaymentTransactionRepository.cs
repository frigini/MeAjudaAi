using MeAjudaAi.Modules.Payments.Domain.Entities;

namespace MeAjudaAi.Modules.Payments.Domain.Repositories;

/// <summary>
/// Repositório para persistência e recuperação de transações de pagamento.
/// </summary>
public interface IPaymentTransactionRepository
{
    /// <summary>
    /// Adiciona uma nova transação ao repositório.
    /// </summary>
    /// <param name="transaction">A transação a ser persistida.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera uma transação pelo seu identificador externo (ex: ID da Invoice do Stripe).
    /// </summary>
    /// <param name="externalTransactionId">Identificador externo da transação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A transação se encontrada; caso contrário, null.</returns>
    Task<PaymentTransaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
}
