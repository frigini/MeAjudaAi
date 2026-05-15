using MeAjudaAi.Modules.Ratings.Domain.Entities;

namespace MeAjudaAi.Modules.Ratings.Domain.Repositories;

/// <summary>
/// Repositório para operações de escrita da entidade Review.
/// </summary>
public interface IReviewRepository
{
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    Task UpdateAsync(Review review, CancellationToken cancellationToken = default);
}