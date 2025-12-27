using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(UsersDbContext context, TimeProvider timeProvider) : IUserRepository
{
    private readonly UsersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersByIdsAsync(IReadOnlyList<UserId> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds == null || userIds.Count == 0)
            return Array.Empty<User>();

        // Para listas muito grandes, considerar chunking para respeitar limites do SQL Server (~2100 parâmetros)
        const int maxBatchSize = 2000;

        if (userIds.Count <= maxBatchSize)
        {
            // Caso simples: uma única query batch
            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
        }

        // Caso complexo: dividir em chunks para listas muito grandes
        var allUsers = new List<User>();
        for (int i = 0; i < userIds.Count; i += maxBatchSize)
        {
            var chunk = userIds.Skip(i).Take(maxBatchSize).ToList();
            var chunkUsers = await _context.Users
                .Where(u => chunk.Contains(u.Id))
                .ToListAsync(cancellationToken);
            allUsers.AddRange(chunkUsers);
        }

        return allUsers;
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = _context.Users.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        return (users, totalCount);
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedWithSearchAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        var query = _context.Users.AsQueryable();

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

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        return string.IsNullOrWhiteSpace(keycloakId)
            ? null
            : await _context.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.MarkAsDeleted(_timeProvider);
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }
}
