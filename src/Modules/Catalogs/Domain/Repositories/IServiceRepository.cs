using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Catalogs.Domain.Repositories;

/// <summary>
/// Contrato de repositório para o agregado Service.
/// </summary>
public interface IServiceRepository
{
    /// <summary>
    /// Recupera um serviço por seu ID.
    /// </summary>
    /// <param name="id">ID do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera múltiplos serviços por seus IDs (consulta em lote).
    /// </summary>
    /// <param name="ids">Coleção de IDs de serviços</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um serviço por seu nome.
    /// </summary>
    /// <param name="name">Nome do serviço</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços de uma categoria específica.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe um serviço com o nome fornecido.
    /// </summary>
    /// <param name="name">O nome do serviço a verificar</param>
    /// <param name="excludeId">ID opcional do serviço a excluir da verificação</param>
    /// <param name="categoryId">ID opcional da categoria para restringir a verificação a uma categoria específica</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId = null, ServiceCategoryId? categoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta quantos serviços existem em uma categoria.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="activeOnly">Se verdadeiro, conta apenas serviços ativos</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo serviço.
    /// </summary>
    /// <param name="service">Serviço a ser adicionado</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um serviço existente.
    /// </summary>
    /// <param name="service">Serviço a ser atualizado</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um serviço por seu ID (exclusão física - usar com cautela).
    /// </summary>
    /// <param name="id">ID do serviço a ser deletado</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default);
}
