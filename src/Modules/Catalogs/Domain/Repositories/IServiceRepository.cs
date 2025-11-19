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
    Task<Service?> GetByIdAsync(ServiceId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera múltiplos serviços por seus IDs (consulta em lote).
    /// </summary>
    Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<ServiceId> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um serviço por seu nome.
    /// </summary>
    Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços.
    /// </summary>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken"></param>
    Task<IReadOnlyList<Service>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera todos os serviços de uma categoria específica.
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <param name="activeOnly">Se verdadeiro, retorna apenas serviços ativos</param>
    /// <param name="cancellationToken"></param>
    Task<IReadOnlyList<Service>> GetByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe um serviço com o nome fornecido.
    /// </summary>
    /// <param name="name">O nome do serviço a verificar</param>
    /// <param name="excludeId">ID opcional do serviço a excluir da verificação</param>
    /// <param name="categoryId">ID opcional da categoria para restringir a verificação a uma categoria específica</param>
    /// <param name="cancellationToken"></param>
    Task<bool> ExistsWithNameAsync(string name, ServiceId? excludeId = null, ServiceCategoryId? categoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta quantos serviços existem em uma categoria.
    /// </summary>
    Task<int> CountByCategoryAsync(ServiceCategoryId categoryId, bool activeOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo serviço.
    /// </summary>
    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um serviço existente.
    /// </summary>
    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um serviço por seu ID (exclusão física - usar com cautela).
    /// </summary>
    Task DeleteAsync(ServiceId id, CancellationToken cancellationToken = default);
}
