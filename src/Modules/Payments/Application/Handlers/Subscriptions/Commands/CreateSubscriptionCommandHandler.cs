using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Exceptions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Commands;

public sealed class CreateSubscriptionCommandHandler(
    [FromKeyedServices(ModuleKeys.Payments)] IUnitOfWork uow,
    IPaymentGateway paymentGateway,
    PaymentsOptions paymentsOptions,
    ILogger<CreateSubscriptionCommandHandler> logger) : ICommandHandler<CreateSubscriptionCommand, string>
{
    private readonly PaymentsOptions _options = paymentsOptions;
    public async Task<string> HandleAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        if (command.IdempotencyKey?.Length > 255)
        {
            throw new SubscriptionCreationException("Chave de idempotência inválida ou muito longa (máximo 255 caracteres).", StatusCodes.Status400BadRequest);
        }

        var (amount, currency) = GetPlanDetails(command.PlanId);
        var moneyAmount = Money.FromDecimal(amount, currency);
        var subscription = new Subscription(command.ProviderId, command.PlanId, moneyAmount);

        SubscriptionGatewayResponse result;
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
            throw new SubscriptionCreationException("Falha ao comunicar com o provedor de pagamento.", StatusCodes.Status502BadGateway);
        }

        if (!result.Success)
        {
            logger.LogWarning("Gateway failed to create subscription for Provider {ProviderId}: {ErrorMessage}", 
                command.ProviderId, result.ErrorMessage);
            
            if (!string.IsNullOrWhiteSpace(result.ExternalSubscriptionId))
            {
                await TryCompensateAsync(result);
            }
            
            throw new SubscriptionCreationException($"Falha ao criar assinatura no gateway: {result.ErrorMessage}", StatusCodes.Status502BadGateway);
        }

        if (string.IsNullOrWhiteSpace(result.CheckoutUrl))
        {
            logger.LogError("Missing checkout URL for Provider {ProviderId}", command.ProviderId);
            await TryCompensateAsync(result);
            throw new SubscriptionCreationException("URL de checkout ausente no resultado do gateway.", StatusCodes.Status502BadGateway);
        }

        try
        {
            uow.GetRepository<Subscription, Guid>().Add(subscription);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await TryCompensateAsync(result);
            throw new SubscriptionCreationException("Falha ao persistir assinatura localmente. Operação revertida no gateway.", ex, StatusCodes.Status500InternalServerError);
        }

        return result.CheckoutUrl;
    }

    private async Task TryCompensateAsync(SubscriptionGatewayResponse result)
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
        if (!_options.Plans.TryGetValue(planId, out var planOptions) || planOptions == null)
        {
            logger.LogWarning("Invalid plan attempted: {PlanId}", planId);
            throw new SubscriptionCreationException($"Plano inválido ou não suportado: {planId}", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrEmpty(planOptions.Currency))
        {
            logger.LogError("Incomplete configuration for plan: {PlanId}", planId);
            throw new SubscriptionCreationException($"Configuração incompleta para o plano: {planId}", StatusCodes.Status500InternalServerError);
        }

        return (planOptions.Amount, planOptions.Currency);
    }
}