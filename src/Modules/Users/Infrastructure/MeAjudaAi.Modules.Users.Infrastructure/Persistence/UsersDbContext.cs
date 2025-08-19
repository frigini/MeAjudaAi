using MeAjudaAi.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ServiceProvider> ServiceProviders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}