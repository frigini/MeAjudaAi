using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class GetBillingPortalCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentGateway paymentGateway,
    ILogger<GetBillingPortalCommandHandler> logger) : ICommandHandler<GetBillingPortalCommand, string>
{
    public async Task<string> HandleAsync(GetBillingPortalCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Gerando portal de faturamento para o Provider {ProviderId}", command.ProviderId);
        
        var subscription = await subscriptionRepository.GetActiveByProviderIdAsync(command.ProviderId, cancellationToken);
        
        if (subscription == null)
        {
            logger.LogWarning("Assinatura ativa não encontrada para o Provider {ProviderId}", command.ProviderId);
            throw new NotFoundException("Subscription", command.ProviderId);
        }

        if (string.IsNullOrEmpty(subscription.ExternalCustomerId))
        {
            logger.LogWarning("Assinatura {SubscriptionId} não possui ExternalCustomerId", subscription.Id);
            throw new BusinessRuleException("MISSING_EXTERNAL_CUSTOMER_ID", $"Assinatura encontrada ({subscription.Id}), mas sem identificador de cliente externo.");
        }

        var maskedCustomerId = Subscription.MaskExternalId(subscription.ExternalCustomerId);
        logger.LogInformation("Assinatura {SubscriptionId} encontrada para o Cliente {CustomerId}. Criando sessão...", 
            subscription.Id, maskedCustomerId);

        var portalUrl = await paymentGateway.CreateBillingPortalSessionAsync(
            subscription.ExternalCustomerId, 
            command.ReturnUrl, 
            cancellationToken);

        if (string.IsNullOrEmpty(portalUrl))
        {
            logger.LogError("Falha no gateway ao gerar URL do portal para o Cliente {CustomerId}", maskedCustomerId);
            throw new BusinessRuleException("GATEWAY_SESSION_FAILURE", "Falha ao gerar sessão do Portal de Faturamento no provedor de pagamento.");
        }

        return portalUrl;
    }
}
