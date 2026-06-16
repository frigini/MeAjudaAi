using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;

namespace MeAjudaAi.Contracts.Modules.Ratings;

/// <summary>
/// API pública do módulo Ratings para consumo por outros módulos.
/// </summary>
public interface IRatingsModuleApi : IModuleApi
{
    /// <summary>
    /// Obtém a avaliação média e total de avaliações de um prestador.
    /// </summary>
    Task<Result<ProviderRatingDto>> GetProviderRatingAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um cliente já avaliou um prestador.
    /// </summary>
    Task<Result<bool>> HasCustomerReviewedProviderAsync(Guid customerId, Guid providerId, CancellationToken cancellationToken = default);
}
