using MeAjudaAi.Modules.Ratings.Domain.Entities;

namespace MeAjudaAi.Modules.Ratings.Application.Queries;

public interface IReviewQueries
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetByProviderIdAsync(Guid providerId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<Review?> GetByProviderAndCustomerAsync(Guid providerId, Guid customerId, CancellationToken cancellationToken = default);
    Task<(decimal AverageRating, int TotalReviews)> GetAverageRatingForProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
}
