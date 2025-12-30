using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Shared.Contracts.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;

/// <summary>
/// Contrato de API pública para o módulo ServiceCatalogs.
/// Fornece acesso a categorias de serviços e catálogo de serviços para outros módulos.
/// </summary>
public interface IServiceCatalogsModuleApi : IModuleApi
{
    // ============ Categorias de Serviços ============

    /// <summary>
    /// Recupera uma categoria de serviço por ID.
    /// </summary>
    /// <param name="categoryId">Identificador da categoria de serviço</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<Result<ModuleServiceCategoryDto?>> GetServiceCategoryByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todas as categorias de serviços.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas categorias ativas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<Result<IReadOnlyList<ModuleServiceCategoryDto>>> GetAllServiceCategoriesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    // ============ Serviços ============

    /// <summary>
    /// Recupera um serviço por ID.
    /// </summary>
    /// <param name="serviceId">Identificador do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<Result<ModuleServiceDto?>> GetServiceByIdAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<Result<IReadOnlyList<ModuleServiceListDto>>> GetAllServicesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços de uma categoria específica.
    /// </summary>
    /// <param name="categoryId">Identificador da categoria para filtrar serviços</param>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task<Result<IReadOnlyList<ModuleServiceDto>>> GetServicesByCategoryAsync(
        Guid categoryId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um serviço existe e está ativo.
    /// </summary>
    /// <param name="serviceId">Identificador do serviço a verificar</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado contendo verdadeiro se o serviço existe e está ativo, falso caso contrário</returns>
    Task<Result<bool>> IsServiceActiveAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida se todos os IDs de serviços fornecidos existem e estão ativos.
    /// </summary>
    /// <param name="serviceIds">Coleção de IDs de serviços a validar. Todos devem existir e estar ativos.</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado contendo o resultado da validação e lista de IDs de serviços inválidos</returns>
    Task<Result<ModuleServiceValidationResultDto>> ValidateServicesAsync(
        IReadOnlyCollection<Guid> serviceIds,
        CancellationToken cancellationToken = default);
}

