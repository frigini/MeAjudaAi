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
        // Obtendo detalhes do plano de uma fonte confiável (hardcoded para exemplo, deveria ser repositório)
        var (amount, currency) = GetPlanDetails(command.PlanId);
        var moneyAmount = Money.FromDecimal(amount, currency);
        var subscription = new Subscription(command.ProviderId, command.PlanId, moneyAmount);

        SubscriptionGatewayResult result;
        try
        {
            result = await paymentGateway.CreateSubscriptionAsync(
                command.ProviderId,
                command.PlanId,
                moneyAmount,
                cancellationToken);
        }
        catch (Exception ex)
        {
            throw new SubscriptionCreationException("Falha ao comunicar com o provedor de pagamento.", ex);
        }

        if (!result.Success)
        {
            throw new SubscriptionCreationException($"Falha ao criar assinatura no gateway: {result.ErrorMessage}");
        }

        if (string.IsNullOrWhiteSpace(result.CheckoutUrl))
        {
            throw new SubscriptionCreationException("URL de checkout ausente no resultado do gateway.");
        }

        try
        {
            await subscriptionRepository.AddAsync(subscription, cancellationToken);
        }
        catch (Exception ex)
        {
            // Compensação: Cancelar no gateway se falhar a persistência local
            if (!string.IsNullOrEmpty(result.ExternalSubscriptionId))
            {
                await paymentGateway.CancelSubscriptionAsync(result.ExternalSubscriptionId, cancellationToken);
            }
            
            throw new SubscriptionCreationException("Falha ao persistir assinatura localmente. Operação revertida no gateway.", ex);
        }

        return result.CheckoutUrl;
    }

    private static (decimal Amount, string Currency) GetPlanDetails(string planId)
    {
        // Mapeamento confiável de planos (Id do Preço no Stripe -> Valor/Moeda)
        return planId switch
        {
            "price_premium_monthly" => (99.90m, "BRL"),
            "price_gold_monthly" => (199.90m, "BRL"),
            _ => throw new SubscriptionCreationException($"Plano inválido ou não suportado: {planId}")
        };
    }
}
