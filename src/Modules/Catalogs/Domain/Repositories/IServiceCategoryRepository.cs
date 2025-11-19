using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Catalogs.Domain.Repositories;

/// <summary>
/// Contrato de repositório para o agregado ServiceCategory.
/// </summary>
public interface IServiceCategoryRepository
{
    /// <summary>
    /// Recupera uma categoria de serviço por seu ID.
    /// </summary>
    /// <param name="id">ID da categoria de serviço</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<ServiceCategory?> GetByIdAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera uma categoria de serviço por seu nome.
    /// </summary>
    /// <param name="name">Nome da categoria de serviço</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<ServiceCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todas as categorias de serviço.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas categorias ativas</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<IReadOnlyList<ServiceCategory>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe uma categoria com o nome fornecido.
    /// </summary>
    /// <param name="name">Nome da categoria a verificar</param>
    /// <param name="excludeId">ID opcional da categoria a excluir da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<bool> ExistsWithNameAsync(string name, ServiceCategoryId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova categoria de serviço.
    /// </summary>
    /// <param name="category">Categoria de serviço a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma categoria de serviço existente.
    /// </summary>
    /// <param name="category">Categoria de serviço a ser atualizada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta uma categoria de serviço por seu ID (exclusão física - usar com cautela).
    /// </summary>
    /// <param name="id">ID da categoria de serviço a ser deletada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task DeleteAsync(ServiceCategoryId id, CancellationToken cancellationToken = default);
}
