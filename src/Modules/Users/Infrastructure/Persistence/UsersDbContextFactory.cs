using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do UsersDbContext em design time (para migrações)
/// O nome do módulo é detectado automaticamente do namespace
/// </summary>
public class UsersDbContextFactory : BaseDesignTimeDbContextFactory<UsersDbContext>
{
    protected override UsersDbContext CreateDbContextInstance(DbContextOptions<UsersDbContext> options)
    {
        return new UsersDbContext(options);
    }
}
