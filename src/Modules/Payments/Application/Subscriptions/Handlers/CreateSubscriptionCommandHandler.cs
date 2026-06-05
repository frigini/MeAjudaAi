using MeAjudaAi.Shared.Database.Abstractions;
using System.Globalization;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public sealed class CreateSubscriptionCommandHandler(
    [FromKeyedServices(ModuleKeys.Payments)] IUnitOfWork uow,
    IPaymentGateway paymentGateway,
    IConfiguration configuration,
    IMessageBus messageBus,
    ILogger<CreateSubscriptionCommandHandler> logger) : ICommandHandler<CreateSubscriptionCommand, string>
{
    public async Task<string> HandleAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        if (command.IdempotencyKey?.Length > 255)
        {
            throw new SubscriptionCreationException("Chave de idempotência inválida ou muito longa (máximo 255 caracteres).");
        }

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
                command.IdempotencyKey,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Payment provider communication failure for Provider {ProviderId}", command.ProviderId);
            throw new SubscriptionCreationException("Falha ao comunicar com o provedor de pagamento.", ex);
        }

        if (!result.Success)
        {
            logger.LogWarning("Gateway failed to create subscription for Provider {ProviderId}: {ErrorMessage}", 
                command.ProviderId, result.ErrorMessage);
            
            if (!string.IsNullOrWhiteSpace(result.ExternalSubscriptionId))
            {
                await TryCompensateAsync(result);
            }
            
            throw new SubscriptionCreationException($"Falha ao criar assinatura no gateway: {result.ErrorMessage}");
        }

        if (string.IsNullOrWhiteSpace(result.CheckoutUrl))
        {
            logger.LogError("Missing checkout URL for Provider {ProviderId}", command.ProviderId);
            await TryCompensateAsync(result);
            throw new SubscriptionCreationException("URL de checkout ausente no resultado do gateway.");
        }

        try
        {
            uow.GetRepository<Subscription, Guid>().Add(subscription);
            await uow.SaveChangesAsync(cancellationToken);

            // Publicar evento de integração
            await messageBus.PublishAsync(new SubscriptionActivatedIntegrationEvent(
                ModuleNames.Payments,
                subscription.Id,
                command.ProviderId), cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await TryCompensateAsync(result);
            throw new SubscriptionCreationException("Falha ao persistir assinatura localmente. Operação revertida no gateway.", ex);
        }

        return result.CheckoutUrl;
    }

    private async Task TryCompensateAsync(SubscriptionGatewayResult result)
    {
        if (!string.IsNullOrEmpty(result.ExternalSubscriptionId))
        {
            logger.LogInformation("Compensating: cancelling external subscription {ExternalSubscriptionId} due to failure", 
                result.ExternalSubscriptionId);
            
            try
            {
                var cancelled = await paymentGateway.CancelSubscriptionAsync(result.ExternalSubscriptionId, CancellationToken.None);
                
                if (!cancelled)
                {
                    logger.LogError("Failed to cancel external subscription {ExternalSubscriptionId} during rollback. Manual intervention may be required.", 
                        result.ExternalSubscriptionId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during compensation rollback for subscription {ExternalSubscriptionId}. Keeping original failure.", 
                    result.ExternalSubscriptionId);
            }
        }
    }

    private (decimal Amount, string Currency) GetPlanDetails(string planId)
    {
        var planSection = configuration.GetSection($"Payments:Plans:{planId}");
        if (string.IsNullOrEmpty(planSection.Value) && !planSection.GetChildren().Any())
        {
            logger.LogWarning("Invalid plan attempted: {PlanId}", planId);
            throw new SubscriptionCreationException($"Plano inválido ou não suportado: {planId}");
        }

        var amountStr = configuration[$"Payments:Plans:{planId}:Amount"];
        var currency = configuration[$"Payments:Plans:{planId}:Currency"];

        if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || string.IsNullOrEmpty(currency))
        {
            logger.LogError("Incomplete configuration for plan: {PlanId}", planId);
            throw new SubscriptionCreationException($"Configuração incompleta para o plano: {planId}");
        }

        return (amount, currency);
    }
}



