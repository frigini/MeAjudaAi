using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Text.Json;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Services;

internal class PaymentCommandService(
    [FromKeyedServices(ModuleKeys.Payments)] IUnitOfWork uow,
    PaymentsDbContext dbContext,
    Microsoft.Extensions.Configuration.IConfiguration configuration,
    ILogger<PaymentCommandService> logger,
    IStringLocalizer<Strings> localizer) : IPaymentCommandService
{
    public async Task<Result> SaveInboxMessageAsync(string type, string content, string externalEventId, CancellationToken ct = default)
    {
        try
        {
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

    public async Task<Result> HandleStripeWebhookAsync(
        string payload,
        string stripeSignature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return Error.BadRequest(localizer["EmptyRequestBody"]);
        }

        var webhookSecret = configuration["Stripe:WebhookSecret"];
        var isTestingEnvironment = MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment();

        if (isTestingEnvironment && string.IsNullOrEmpty(stripeSignature))
        {
            return await ProcessMockEventAsync(payload, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            logger.LogError("Stripe:WebhookSecret not configured.");
            return Error.Internal(localizer["StripeWebhookConfigMissing"]);
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                stripeSignature,
                webhookSecret,
                throwOnApiVersionMismatch: false);

            return await SaveInboxMessageAsync(stripeEvent.Type, payload, stripeEvent.Id, cancellationToken);
        }
        catch (StripeException e)
        {
            logger.LogWarning(e, "Stripe signature validation failed.");
            return Error.BadRequest(localizer["InvalidWebhookRequest"]);
        }
        catch (JsonException e)
        {
            logger.LogWarning(e, "Stripe webhook JSON parsing failed.");
            return Error.BadRequest(localizer["InvalidWebhookRequest"]);
        }
    }

    private async Task<Result> ProcessMockEventAsync(string json, CancellationToken cancellationToken)
    {
        try
        {
            var mockEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
            if (mockEvent is null)
            {
                return Error.BadRequest(localizer["MockEventProcessingFailed"]);
            }

            return await SaveInboxMessageAsync(mockEvent.Type, json, mockEvent.Id, cancellationToken);
        }
        catch (JsonException e)
        {
            logger.LogWarning(e, "Mock event JSON parsing failed.");
            return Error.BadRequest(localizer["InvalidWebhookRequest"]);
        }
    }
}