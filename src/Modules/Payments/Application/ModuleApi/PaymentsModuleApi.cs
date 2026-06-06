using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Payments;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using MeAjudaAi.Modules.Payments.Application.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.ModuleApi;

[ModuleApi("Payments", "1.0")]
public sealed class PaymentsModuleApi(
    ISubscriptionQueries subscriptionQueries,
    ILogger<PaymentsModuleApi> logger) : IPaymentsModuleApi
{
    public string ModuleName => "Payments";
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(true); // Placeholder
    }

    public async Task<Result<ModuleSubscriptionDto?>> GetActiveSubscriptionByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await subscriptionQueries.GetActiveByProviderIdAsync(providerId, cancellationToken);
            
            if (subscription == null)
            {
                return Result<ModuleSubscriptionDto?>.Success(null);
            }

            var dto = new ModuleSubscriptionDto(
                subscription.Id,
                subscription.ProviderId,
                subscription.PlanId,
                subscription.Status.ToString(),
                subscription.ExpiresAt);

            return Result<ModuleSubscriptionDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active subscription for provider {ProviderId}", providerId);
            return Result<ModuleSubscriptionDto?>.Failure("Error retrieving subscription data.");
        }
    }

    public async Task<Result<bool>> HasActiveSubscriptionAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await subscriptionQueries.GetActiveByProviderIdAsync(providerId, cancellationToken);
            return Result<bool>.Success(subscription != null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking active subscription for provider {ProviderId}", providerId);
            return Result<bool>.Failure("Error checking subscription status.");
        }
    }
}
