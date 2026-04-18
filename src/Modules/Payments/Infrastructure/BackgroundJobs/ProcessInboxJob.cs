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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;

public class ProcessInboxJob(
    IServiceProvider sp,
    ILogger<ProcessInboxJob> logger) : BackgroundService
{
    protected readonly IServiceProvider _sp = sp;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inbox Processor for Payments starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
                var paymentTransactionRepository = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();

                using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                // Processar em lote para eficiência
                var messages = await dbContext.InboxMessages
                    .Where(m => m.ProcessedAt == null && m.RetryCount < m.MaxRetries && (m.NextAttemptAt == null || m.NextAttemptAt <= DateTime.UtcNow))
                    .OrderBy(m => m.CreatedAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await transaction.CommitAsync(stoppingToken);
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                await ProcessMessagesBatchAsync(messages, dbContext, subscriptionRepository, paymentTransactionRepository, stoppingToken);

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

    public virtual async Task ProcessMessagesBatchAsync(
        List<InboxMessage> messages, 
        PaymentsDbContext dbContext, 
        ISubscriptionRepository subscriptionRepository, 
        IPaymentTransactionRepository paymentTransactionRepository, 
        CancellationToken ct)
    {
        foreach (var message in messages)
        {
            try
            {
                var stripeEvent = EventUtility.ParseEvent(message.Content, throwOnApiVersionMismatch: false);
                var data = MapToStripeEventData(stripeEvent);
                
                await ProcessStripeEventAsync(data, subscriptionRepository, paymentTransactionRepository, ct);

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
                    invoice.Parent?.SubscriptionDetails?.Subscription?.Id ??
                    invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId,
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

    private Guid? GetProviderIdFromMetadata(Dictionary<string, string> metadata)
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
                    throw new InvalidOperationException("Essential data missing in checkout.session.completed event");
                }

                // Tenta carregar a assinatura mais recente pendente
                var subscription = await repository.GetLatestByProviderIdAsync(data.ProviderId.Value, ct);

                if (subscription == null)
                {
                    logger.LogError("Subscription for Provider {ProviderId} not found.", data.ProviderId);
                    throw new InvalidOperationException("Subscription not found");
                }

                if (subscription.Status == ESubscriptionStatus.Active && subscription.ExternalSubscriptionId == data.SubscriptionId)
                {
                    logger.LogDebug("Subscription {Id} already active with same external ID, skipping", subscription.Id);
                    break;
                }

                subscription.Activate(data.SubscriptionId, data.CustomerId ?? string.Empty); 
                
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
                if (subToRenew != null)
                {
                    if (data.PeriodEnd.HasValue)
                    {
                        subToRenew.Renew(data.PeriodEnd.Value);
                        await repository.UpdateAsync(subToRenew, ct);
                        logger.LogInformation("Subscription {Id} renewed until {ExpiresAt}", subToRenew.Id, subToRenew.ExpiresAt);
                    }

                    // Registrar a transação de pagamento
                    Money? amount = null;
                    try
                    {
                        if (data.AmountPaid > 0 && !string.IsNullOrEmpty(data.Currency))
                        {
                            amount = Money.FromDecimal(
                                CurrencyUtils.ConvertFromMinorUnits(data.AmountPaid, data.Currency), 
                                data.Currency);
                            
                            // Log warning if currency differs from subscription
                            if (!string.Equals(amount.Currency, subToRenew.Amount.Currency, StringComparison.OrdinalIgnoreCase))
                            {
                                logger.LogWarning("Currency divergence detected for Subscription {Id}. Subscription: {SubCurrency}, Invoice: {InvCurrency}", 
                                    subToRenew.Id, subToRenew.Amount.Currency, amount.Currency);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error computing amount for PaymentTransaction from Invoice {InvoiceId} (Currency: {Currency}, AmountPaid: {AmountPaid})", 
                            data.InvoiceId, data.Currency ?? "null", data.AmountPaid);
                    }
                    
                    if (amount != null && amount.Amount > 0)
                    {
                        var paymentTransaction = new PaymentTransaction(subToRenew.Id, amount);
                        paymentTransaction.Settle(data.InvoiceId!);
                        await paymentTransactionRepository.AddAsync(paymentTransaction, ct);
                    }
                    else
                    {
                        logger.LogWarning("Skipping PaymentTransaction creation for Invoice {InvoiceId} due to unknown or zero amount", data.InvoiceId);
                    }
                }
                else
                {
                    logger.LogWarning("Subscription {SubscriptionId} not found. Event will be retried.", data.SubscriptionId);
                    throw new InvalidOperationException($"Subscription {data.SubscriptionId} not found. Event will be retried.");
                }
                break;

            case "customer.subscription.deleted":
                if (string.IsNullOrEmpty(data.SubscriptionId)) break;

                var subToCancel = await repository.GetByExternalIdAsync(data.SubscriptionId, ct);
                if (subToCancel != null)
                {
                    subToCancel.Cancel();
                    await repository.UpdateAsync(subToCancel, ct);
                    logger.LogInformation("Subscription {Id} canceled due to deletion in Stripe", subToCancel.Id);
                }
                else
                {
                    logger.LogWarning("Subscription {SubscriptionId} to cancel not found.", data.SubscriptionId);
                    throw new InvalidOperationException($"Subscription {data.SubscriptionId} not found.");
                }
                break;

            default:
                logger.LogDebug("Stripe event {Type} ignored", data.Type);
                break;
        }
    }
}
