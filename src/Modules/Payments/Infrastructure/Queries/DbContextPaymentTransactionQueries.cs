using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Queries;

public class DbContextPaymentTransactionQueries(PaymentsDbContext dbContext) : IPaymentTransactionQueries
{
    private readonly PaymentsDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<PaymentTransaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("ExternalTransactionId cannot be empty.", nameof(externalTransactionId));
        return await _dbContext.PaymentTransactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalTransactionId == externalTransactionId, cancellationToken);
    }
}
