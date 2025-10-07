using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Shared.Contracts.Modules.Users;

/// <summary>
/// API pública do módulo Users para consumo por outros módulos
/// </summary>
public interface IUsersModuleApi
{
    /// <summary>
    /// Obtém dados básicos de um usuário por ID
    /// </summary>
    Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém dados básicos de um usuário por email
    /// </summary>
    Task<Result<ModuleUserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém informações básicas de múltiplos usuários
    /// </summary>
    Task<Result<IReadOnlyList<ModuleUserBasicDto>>> GetUsersBatchAsync(IReadOnlyList<Guid> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um usuário existe
    /// </summary>
    Task<Result<bool>> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um email já está em uso
    /// </summary>
    Task<Result<bool>> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um username já está em uso
    /// </summary>
    Task<Result<bool>> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}