using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class GetBillingPortalCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentGateway paymentGateway,
    IConfiguration configuration,
    ILogger<GetBillingPortalCommandHandler> logger) : ICommandHandler<GetBillingPortalCommand, string>
{
    public async Task<string> HandleAsync(GetBillingPortalCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating billing portal for Provider {ProviderId}", command.ProviderId);
        
        ValidateReturnUrl(command.ReturnUrl);

        var subscription = await subscriptionRepository.GetActiveByProviderIdAsync(command.ProviderId, cancellationToken);
        
        if (subscription == null)
        {
            logger.LogWarning("Active subscription not found for Provider {ProviderId}", command.ProviderId);
            throw new NotFoundException("Subscription", command.ProviderId);
        }

        if (string.IsNullOrEmpty(subscription.ExternalCustomerId))
        {
            logger.LogWarning("Subscription {SubscriptionId} missing ExternalCustomerId", subscription.Id);
            throw new BusinessRuleException("MISSING_EXTERNAL_CUSTOMER_ID", $"Assinatura encontrada ({subscription.Id}), mas sem identificador de cliente externo.");
        }

        var maskedCustomerId = Subscription.MaskExternalId(subscription.ExternalCustomerId);
        logger.LogInformation("Subscription {SubscriptionId} found for Customer {CustomerId}. Creating session...", 
            subscription.Id, maskedCustomerId);

        var portalUrl = await paymentGateway.CreateBillingPortalSessionAsync(
            subscription.ExternalCustomerId, 
            command.ReturnUrl, 
            cancellationToken);

        if (string.IsNullOrEmpty(portalUrl))
        {
            logger.LogError("Gateway failed to generate portal URL for Customer {CustomerId}", maskedCustomerId);
            throw new BusinessRuleException("GATEWAY_SESSION_FAILURE", "Falha ao gerar sessão do Portal de Faturamento no provedor de pagamento.");
        }

        return portalUrl;
    }

    private void ValidateReturnUrl(string returnUrl)
    {
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
        {
            throw new BusinessRuleException("INVALID_RETURN_URL", "A URL de retorno deve ser uma URL absoluta válida.");
        }

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Host != "localhost")
        {
            throw new BusinessRuleException("INVALID_RETURN_URL_SCHEME", "A URL de retorno deve utilizar o protocolo HTTPS.");
        }

        var allowedHostsSection = configuration.GetSection("Payments:AllowedReturnHosts");
        var allowedHosts = allowedHostsSection.GetChildren().Select(c => c.Value).Where(v => v != null).ToList();
        
        if (!allowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
        {
            logger.LogWarning("Blocked billing portal redirect to untrusted host: {Host}", uri.Host);
            throw new BusinessRuleException("UNTRUSTED_RETURN_HOST", "O domínio da URL de retorno não é confiável.");
        }
    }
}
