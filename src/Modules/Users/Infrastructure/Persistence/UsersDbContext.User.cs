using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Implementação parcial de UsersDbContext como IRepository para o agregado User.
/// </summary>
public partial class UsersDbContext : IRepository<User, UserId>
{
    async Task<User?> IRepository<User, UserId>.TryFindAsync(UserId key, CancellationToken cancellationToken) =>
        await Users.FirstOrDefaultAsync(u => u.Id == key, cancellationToken);

    void IRepository<User, UserId>.Add(User aggregate) =>
        Users.Add(aggregate);

    void IRepository<User, UserId>.Delete(User aggregate) =>
        Users.Remove(aggregate);
}
