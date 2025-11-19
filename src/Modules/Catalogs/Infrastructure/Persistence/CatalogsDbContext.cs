using System.Reflection;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;

/// <summary>
/// Entity Framework context for the Catalogs module.
/// </summary>
public class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
{
    public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalogs");

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
