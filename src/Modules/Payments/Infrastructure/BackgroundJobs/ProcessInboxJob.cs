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
                        var stripeEvent = EventUtility.ParseEvent(message.Content, throwOnApiVersionMismatch: false);
                        var data = MapToStripeEventData(stripeEvent);
                        
                        await ProcessStripeEventAsync(data, subscriptionRepository, paymentTransactionRepository, stoppingToken);

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

    public StripeEventData MapToStripeEventData(Event stripeEvent)
    {
        if (stripeEvent.Data?.Object == null)
        {
            return new StripeEventData(stripeEvent.Type, stripeEvent.Id, null, null, null);
        }

        return stripeEvent.Type switch
        {
            "checkout.session.completed" when stripeEvent.Data.Object is Stripe.Checkout.Session session =>
                new StripeEventData(
                    stripeEvent.Type,
                    stripeEvent.Id,
                    session.SubscriptionId,
                    session.CustomerId,
                    GetProviderIdFromMetadata(session.Metadata)),

            "invoice.paid" when stripeEvent.Data.Object is Stripe.Invoice invoice =>
                new StripeEventData(
                    stripeEvent.Type,
                    stripeEvent.Id,
                    invoice.Parent?.SubscriptionDetails?.SubscriptionId ?? invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId,
                    invoice.CustomerId,
                    null,
                    invoice.Lines?.Data?.FirstOrDefault()?.Period?.End,
                    invoice.AmountPaid,
                    invoice.Currency,
                    invoice.Id),

            "customer.subscription.deleted" when stripeEvent.Data.Object is Stripe.Subscription sub =>
                new StripeEventData(
                    stripeEvent.Type,
                    stripeEvent.Id,
                    sub.Id,
                    sub.CustomerId,
                    null),

            _ => new StripeEventData(stripeEvent.Type, stripeEvent.Id, null, null, null)
        };
    }

    private Guid? GetProviderIdFromMetadata(System.Collections.Generic.Dictionary<string, string> metadata)
    {
        if (metadata != null && metadata.TryGetValue("provider_id", out var idStr) && Guid.TryParse(idStr, out var id))
            return id;
        return null;
    }

    public async Task ProcessStripeEventAsync(
        StripeEventData data, 
        ISubscriptionRepository repository, 
        IPaymentTransactionRepository paymentTransactionRepository,
        CancellationToken ct)
    {
        switch (data.Type)
        {
            case "checkout.session.completed":
                if (data.ProviderId == null || string.IsNullOrEmpty(data.SubscriptionId))
                {
                    logger.LogError("Invalid or missing data in checkout.session.completed event. EventId: {EventId}", data.ExternalEventId);
                    throw new InvalidOperationException("Essential data missing from checkout.session.completed event");
                }

                if (string.IsNullOrWhiteSpace(data.CustomerId))
                {
                    logger.LogError("CustomerId is required to activate subscription. EventId: {EventId}", data.ExternalEventId);
                    throw new InvalidOperationException("CustomerId is required to activate subscription");
                }

                var subscription = await repository.GetLatestByProviderIdAsync(data.ProviderId.Value, ct);
                if (subscription == null)
                {
                    logger.LogWarning("Subscription not found for Provider {ProviderId}", data.ProviderId);
                    throw new InvalidOperationException($"Subscription not found for Provider {data.ProviderId}");
                }

                if (subscription.Status == ESubscriptionStatus.Active && subscription.ExternalSubscriptionId == data.SubscriptionId)
                {
                    logger.LogDebug("Subscription {Id} already active with same external ID, skipping", subscription.Id);
                    break;
                }

                if (string.IsNullOrWhiteSpace(data.CustomerId))
                {
                    logger.LogError("CustomerId is required to activate subscription {SubscriptionId}", data.SubscriptionId);
                    throw new InvalidOperationException("CustomerId is required to activate subscription");
                }

                subscription.Activate(data.SubscriptionId, data.CustomerId); 
                
                await repository.UpdateAsync(subscription, ct);
                logger.LogInformation("Subscription {Id} activated for Provider {ProviderId} (Customer: {CustomerId})", subscription.Id, data.ProviderId, data.CustomerId);
                break;

            case "invoice.paid":
                if (string.IsNullOrEmpty(data.SubscriptionId))
                {
                    logger.LogInformation("Invoice {InvoiceId} has no subscription, ignoring", data.InvoiceId);
                    break;
                }

                var subToRenew = await repository.GetByExternalIdAsync(data.SubscriptionId, ct);
                if (subToRenew == null)
                {
                    logger.LogWarning("Subscription not found for external ID: {ExternalId} (invoice: {InvoiceId}). Retrying...", data.SubscriptionId, data.InvoiceId);
                    throw new InvalidOperationException($"Subscription with external ID {data.SubscriptionId} not found. Event will be retried.");
                }

                // Renovar a assinatura
                DateTime periodEndValue = data.PeriodEnd ?? DateTime.UtcNow.AddMonths(1);

                // Garantir que a renovação seja sempre para uma data futura em relação à expiração atual
                var minExpiration = (subToRenew.ExpiresAt ?? DateTime.UtcNow);
                if (periodEndValue <= minExpiration)
                {
                    periodEndValue = minExpiration.AddMonths(1);
                }
                
                subToRenew.Renew(periodEndValue);
                await repository.UpdateAsync(subToRenew, ct);

                // Create PaymentTransaction audit record
                if (string.IsNullOrWhiteSpace(data.InvoiceId))
                {
                    logger.LogWarning("Skipping PaymentTransaction creation for subscription {Id}: InvoiceId is missing", subToRenew.Id);
                    break;
                }

                Money? amount = null;
                try
                {
                    if (data.AmountPaid > 0)
                    {
                        var currency = (data.Currency ?? "usd").ToUpperInvariant();
                        var decimalAmount = CurrencyUtils.ConvertFromMinorUnits(data.AmountPaid, currency);
                        amount = Money.FromDecimal(decimalAmount, currency);

                        if (subToRenew.Amount != null && !string.Equals(currency, subToRenew.Amount.Currency, StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("Currency divergence detected for Invoice {InvoiceId}. Invoice: {InvoiceCurrency}, Subscription: {SubCurrency}", 
                                data.InvoiceId, currency, subToRenew.Amount.Currency);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error computing amount for PaymentTransaction from Invoice {InvoiceId} (Currency: {Currency}, AmountPaid: {AmountPaid})", 
                        data.InvoiceId, data.Currency ?? "null", data.AmountPaid);
                }
                
                if ((amount == null || amount.Amount <= 0) && subToRenew.Amount != null)
                {
                    var fallbackCurrency = (data.Currency ?? subToRenew.Amount.Currency ?? "usd").ToUpperInvariant();
                    amount = Money.FromDecimal(subToRenew.Amount.Amount, fallbackCurrency);
                }
                
                if (amount != null && amount.Amount > 0)
                {
                    var paymentTransaction = new PaymentTransaction(subToRenew.Id, amount);
                    paymentTransaction.Settle(data.InvoiceId);
                    await paymentTransactionRepository.AddAsync(paymentTransaction, ct);

                    logger.LogInformation("Subscription {Id} renewed until {ExpiresAt}. PaymentTransaction {PaymentTransactionId} recorded for Invoice {InvoiceId}", 
                        subToRenew.Id, periodEndValue, paymentTransaction.Id, data.InvoiceId);
                }
                else
                {
                    logger.LogWarning("Skipping PaymentTransaction creation for Invoice {InvoiceId} due to unknown or zero amount", data.InvoiceId);
                }
                break;

            case "customer.subscription.deleted":
                if (string.IsNullOrEmpty(data.SubscriptionId)) break;

                var existingSubscription = await repository.GetByExternalIdAsync(data.SubscriptionId, ct);
                if (existingSubscription != null)
                {
                    existingSubscription.Cancel();
                    await repository.UpdateAsync(existingSubscription, ct);
                    logger.LogInformation("Subscription {Id} canceled (external ID: {ExternalId})", existingSubscription.Id, data.SubscriptionId);
                }
                else
                {
                    logger.LogWarning("Subscription not found for external ID: {ExternalId} (event: customer.subscription.deleted). Retrying...", data.SubscriptionId);
                    throw new InvalidOperationException($"Subscription with external ID {data.SubscriptionId} not found. Event will be retried.");
                }
                break;

            default:
                logger.LogDebug("Stripe event {Type} ignored", data.Type);
                break;
        }
    }
}
