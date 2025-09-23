using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

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
        var totalCount = countTask.Result;
        var users = usersTask.Result;
        return (users, totalCount);
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakId))
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.MarkAsDeleted();
            _context.Users.Update(user);
        }
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }
}