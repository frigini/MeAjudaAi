using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;

public class ProcessInboxJob(
    IServiceProvider serviceProvider,
    ILogger<ProcessInboxJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inbox Processor for Payments starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();

                using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                var messages = await dbContext.InboxMessages
                    // NOTE: This SQL query must mirror InboxMessage.ShouldRetry logic
                    // See: src/Modules/Payments/Domain/Entities/InboxMessage.cs - ShouldRetry property
                    .FromSqlRaw(@"
                        SELECT * FROM payments.inbox_messages
                        WHERE processed_at IS NULL 
                        AND retry_count < max_retries 
                        AND (next_attempt_at IS NULL OR next_attempt_at <= NOW())
                        ORDER BY created_at
                        LIMIT 20
                        FOR UPDATE SKIP LOCKED")
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    try
                    {
                        var stripeEvent = EventUtility.ParseEvent(message.Content);
                        await ProcessStripeEventAsync(stripeEvent, subscriptionRepository, stoppingToken);

                        message.ProcessedAt = DateTime.UtcNow;
                        message.Error = null;
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Processing of inbox message {Id} was canceled", message.Id);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing inbox message {Id}", message.Id);
                        message.Error = ex.Message;
                        message.RetryCount++;
                        message.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, message.RetryCount)); // Backoff exponencial
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Inbox Processor for Payments stopping gracefully.");
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Inbox Processor loop");
                await Task.Delay(10000, stoppingToken);
            }
        }
    }

    private async Task ProcessStripeEventAsync(
        Event stripeEvent, 
        ISubscriptionRepository repository, 
        CancellationToken ct)
    {
        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                var session = stripeEvent.Data.Object as Session;
                if (session == null)
                {
                    logger.LogWarning("Session data is null for event {EventId}", stripeEvent.Id);
                    throw new InvalidOperationException("Session data is missing from checkout.session.completed event");
                }

                if (!session.Metadata.TryGetValue("provider_id", out var providerIdStr) || 
                    !Guid.TryParse(providerIdStr, out var providerId))
                {
                    if (session.Metadata == null)
                    {
                        logger.LogError(
                            "Metadata is null in checkout.session.completed event. SessionId: {SessionId}",
                            session.Id);
                        throw new InvalidOperationException($"Metadata is null in checkout.session.completed event. SessionId: {session.Id}");
                    }

                    logger.LogError(
                        "Invalid or missing provider_id in checkout.session.completed event. SessionId: {SessionId}, Metadata: {Metadata}",
                        session.Id,
                        session.Metadata);
                    throw new InvalidOperationException($"Invalid or missing provider_id in checkout.session.completed event. SessionId: {session.Id}");
                }

                var subscription = await repository.GetLatestByProviderIdAsync(providerId, ct);
                if (subscription == null)
                {
                    logger.LogWarning("Subscription not found for Provider {ProviderId}", providerId);
                    throw new InvalidOperationException($"Subscription not found for Provider {providerId}");
                }

                if (subscription.Status == ESubscriptionStatus.Active && subscription.ExternalSubscriptionId == session.SubscriptionId)
                {
                    logger.LogDebug("Subscription {Id} already active with same external ID, skipping", subscription.Id);
                    break;
                }

                subscription.Activate(
                    session.SubscriptionId, 
                    DateTime.UtcNow.AddMonths(1)); // Default for initial checkout if not specified
                
                await repository.UpdateAsync(subscription, ct);
                logger.LogInformation("Subscription {Id} activated for Provider {ProviderId} (Customer: {CustomerId})", subscription.Id, providerId, session.CustomerId);
                break;

            case "invoice.paid":
                var invoice = stripeEvent.Data.Object as dynamic;
                if (invoice == null)
                {
                    logger.LogWarning("Invoice data is null for event {EventId}", stripeEvent.Id);
                    throw new InvalidOperationException("Invoice data is missing from invoice.paid event");
                }

                string? externalSubscriptionId = invoice.SubscriptionId;
                if (string.IsNullOrEmpty(externalSubscriptionId))
                {
                    logger.LogInformation("Invoice {InvoiceId} has no subscription, ignoring", (string)invoice.Id);
                    break;
                }

                var subToRenew = await repository.GetByExternalIdAsync(externalSubscriptionId, ct);
                if (subToRenew == null)
                {
                    logger.LogWarning("Subscription not found for external ID: {ExternalId} (invoice: {InvoiceId})", externalSubscriptionId, (string)invoice.Id);
                    break;
                }

                // Renew the subscription
                var nextPeriodEnd = (DateTime?)(invoice.Lines.Data[0].Period.End) ?? DateTime.UtcNow.AddMonths(1);
                subToRenew.Renew(nextPeriodEnd);
                await repository.UpdateAsync(subToRenew, ct);

                logger.LogInformation("Subscription {Id} renewed until {ExpiresAt} due to Invoice {InvoiceId}", subToRenew.Id, nextPeriodEnd, (string)invoice.Id);
                break;

            case "customer.subscription.deleted":
                var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
                if (stripeSubscription == null)
                {
                    logger.LogWarning("Subscription data is null for event {EventId}", stripeEvent.Id);
                    throw new InvalidOperationException("Subscription data is missing from customer.subscription.deleted event");
                }

                var existingSubscription = await repository.GetByExternalIdAsync(stripeSubscription.Id, ct);
                if (existingSubscription != null)
                {
                    existingSubscription.Cancel();
                    await repository.UpdateAsync(existingSubscription, ct);
                    logger.LogInformation("Subscription {Id} canceled (external ID: {ExternalId})", existingSubscription.Id, stripeSubscription.Id);
                }
                else
                {
                    logger.LogWarning("Subscription not found for external ID: {ExternalId} (event: customer.subscription.deleted)", stripeSubscription.Id);
                }
                break;

            default:
                logger.LogDebug("Stripe event {Type} ignored", stripeEvent.Type);
                break;
        }
    }
}
