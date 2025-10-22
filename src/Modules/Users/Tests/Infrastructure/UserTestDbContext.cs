using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure;

/// <summary>
/// DbContext específico para testes unitários de configuração do Entity Framework.
/// Utilizado para validar mapeamentos e configurações sem dependências externas.
/// </summary>
public class UserTestDbContext(DbContextOptions<UserTestDbContext> options) : DbContext(options)
{
    public DbSet<User>? Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
