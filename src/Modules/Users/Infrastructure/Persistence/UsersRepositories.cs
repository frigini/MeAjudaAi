using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

public partial class UsersDbContext : IRepository<User, Guid>, IRepository<User, UserId>
{
    async Task<User?> IRepository<User, Guid>.TryFindAsync(
        Guid key, CancellationToken ct) =>
        await Users.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<User, Guid>.Add(User aggregate) =>
        Users.Add(aggregate);

    void IRepository<User, Guid>.Delete(User aggregate) =>
        Users.Remove(aggregate);

    async Task<User?> IRepository<User, UserId>.TryFindAsync(
        UserId key, CancellationToken ct) =>
        await Users.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<User, UserId>.Add(User aggregate) =>
        Users.Add(aggregate);

    void IRepository<User, UserId>.Delete(User aggregate) =>
        Users.Remove(aggregate);
}
