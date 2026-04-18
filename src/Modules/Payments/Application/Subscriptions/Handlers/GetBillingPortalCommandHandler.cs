using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class GetBillingPortalCommandHandler(
    ISubscriptionRepository repository,
    IPaymentGateway gateway,
    IConfiguration configuration,
    IOptions<PaymentsOptions> paymentsOptions,
    ILogger<GetBillingPortalCommandHandler> logger) : ICommandHandler<GetBillingPortalCommand, string>
{
    private readonly PaymentsOptions _options = paymentsOptions.Value;

    public async Task<string> HandleAsync(GetBillingPortalCommand command, CancellationToken cancellationToken = default)
    {
        ValidateReturnUrl(command.ReturnUrl);

        var subscription = await repository.GetActiveByProviderIdAsync(command.ProviderId, cancellationToken);
        if (subscription == null)
        {
            throw new NotFoundException(nameof(Subscription), command.ProviderId);
        }

        if (string.IsNullOrEmpty(subscription.ExternalCustomerId))
        {
            logger.LogError("Active subscription {Id} for Provider {ProviderId} is missing ExternalCustomerId", 
                subscription.Id, command.ProviderId);
            throw new BusinessRuleException("MISSING_EXTERNAL_CUSTOMER_ID", "Não foi possível localizar o cadastro no provedor de pagamento.");
        }

        var portalUrl = await gateway.CreateBillingPortalSessionAsync(
            subscription.ExternalCustomerId, 
            command.ReturnUrl, 
            cancellationToken);

        if (string.IsNullOrEmpty(portalUrl))
        {
            throw new BusinessRuleException("GATEWAY_SESSION_FAILURE", "Falha ao criar sessão do portal de faturamento.");
        }

        return portalUrl;
    }

    private void ValidateReturnUrl(string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            throw new BusinessRuleException("INVALID_RETURN_URL", "A URL de retorno é obrigatória.");

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            throw new BusinessRuleException("INVALID_RETURN_URL", "A URL de retorno informada é inválida.");

        var isLocalhost = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || 
                         uri.Host.Equals("127.0.0.1") || 
                         uri.Host.Equals("::1") ||
                         (System.Net.IPAddress.TryParse(uri.Host, out var ip) && System.Net.IPAddress.IsLoopback(ip));

        if (uri.Scheme != Uri.UriSchemeHttps && !isLocalhost)
            throw new BusinessRuleException("INVALID_RETURN_URL_SCHEME", "A URL de retorno deve usar o protocolo HTTPS.");

        var trustedHosts = new HashSet<string>(_options.AllowedReturnHosts, StringComparer.OrdinalIgnoreCase);
        
        // Incluir ClientBaseUrl na lista de confiáveis se configurado
        var clientBaseUrl = configuration["ClientBaseUrl"];
        if (!string.IsNullOrEmpty(clientBaseUrl) && Uri.TryCreate(clientBaseUrl, UriKind.Absolute, out var clientUri))
        {
            trustedHosts.Add(clientUri.Host);
        }

        if (!isLocalhost && !trustedHosts.Contains(uri.Host))
        {
            logger.LogWarning("Blocked billing portal redirect to untrusted host: {Host}. Trusted: {Trusted}", 
                uri.Host, string.Join(", ", trustedHosts));
            throw new BusinessRuleException("UNTRUSTED_RETURN_HOST", "O domínio da URL de retorno não é confiável.");
        }
    }
}
