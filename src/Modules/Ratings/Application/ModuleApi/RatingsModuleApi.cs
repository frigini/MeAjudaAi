using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Ratings;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Application.ModuleApi;

[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class RatingsModuleApi(
    IReviewQueries reviewQueries,
    ILogger<RatingsModuleApi> logger) : IRatingsModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = ModuleNames.Bookings;
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await reviewQueries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<ProviderRatingDto>> GetProviderRatingAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var (average, total) = await reviewQueries.GetAverageRatingForProviderAsync(providerId, cancellationToken);
            
            return Result<ProviderRatingDto>.Success(new ProviderRatingDto(providerId, average, total));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            logger.LogError(ex, "Error getting rating for provider {ProviderId}", providerId);
            return Result<ProviderRatingDto>.Failure("Error retrieving rating data.");
        }
    }

    public async Task<Result<bool>> HasCustomerReviewedProviderAsync(Guid customerId, Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await reviewQueries.GetByProviderAndCustomerAsync(providerId, customerId, cancellationToken);
            return Result<bool>.Success(review != null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            logger.LogError(ex, "Error checking review for customer {CustomerId} and provider {ProviderId}", customerId, providerId);
            return Result<bool>.Failure("Error checking review status.");
        }
    }
}
