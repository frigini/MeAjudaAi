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

        try
        {
            var result = await paymentGateway.CreateSubscriptionAsync(
                command.ProviderId,
                command.PlanId,
                amount,
                cancellationToken);

            if (!result.Success)
            {
                throw new SubscriptionCreationException($"Falha ao criar assinatura: {result.ErrorMessage}");
            }

            if (string.IsNullOrWhiteSpace(result.CheckoutUrl))
            {
                throw new SubscriptionCreationException("URL de checkout ausente no resultado do gateway.");
            }

            await subscriptionRepository.AddAsync(subscription, cancellationToken);

            return result.CheckoutUrl;
        }
        catch (Exception ex) when (ex is not SubscriptionCreationException)
        {
            throw new SubscriptionCreationException("Erro técnico ao criar assinatura.", ex);
        }
    }
}
