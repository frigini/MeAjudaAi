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

                var messages = await dbContext.InboxMessages
                    .Where(m => m.ProcessedAt == null && m.RetryCount < m.MaxRetries && (m.NextAttemptAt == null || m.NextAttemptAt <= DateTime.UtcNow))
                    .OrderBy(m => m.CreatedAt)
                    .Take(20)
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Inbox Processor loop");
                await Task.Delay(10000, stoppingToken);
            }
        }
    }

    private async Task ProcessStripeEventAsync(Event stripeEvent, ISubscriptionRepository repository, CancellationToken ct)
    {
        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                var session = stripeEvent.Data.Object as Session;
                if (session?.Metadata.TryGetValue("provider_id", out var providerIdStr) == true &&
                    Guid.TryParse(providerIdStr, out var providerId))
                {
                    var subscription = await repository.GetLatestByProviderIdAsync(providerId, ct);
                    if (subscription == null)
                    {
                        logger.LogWarning("Subscription not found for Provider {ProviderId}", providerId);
                        break;
                    }

                    if (subscription.Status == ESubscriptionStatus.Active && subscription.ExternalSubscriptionId == session.SubscriptionId)
                    {
                        // Já processado
                        break;
                    }

                    subscription.Activate(
                        session.SubscriptionId, 
                        DateTime.UtcNow, 
                        null); 
                    
                    await repository.UpdateAsync(subscription, ct);
                    logger.LogInformation("Subscription {Id} activated for Provider {ProviderId}", subscription.Id, providerId);
                }
                break;

            case "customer.subscription.deleted":
                var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
                if (stripeSubscription != null)
                {
                    var subscription = await repository.GetByExternalIdAsync(stripeSubscription.Id, ct);
                    if (subscription != null)
                    {
                        subscription.Cancel();
                        await repository.UpdateAsync(subscription, ct);
                        logger.LogInformation("Subscription {Id} canceled (external ID: {ExternalId})", subscription.Id, stripeSubscription.Id);
                    }
                    else
                    {
                        logger.LogWarning("Subscription not found for external ID: {ExternalId} (event: customer.subscription.deleted)", stripeSubscription.Id);
                    }
                }
                break;

            default:
                logger.LogDebug("Stripe event {Type} ignored", stripeEvent.Type);
                break;
        }
    }
}
