using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stripe;

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
                var paymentTransactionRepository = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();

                using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                var messages = await dbContext.InboxMessages
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
                    await transaction.RollbackAsync(stoppingToken);
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    try
                    {
                        var stripeEvent = EventUtility.ParseEvent(message.Content);
                        await ProcessStripeEventAsync(stripeEvent, subscriptionRepository, paymentTransactionRepository, stoppingToken);

                        message.MarkAsProcessed();
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Processing of inbox message {Id} was canceled", message.Id);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing inbox message {Id}", message.Id);
                        message.IncrementRetry();
                        message.RecordError(ex.Message);
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

    public async Task ProcessStripeEventAsync(
        Event stripeEvent, 
        ISubscriptionRepository repository, 
        IPaymentTransactionRepository paymentTransactionRepository,
        CancellationToken ct)
    {        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                var session = stripeEvent.Data.Object as dynamic;
                if (session == null)
                {
                    logger.LogWarning("Event object is not a Session for event {EventId}", stripeEvent.Id);
                    throw new InvalidOperationException("Session data is missing from checkout.session.completed event");
                }

                string? providerIdStr = null;
                Guid providerId = Guid.Empty;
                if (session.Metadata == null || 
                    !session.Metadata.TryGetValue("provider_id", out providerIdStr) || 
                    !Guid.TryParse(providerIdStr, out providerId))
                {
                    logger.LogError(
                        "Invalid or missing provider_id in checkout.session.completed event. SessionId: {SessionId}",
                        (string)session.Id);
                    throw new InvalidOperationException($"Invalid or missing provider_id in checkout.session.completed event. SessionId: {session.Id}");
                }

                var subscription = await repository.GetLatestByProviderIdAsync(providerId, ct);
                if (subscription == null)
                {
                    logger.LogWarning("Subscription not found for Provider {ProviderId}", providerId);
                    throw new InvalidOperationException($"Subscription not found for Provider {providerId}");
                }

                if (subscription.Status == ESubscriptionStatus.Active && subscription.ExternalSubscriptionId == (string)session.SubscriptionId)
                {
                    logger.LogDebug("Subscription {Id} already active with same external ID, skipping", subscription.Id);
                    break;
                }

                subscription.Activate(
                    (string)session.SubscriptionId, 
                    (string)session.CustomerId); 
                
                await repository.UpdateAsync(subscription, ct);
                logger.LogInformation("Subscription {Id} activated for Provider {ProviderId} (Customer: {CustomerId})", subscription.Id, providerId, (string)session.CustomerId);
                break;

            case "invoice.paid":
                var invoice = stripeEvent.Data.Object as dynamic;
                if (invoice == null)
                {
                    logger.LogWarning("Event object is not an Invoice for event {EventId}", stripeEvent.Id);
                    throw new InvalidOperationException("Invoice data is missing from invoice.paid event");
                }

                string? externalSubscriptionId = (string)invoice.SubscriptionId;
                if (string.IsNullOrEmpty(externalSubscriptionId))
                {
                    logger.LogInformation("Invoice {InvoiceId} has no subscription, ignoring", (string)invoice.Id);
                    break;
                }

                var subToRenew = await repository.GetByExternalIdAsync(externalSubscriptionId, ct);
                if (subToRenew == null)
                {
                    logger.LogWarning("Subscription not found for external ID: {ExternalId} (invoice: {InvoiceId}). Retrying...", externalSubscriptionId, (string)invoice.Id);
                    throw new InvalidOperationException($"Subscription with external ID {externalSubscriptionId} not found. Event will be retried.");
                }

                DateTime periodEndValue = DateTime.UtcNow.AddMonths(1);
                try
                {
                    var linesData = invoice.Lines?.Data;
                    if (linesData != null && linesData.Count > 0)
                    {
                        var firstLine = linesData[0];
                        var period = firstLine?.Period;
                        if (period != null)
                            periodEndValue = period.End;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing invoice lines or period for Invoice {InvoiceId}. Using default expiration.", (string)invoice.Id);
                }
                
                subToRenew.Renew(periodEndValue);
                await repository.UpdateAsync(subToRenew, ct);

                Money? amount = null;
                try
                {
                    if (invoice.AmountPaid > 0)
                    {
                        var currency = ((string?)invoice.Currency ?? "usd").ToUpperInvariant();
                        var decimalAmount = CurrencyUtils.ConvertFromMinorUnits((long)invoice.AmountPaid, currency);
                        amount = Money.FromDecimal(decimalAmount, currency);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error computing amount for PaymentTransaction from Invoice {InvoiceId} (Currency: {Currency}, AmountPaid: {AmountPaid})", 
                        (string)invoice.Id, (string)invoice.Currency, (long)invoice.AmountPaid);
                }
                
                if ((amount == null || amount.Amount <= 0) && subToRenew.Amount != null)
                {
                    amount = Money.FromDecimal(subToRenew.Amount.Amount, subToRenew.Amount.Currency);
                }
                
                if (amount != null && amount.Amount > 0)
                {
                    var paymentTransaction = new PaymentTransaction(subToRenew.Id, amount);
                    paymentTransaction.Settle((string)invoice.Id);
                    await paymentTransactionRepository.AddAsync(paymentTransaction, ct);

                    logger.LogInformation("Subscription {Id} renewed until {ExpiresAt}. PaymentTransaction {PaymentTransactionId} recorded for Invoice {InvoiceId}", 
                        subToRenew.Id, periodEndValue, paymentTransaction.Id, (string)invoice.Id);
                }
                else
                {
                    logger.LogWarning("Skipping PaymentTransaction creation for Invoice {InvoiceId} due to unknown or zero amount", (string)invoice.Id);
                }
                break;

            case "customer.subscription.deleted":
                var stripeSubscription = stripeEvent.Data.Object as dynamic;
                if (stripeSubscription == null)
                {
                    logger.LogWarning("Event object is not a Subscription for event {EventId}", stripeEvent.Id);
                    throw new InvalidOperationException("Subscription data is missing from customer.subscription.deleted event");
                }

                var externalId = (string)stripeSubscription.Id;
                var existingSubscription = await repository.GetByExternalIdAsync(externalId, ct);
                if (existingSubscription != null)
                {
                    existingSubscription.Cancel();
                    await repository.UpdateAsync(existingSubscription, ct);
                    logger.LogInformation("Subscription {Id} canceled (external ID: {ExternalId})", existingSubscription.Id, externalId);
                }
                else
                {
                    logger.LogWarning("Subscription not found for external ID: {ExternalId} (event: customer.subscription.deleted). Retrying...", externalId);
                    throw new InvalidOperationException($"Subscription with external ID {externalId} not found. Event will be retried.");
                }
                break;

            default:
                logger.LogDebug("Stripe event {Type} ignored", stripeEvent.Type);
                break;
        }
    }
}
