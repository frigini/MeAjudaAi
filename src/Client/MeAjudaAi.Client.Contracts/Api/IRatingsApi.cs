using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IRatingsApi
{
    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Ratings.Base}")]
    Task<Result<Guid>> CreateReviewAsync(
        [Body] CreateReviewRequest request,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Ratings.Base}{ApiEndpoints.Ratings.GetById}")]
    Task<Result<ProviderReviewResponse>> GetReviewByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Ratings.Base}{ApiEndpoints.Ratings.GetByProvider}")]
    Task<Result<PagedResult<ProviderReviewResponse>>> GetProviderReviewsAsync(
        Guid providerId,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Ratings.Base}{ApiEndpoints.Ratings.GetStatus}")]
    Task<Result<ReviewStatusResponse>> GetReviewStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
