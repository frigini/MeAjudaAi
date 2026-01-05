using MeAjudaAi.Shared.Contracts.Functional;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

/// <summary>
/// Refit client interface para o endpoint de Service Catalogs.
/// </summary>
public interface IServiceCatalogsApi
{
    /// <summary>
    /// Recupera todas as categorias de serviços.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas categorias ativas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Get("/api/v1/service-catalogs/categories")]
    Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(
        [Query] bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Get("/api/v1/service-catalogs/services")]
    Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(
        [Query] bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um serviço por ID.
    /// </summary>
    /// <param name="serviceId">Identificador do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Get("/api/v1/service-catalogs/services/{serviceId}")]
    Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços de uma categoria específica.
    /// </summary>
    /// <param name="categoryId">Identificador da categoria</param>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Get("/api/v1/service-catalogs/services/category/{categoryId}")]
    Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(
        Guid categoryId,
        [Query] bool activeOnly = true,
        CancellationToken cancellationToken = default);
}
