using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Services;

public class DbContextPaymentCommandService(
    [FromKeyedServices(ModuleKeys.Payments)] IUnitOfWork uow,
    PaymentsDbContext dbContext,
    ILogger<DbContextPaymentCommandService> logger) : IPaymentCommandService
{
    public async Task<Result> SaveInboxMessageAsync(string type, string content, string externalEventId, CancellationToken ct = default)
    {
        try
        {
            // Idempotência: verifica se o evento já existe
            var exists = await dbContext.InboxMessages.AnyAsync(m => m.ExternalEventId == externalEventId, ct);
            if (exists)
            {
                logger.LogInformation("Stripe event {ExternalEventId} already exists in inbox, skipping.", externalEventId);
                return Result.Success();
            }

            var inboxMessage = new InboxMessage(type, content, externalEventId);
            uow.GetRepository<InboxMessage, Guid>().Add(inboxMessage);
            
            await uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(ex);
            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation("Stripe event {ExternalEventId} unique constraint violation (concurrency), skipping.", externalEventId);
                return Result.Success();
            }

            logger.LogError(ex, "Error saving Stripe event {ExternalEventId} to inbox.", externalEventId);
            throw;
        }
    }
}
