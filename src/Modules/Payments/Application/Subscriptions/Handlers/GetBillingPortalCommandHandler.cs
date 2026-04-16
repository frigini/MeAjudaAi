using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class GetBillingPortalCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentGateway paymentGateway,
    ILogger<GetBillingPortalCommandHandler> logger) : ICommandHandler<GetBillingPortalCommand, string>
{
    public async Task<string> HandleAsync(GetBillingPortalCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating billing portal for Provider {ProviderId}", command.ProviderId);
        
        var subscription = await subscriptionRepository.GetActiveByProviderIdAsync(command.ProviderId, cancellationToken);
        
        if (subscription == null)
        {
            logger.LogWarning("Active subscription not found for Provider {ProviderId}", command.ProviderId);
            throw new ApplicationException($"Subscription not found or not active for provider {command.ProviderId}");
        }

        if (string.IsNullOrEmpty(subscription.ExternalCustomerId))
        {
            logger.LogWarning("Subscription {SubscriptionId} has no ExternalCustomerId", subscription.Id);
            throw new ApplicationException($"Subscription exists but has no ExternalCustomerId for provider {command.ProviderId}");
        }

        logger.LogInformation("Found subscription {SubscriptionId} with Customer {CustomerId}. Creating session...", 
            subscription.Id, subscription.ExternalCustomerId);

        var portalUrl = await paymentGateway.CreateBillingPortalSessionAsync(
            subscription.ExternalCustomerId, 
            command.ReturnUrl, 
            cancellationToken);

        if (string.IsNullOrEmpty(portalUrl))
        {
            logger.LogError("Gateway failed to generate portal URL for Customer {CustomerId}", subscription.ExternalCustomerId);
            throw new ApplicationException("Failed to generate Billing Portal session.");
        }

        return portalUrl;
    }
}
