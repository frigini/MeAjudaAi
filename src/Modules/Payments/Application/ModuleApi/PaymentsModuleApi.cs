using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Payments;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Application.ModuleApi;

[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class PaymentsModuleApi(
    ISubscriptionQueries subscriptionQueries,
    ILogger<PaymentsModuleApi> logger) : IPaymentsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = ModuleNames.Payments;
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    private static Contracts.Modules.Payments.Enums.ESubscriptionStatus MapStatus(Domain.Enums.ESubscriptionStatus status) => status switch
    {
        Domain.Enums.ESubscriptionStatus.Pending => Contracts.Modules.Payments.Enums.ESubscriptionStatus.Pending,
        Domain.Enums.ESubscriptionStatus.Active => Contracts.Modules.Payments.Enums.ESubscriptionStatus.Active,
        Domain.Enums.ESubscriptionStatus.Canceled => Contracts.Modules.Payments.Enums.ESubscriptionStatus.Canceled,
        Domain.Enums.ESubscriptionStatus.Expired => Contracts.Modules.Payments.Enums.ESubscriptionStatus.Expired,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Payments module availability");

            // Teste simples de conectividade com o banco de dados
            _ = await subscriptionQueries.GetActiveByProviderIdAsync(Guid.Empty, cancellationToken);

            logger.LogDebug("Payments module is available and healthy");
            return true;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogDebug(ex, "Payments module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Payments module availability");
            return false;
        }
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
                MapStatus(subscription.Status),
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
