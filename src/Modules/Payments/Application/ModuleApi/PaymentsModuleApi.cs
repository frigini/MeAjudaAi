using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Payments;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Payments.Application.ModuleApi;

[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class PaymentsModuleApi(
    IPaymentsHealthQueries healthQueries,
    ISubscriptionQueries subscriptionQueries) : IPaymentsModuleApi
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
        return await healthQueries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<ModuleSubscriptionDto?>> GetActiveSubscriptionByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
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

    public async Task<Result<bool>> HasActiveSubscriptionAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionQueries.GetActiveByProviderIdAsync(providerId, cancellationToken);
        return Result<bool>.Success(subscription != null);
    }
}
