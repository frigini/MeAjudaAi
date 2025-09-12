using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;

public class UserRepository(UsersDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.MarkAsDeleted(); // Soft delete
            await UpdateAsync(user, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        var totalCount = await context.Users.CountAsync(cancellationToken);

        var users = await context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users.AsReadOnly(), totalCount);
    }
}