using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs.Models;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stripe;

namespace MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job que processa mensagens da inbox de eventos Stripe.
/// </summary>
/// <remarks>
/// Implementa o padrão Inbox para garantia de processamento de eventos recebidos do Stripe.
/// Processa mensagens em lotes de 20, com suporte a retry com backoff exponencial.
/// Eventos suportados: CheckoutSessionCompleted, InvoicePaid, CustomerSubscriptionDeleted.
/// </remarks>
public class ProcessInboxJob(
    IServiceProvider sp,
    ILogger<ProcessInboxJob> logger) : BackgroundService
{
    protected readonly IServiceProvider _sp = sp;

    /// <summary>
    /// Loop principal que busca e processa mensagens pendentes da inbox.
    /// </summary>
    /// <param name="stoppingToken">Token de cancelamento do serviço.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inbox Processor for Payments starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(MeAjudaAi.Shared.Database.Constants.ModuleKeys.Payments);
                var subscriptionQueries = scope.ServiceProvider.GetRequiredService<ISubscriptionQueries>();

                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

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

                await ProcessMessagesBatchAsync(messages, dbContext, subscriptionQueries, uow, stoppingToken);

                await uow.SaveChangesAsync(stoppingToken);
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

    /// <summary>
    /// Processa um lote de mensagens da inbox, extraindo e aplicando eventos Stripe.
    /// </summary>
    /// <param name="messages">Lista de mensagens pendentes a serem processadas.</param>
    /// <param name="dbContext">Contexto do banco de dados.</param>
    /// <param name="subscriptionQueries">Queries de acesso a dados de assinaturas.</param>
    /// <param name="uow">Unit of Work para persistência.</param>
    /// <param name="ct">Token de cancelamento.</param>
    public virtual async Task ProcessMessagesBatchAsync(
        List<InboxMessage> messages, 
        PaymentsDbContext dbContext, 
        ISubscriptionQueries subscriptionQueries, 
        IUnitOfWork uow,
        CancellationToken ct)
    {
        var transactionRepo = uow.GetRepository<PaymentTransaction, Guid>();

        foreach (var message in messages)
        {
            try
            {
                var stripeEvent = EventUtility.ParseEvent(message.Content, throwOnApiVersionMismatch: false);
                var data = MapToStripeEventData(stripeEvent);
                
                await ProcessStripeEventAsync(data, transactionRepo, subscriptionQueries, dbContext, uow, ct);


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

    /// <summary>
    /// Converte um evento Stripe bruto em um DTO normalizado para processamento.
    /// </summary>
    /// <param name="stripeEvent">Evento Stripe extraído do payload.</param>
    /// <returns>Dados normalizados do evento para processamento pelo switch de eventos.</returns>
    public StripeEventData MapToStripeEventData(Event stripeEvent)
    {
        if (stripeEvent.Data?.Object == null)
        {
            return new StripeEventData(stripeEvent.Type, stripeEvent.Id, null, null, null);
        }

        return stripeEvent.Type switch
        {
            StripeConstants.CheckoutSessionCompleted when stripeEvent.Data.Object is Stripe.Checkout.Session session =>
                new StripeEventData(
                    stripeEvent.Type,
                    stripeEvent.Id,
                    session.SubscriptionId,
                    session.CustomerId,
                    GetProviderIdFromMetadata(session.Metadata)),

            StripeConstants.InvoicePaid when stripeEvent.Data.Object is Stripe.Invoice invoice =>
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
            StripeConstants.CustomerSubscriptionDeleted when stripeEvent.Data.Object is Stripe.Subscription sub =>
                new StripeEventData(
                    stripeEvent.Type,
                    stripeEvent.Id,
                    sub.Id,
                    sub.CustomerId,
                    null),

            _ => new StripeEventData(stripeEvent.Type, stripeEvent.Id, null, null, null)
        };
    }

    private static Guid? GetProviderIdFromMetadata(Dictionary<string, string> metadata)
    {
        if (metadata != null && metadata.TryGetValue(StripeConstants.ProviderIdMetadataKey, out var idStr) && Guid.TryParse(idStr, out var id))
            return id;
        return null;
    }

    /// <summary>
    /// Processa um evento Stripe normalizado, aplicando ações de domínio conforme o tipo do evento.
    /// </summary>
    /// <param name="data">Dados normalizados do evento Stripe.</param>
    /// <param name="transactionRepo">Repositório de transações de pagamento.</param>
    /// <param name="subscriptionQueries">Queries de assinaturas.</param>
    /// <param name="dbContext">Contexto do banco de dados.</param>
    /// <param name="uow">Unit of Work para persistência.</param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ProcessStripeEventAsync(
        StripeEventData data, 
        IRepository<PaymentTransaction, Guid> transactionRepo,
        ISubscriptionQueries subscriptionQueries,
        PaymentsDbContext dbContext,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        switch (data.Type)
        {
            case StripeConstants.CheckoutSessionCompleted:
                if (data.ProviderId == null || string.IsNullOrEmpty(data.SubscriptionId))
                {
                    logger.LogError("Invalid or missing data in checkout.session.completed event. EventId: {EventId}", data.ExternalEventId);
                    throw new InvalidOperationException("Essential data missing in checkout.session.completed event");
                }

                var subscription = await subscriptionQueries.GetByExternalIdAsync(data.SubscriptionId, ct)
                                   ?? await subscriptionQueries.GetLatestByProviderIdAsync(data.ProviderId.Value, ct);

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
                
                await uow.SaveChangesAsync(ct);
                break;


            case StripeConstants.InvoicePaid:
                if (string.IsNullOrEmpty(data.SubscriptionId))
                {
                    logger.LogInformation("Invoice {InvoiceId} has no subscription, ignoring", data.InvoiceId);
                    break;
                }

                var subToRenew = await subscriptionQueries.GetByExternalIdAsync(data.SubscriptionId, ct);
                if (subToRenew != null)
                {
                    if (data.PeriodEnd.HasValue)
                    {
                        subToRenew.Renew(data.PeriodEnd.Value);
                        logger.LogInformation("Subscription {Id} renewed until {ExpiresAt}", subToRenew.Id, subToRenew.ExpiresAt);
                    }

                    Money? amount = null;
                    try
                    {
                        if (data.AmountPaid > 0 && !string.IsNullOrEmpty(data.Currency))
                        {
                            amount = Money.FromDecimal(
                                CurrencyUtils.ConvertFromMinorUnits(data.AmountPaid, data.Currency), 
                                data.Currency);
                            
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
                    
                    if (amount != null && amount.Amount > 0 && !string.IsNullOrEmpty(data.InvoiceId))
                    {
                        var paymentTransaction = new PaymentTransaction(subToRenew.Id, amount);
                        paymentTransaction.Settle(data.InvoiceId);
                        transactionRepo.Add(paymentTransaction);
                    }
                    else
                    {
                        logger.LogWarning("Skipping PaymentTransaction creation for Invoice {InvoiceId} due to unknown or zero amount", data.InvoiceId);
                    }
                    await uow.SaveChangesAsync(ct);
                }
                else
                {
                    logger.LogWarning("Subscription {SubscriptionId} not found. Event will be retried.", data.SubscriptionId);
                    throw new InvalidOperationException($"Subscription {data.SubscriptionId} not found. Event will be retried.");
                }
                break;

            case StripeConstants.CustomerSubscriptionDeleted:
                if (string.IsNullOrEmpty(data.SubscriptionId)) break;

                var subToCancel = await subscriptionQueries.GetByExternalIdAsync(data.SubscriptionId, ct);
                if (subToCancel != null)
                {
                    subToCancel.Cancel();
                    logger.LogInformation("Subscription {Id} canceled due to deletion in Stripe", subToCancel.Id);
                    await uow.SaveChangesAsync(ct);
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