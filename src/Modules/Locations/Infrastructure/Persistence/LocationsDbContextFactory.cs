using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do LocationsDbContext em design time (para migrações)
/// </summary>
/// <remarks>
/// IMPORTANTE: Este pattern é essencial para migrations do EF Core funcionarem corretamente.
/// O namespace `MeAjudaAi.Modules.Locations.Infrastructure.Persistence` permite que
/// a BaseDesignTimeDbContextFactory detecte automaticamente:
/// - Module name: "Locations" (do namespace)
/// - Schema: "locations" (lowercase)
/// - Migrations assembly: "MeAjudaAi.Modules.Locations.Infrastructure"
/// </remarks>
public class LocationsDbContextFactory : BaseDesignTimeDbContextFactory<LocationsDbContext>
{
    protected override string GetDesignTimeConnectionString()
    {
        return "Host=localhost;Database=meajudaai_dev;Username=postgres;Password=postgres";
    }

    protected override string GetMigrationsAssembly()
    {
        return "MeAjudaAi.Modules.Locations.Infrastructure";
    }

    protected override string GetMigrationsHistorySchema()
    {
        return "locations";
    }

    protected override LocationsDbContext CreateDbContextInstance(DbContextOptions<LocationsDbContext> options)
    {
        return new LocationsDbContext(options);
    }
}
