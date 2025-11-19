using System.Reflection;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

/// <summary>
/// Contexto Entity Framework para o módulo ServiceCatalogs.
/// </summary>
public class ServiceCatalogsDbContext(DbContextOptions<ServiceCatalogsDbContext> options) : DbContext(options)
{
    public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ServiceCatalogs");

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
