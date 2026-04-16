using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Repositories;

public class PaymentTransactionRepository(PaymentsDbContext context) : IPaymentTransactionRepository
{
    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Transactions.AddAsync(transaction, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // Valida se a violação é especificamente no índice de transação externa
            var databaseException = PostgreSqlExceptionProcessor.ProcessException(ex);
            if (databaseException.Message.Contains("IX_transactions_external_transaction_id", StringComparison.OrdinalIgnoreCase))
            {
                // Violação de chave única esperada para garantir idempotência.
                context.Entry(transaction).State = EntityState.Detached;
                return;
            }
            
            throw;
        }
    }

    public async Task<PaymentTransaction?> GetByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        return await context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalTransactionId == externalTransactionId, cancellationToken);
    }
}
