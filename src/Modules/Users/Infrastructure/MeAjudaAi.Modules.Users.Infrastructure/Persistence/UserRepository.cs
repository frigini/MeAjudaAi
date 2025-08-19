using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValuleObjects;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ServiceProvider)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ServiceProvider)
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ServiceProvider)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? role = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.ServiceProvider)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => 
                u.Email.Value.Contains(searchTerm) ||
                u.Profile.FirstName.Contains(searchTerm) ||
                u.Profile.LastName.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Roles.Contains(role));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.Status.ToString() == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ServiceProvider)
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}