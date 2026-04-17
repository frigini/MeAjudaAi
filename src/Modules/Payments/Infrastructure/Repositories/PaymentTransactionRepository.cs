using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Repositories;

public class PaymentTransactionRepository(
    PaymentsDbContext context,
    ILogger<PaymentTransactionRepository> logger) : IPaymentTransactionRepository
{
    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.PaymentTransactions.AddAsync(transaction, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            var databaseException = PostgreSqlExceptionProcessor.ProcessException(ex);
            
            // Note: The constraint name might vary depending on EF Core naming convention.
            // Using a case-insensitive check for common patterns.
            if (databaseException is UniqueConstraintException uniqueEx && 
                uniqueEx.ConstraintName.Contains("external_transaction_id", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Duplicate external transaction id {ExternalTransactionId} detected; treating as idempotent replay.", transaction.ExternalTransactionId);
                // Violação de chave única esperada para garantir idempotência.
                context.Entry(transaction).State = EntityState.Detached;
                return;
            }
            
            throw;
        }
    }

    public async Task<PaymentTransaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        return await context.PaymentTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalTransactionId == externalTransactionId, cancellationToken);
    }
}
