using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Application.Queries.Interfaces;

/// <summary>
/// Interface para consultas otimizadas de leitura (NoTracking) do módulo Users.
/// </summary>
public interface IUserQueries
{
    /// <summary>
    /// Verifica se o módulo consegue conectar ao banco de dados.
    /// </summary>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo identificador único sem rastreamento do EF Core (AsNoTracking).
    /// </summary>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo endereço de email sem rastreamento.
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo nome de usuário sem rastreamento.
    /// </summary>
    Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca múltiplos usuários pelos seus identificadores únicos.
    /// </summary>
    Task<IReadOnlyList<User>> GetUsersByIdsAsync(IReadOnlyList<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários com paginação de forma otimizada.
    /// </summary>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários com paginação e filtro de pesquisa sem rastreamento.
    /// </summary>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedWithSearchAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo identificador do Keycloak.
    /// </summary>
    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário existe no banco de dados.
    /// </summary>
    Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);
}
