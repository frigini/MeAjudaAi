using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class GetBillingPortalCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentGateway paymentGateway) : ICommandHandler<GetBillingPortalCommand, string>
{
    public async Task<string> HandleAsync(GetBillingPortalCommand command, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionRepository.GetLatestByProviderIdAsync(command.ProviderId, cancellationToken);
        
        if (subscription == null)
            throw new ApplicationException($"Subscription not found for provider {command.ProviderId}");

        var portalUrl = await paymentGateway.CreateBillingPortalSessionAsync(
            subscription.ExternalSubscriptionId ?? subscription.Id.ToString(), 
            command.ReturnUrl, 
            cancellationToken);

        if (string.IsNullOrEmpty(portalUrl))
            throw new ApplicationException("Failed to generate Billing Portal session.");

        return portalUrl;
    }
}
