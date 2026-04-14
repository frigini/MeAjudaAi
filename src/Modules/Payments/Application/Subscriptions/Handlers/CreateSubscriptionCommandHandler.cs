using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class CreateSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentGateway paymentGateway) : ICommandHandler<CreateSubscriptionCommand, string>
{
    public async Task<string> HandleAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        var amount = Money.FromDecimal(command.Amount, command.Currency);
        var subscription = new Subscription(command.ProviderId, command.PlanId, amount);

        var result = await paymentGateway.CreateSubscriptionAsync(
            command.ProviderId,
            command.PlanId,
            amount,
            cancellationToken);

        if (!result.Success)
        {
            throw new Exception($"Failed to create subscription: {result.ErrorMessage}");
        }

        // The checkout session doesn't create the subscription in Stripe yet,
        // so we don't have an ExternalSubscriptionId here usually.
        // It'll be set via Webhook once the payment is completed.
        
        await subscriptionRepository.AddAsync(subscription, cancellationToken);

        return result.CheckoutUrl ?? throw new Exception("Checkout URL is missing.");
    }
}
