using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Queries;

/// <summary>
/// Implementação de IUserQueries usando Entity Framework Core com AsNoTracking.
/// </summary>
public sealed class DbContextUserQueries(UsersDbContext context) : IUserQueries
{
    private readonly UsersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetUsersByIdsAsync(IReadOnlyList<UserId> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds == null || userIds.Count == 0)
            return Array.Empty<User>();

        const int maxBatchSize = 2000;

        if (userIds.Count <= maxBatchSize)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
        }

        var allUsers = new List<User>();
        for (int i = 0; i < userIds.Count; i += maxBatchSize)
        {
            var chunk = userIds.Skip(i).Take(maxBatchSize).ToList();
            var chunkUsers = await _context.Users
                .AsNoTracking()
                .Where(u => chunk.Contains(u.Id))
                .ToListAsync(cancellationToken);
            allUsers.AddRange(chunkUsers);
        }

        return allUsers;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);
        var skip = (pageNumber - 1) * pageSize;
        var query = _context.Users.AsNoTracking().AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        return (users, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedWithSearchAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);
        var skip = (pageNumber - 1) * pageSize;
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.Email.Value.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                u.Username.Value.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                u.FirstName.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                u.LastName.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }

        var countTask = query.CountAsync(cancellationToken);
        var usersTask = query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        await Task.WhenAll(countTask, usersTask);
        var totalCount = await countTask;
        var users = await usersTask;
        return (users, totalCount);
    }

    /// <inheritdoc />
    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        return string.IsNullOrWhiteSpace(keycloakId)
            ? null
            : await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().AnyAsync(u => u.Id == id, cancellationToken);
    }
}
