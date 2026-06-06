using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Implementação parcial de UsersDbContext como IRepository para o agregado User.
/// </summary>
public partial class UsersDbContext : IRepository<User, UserId>
{
    public async Task<User?> TryFindAsync(UserId key, CancellationToken cancellationToken) =>
        await Users.FirstOrDefaultAsync(u => u.Id == key, cancellationToken);

    public void Add(User aggregate) =>
        Users.Add(aggregate);

    public void Delete(User aggregate) =>
        Users.Remove(aggregate);
}


