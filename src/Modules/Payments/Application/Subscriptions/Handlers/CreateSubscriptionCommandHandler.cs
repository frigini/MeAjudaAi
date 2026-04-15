using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

using MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class CreateSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentGateway paymentGateway) : ICommandHandler<CreateSubscriptionCommand, string>
{
    public async Task<string> HandleAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        var amount = Money.FromDecimal(command.Amount, command.Currency);
        var subscription = new Subscription(command.ProviderId, command.PlanId, amount);

        // Persistir primeiro em estado Pending para garantir consistência.
        // Se a chamada ao gateway falhar depois, o registro local permanece como Pending
        // e pode ser limpo ou reprocessado posteriormente.
        await subscriptionRepository.AddAsync(subscription, cancellationToken);

        var result = await paymentGateway.CreateSubscriptionAsync(
            command.ProviderId,
            command.PlanId,
            amount,
            cancellationToken);

        if (!result.Success)
        {
            throw new SubscriptionCreationException($"Failed to create subscription: {result.ErrorMessage}");
        }

        if (string.IsNullOrWhiteSpace(result.CheckoutUrl))
        {
            throw new SubscriptionCreationException("Checkout URL is missing from gateway result.");
        }

        // A sessão de checkout não cria a assinatura no Stripe ainda,
        // portanto normalmente não há ExternalSubscriptionId aqui.
        // Ele será definido via Webhook quando o pagamento for concluído.

        return result.CheckoutUrl;
    }
}
