using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Queries;

public class DbContextPaymentTransactionQueries(PaymentsDbContext dbContext) : IPaymentTransactionQueries
{
    public async Task<PaymentTransaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("ExternalTransactionId cannot be empty.", nameof(externalTransactionId));
        return await dbContext.PaymentTransactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalTransactionId == externalTransactionId, cancellationToken);
    }
}
