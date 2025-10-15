using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Users.Domain.Repositories;

/// <summary>
/// Interface do repositório para operações de persistência da entidade User.
/// </summary>
/// <remarks>
/// Define o contrato para acesso a dados dos usuários seguindo o padrão Repository.
/// Implementa operações CRUD básicas e consultas especializadas para o domínio de usuários.
/// A implementação concreta deve estar na camada de infraestrutura.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Busca um usuário pelo seu identificador único.
    /// </summary>
    /// <param name="id">Identificador único do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>O usuário encontrado ou null se não existir</returns>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo endereço de email.
    /// </summary>
    /// <param name="email">Endereço de email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>O usuário encontrado ou null se não existir</returns>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo nome de usuário.
    /// </summary>
    /// <param name="username">Nome de usuário</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>O usuário encontrado ou null se não existir</returns>
    Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca múltiplos usuários pelos seus identificadores únicos em uma única consulta batch.
    /// </summary>
    /// <param name="userIds">Lista de identificadores dos usuários a serem buscados</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista com os usuários encontrados</returns>
    /// <remarks>
    /// Método otimizado para buscar múltiplos usuários em uma única query SQL usando WHERE IN.
    /// Substitui N queries individuais por uma única query batch, resolvendo o problema de N+1 queries.
    /// Para listas muito grandes (>2000 IDs), considere usar chunking para respeitar limites do SQL.
    /// </remarks>
    Task<IReadOnlyList<User>> GetUsersByIdsAsync(IReadOnlyList<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários com paginação.
    /// </summary>
    /// <param name="pageNumber">Número da página (base 1)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista paginada de usuários e o total de registros</returns>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuários com paginação e filtro de pesquisa otimizado.
    /// </summary>
    /// <param name="pageNumber">Número da página (base 1)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="searchTerm">Termo de pesquisa opcional para filtrar por email, nome de usuário ou nome completo</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Lista paginada de usuários filtrados e o total de registros</returns>
    /// <remarks>
    /// Método otimizado que utiliza execução paralela de contagem e busca de dados,
    /// além de índices compostos para melhor performance em consultas com filtros.
    /// </remarks>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedWithSearchAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário pelo identificador do Keycloak.
    /// </summary>
    /// <param name="keycloakId">Identificador do usuário no Keycloak</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>O usuário encontrado ou null se não existir</returns>
    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo usuário ao repositório.
    /// </summary>
    /// <param name="user">Usuário a ser adicionado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um usuário existente no repositório.
    /// </summary>
    /// <param name="user">Usuário com dados atualizados</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um usuário do repositório (exclusão física).
    /// </summary>
    /// <param name="id">Identificador do usuário a ser removido</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <remarks>
    /// Esta operação realiza exclusão física. Para exclusão lógica, use o método MarkAsDeleted da entidade User.
    /// </remarks>
    Task DeleteAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário existe no repositório.
    /// </summary>
    /// <param name="id">Identificador do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>True se o usuário existir, false caso contrário</returns>
    Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);
}