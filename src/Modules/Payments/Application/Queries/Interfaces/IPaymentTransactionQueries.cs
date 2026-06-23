using MeAjudaAi.Modules.Payments.Domain.Entities;

namespace MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;

public interface IPaymentTransactionQueries
{
    Task<PaymentTransaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
}
