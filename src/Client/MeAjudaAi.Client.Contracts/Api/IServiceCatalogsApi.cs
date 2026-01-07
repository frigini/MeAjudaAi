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

    // ========== CATEGORIES WRITE OPERATIONS ==========

    /// <summary>
    /// Cria uma nova categoria de serviços.
    /// </summary>
    /// <param name="request">Dados da categoria (Name, Description, DisplayOrder)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Post("/api/v1/service-catalogs/categories")]
    Task<Result<ModuleServiceCategoryDto>> CreateCategoryAsync(
        [Body] object request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma categoria existente.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="request">Dados atualizados (Name, Description, DisplayOrder)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Put("/api/v1/service-catalogs/categories/{categoryId}")]
    Task<Result<Unit>> UpdateCategoryAsync(
        Guid categoryId,
        [Body] object request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta uma categoria.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Delete("/api/v1/service-catalogs/categories/{categoryId}")]
    Task<Result<Unit>> DeleteCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ativa uma categoria.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Post("/api/v1/service-catalogs/categories/{categoryId}/activate")]
    Task<Result<Unit>> ActivateCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa uma categoria.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Post("/api/v1/service-catalogs/categories/{categoryId}/deactivate")]
    Task<Result<Unit>> DeactivateCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    // ========== SERVICES WRITE OPERATIONS ==========

    /// <summary>
    /// Cria um novo serviço.
    /// </summary>
    /// <param name="request">Dados do serviço (CategoryId, Name, Description, DisplayOrder)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Post("/api/v1/service-catalogs/services")]
    Task<Result<ModuleServiceDto>> CreateServiceAsync(
        [Body] object request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um serviço existente.
    /// </summary>
    /// <param name="serviceId">ID do serviço</param>
    /// <param name="request">Dados atualizados (Name, Description, DisplayOrder)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Put("/api/v1/service-catalogs/services/{serviceId}")]
    Task<Result<Unit>> UpdateServiceAsync(
        Guid serviceId,
        [Body] object request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um serviço.
    /// </summary>
    /// <param name="serviceId">ID do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Delete("/api/v1/service-catalogs/services/{serviceId}")]
    Task<Result<Unit>> DeleteServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ativa um serviço.
    /// </summary>
    /// <param name="serviceId">ID do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Post("/api/v1/service-catalogs/services/{serviceId}/activate")]
    Task<Result<Unit>> ActivateServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Desativa um serviço.
    /// </summary>
    /// <param name="serviceId">ID do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [Post("/api/v1/service-catalogs/services/{serviceId}/deactivate")]
    Task<Result<Unit>> DeactivateServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);
}
