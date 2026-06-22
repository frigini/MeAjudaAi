using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Commands;

public class CreateBillingPortalSessionCommandHandler(
    IQueryDispatcher queryDispatcher,
    IPaymentGateway gateway,
    ILogger<CreateBillingPortalSessionCommandHandler> logger) : ICommandHandler<CreateBillingPortalSessionCommand, string>
{
    public async Task<string> HandleAsync(CreateBillingPortalSessionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetActiveSubscriptionByProviderQuery(command.ProviderId, command.CorrelationId);
            var result = await queryDispatcher.QueryAsync<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>(query, cancellationToken);
            
            if (result.IsFailure)
            {
                logger.LogError("Query failed: {Error}", result.Error.Message);
                throw new BusinessRuleException("QUERY_FAILURE", result.Error.Message);
            }
            
            if (result.Value == null)
            {
                logger.LogWarning("Subscription not found for Provider {ProviderId}", command.ProviderId);
                throw new NotFoundException(nameof(Subscription), command.ProviderId);
            }

            var subscription = result.Value;
            
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
                logger.LogError("Failed to create billing portal session for Customer {CustomerId}", subscription.ExternalCustomerId);
                throw new BusinessRuleException("GATEWAY_SESSION_FAILURE", "Falha ao criar sessão do portal de faturamento.");
            }

            return portalUrl;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in CreateBillingPortalSessionCommandHandler: {Message}", ex.Message);
            throw;
        }
    }
}
