using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Ratings.Domain.Repositories;

/// <summary>
/// Repositório para a entidade Review.
/// </summary>
public interface IReviewRepository
{
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    Task<Review?> GetByIdAsync(ReviewId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Review review, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recupera uma avaliação específica de um cliente para um prestador.
    /// </summary>
    Task<Review?> GetByProviderAndCustomerAsync(Guid providerId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a média de avaliações e total de reviews aprovados de um provedor.
    /// </summary>
    Task<(decimal AverageRating, int TotalReviews)> GetAverageRatingForProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
}
