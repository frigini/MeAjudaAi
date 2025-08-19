using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;

namespace MeAjudaAi.Modules.Users.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? role = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task DeleteAsync(UserId id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}