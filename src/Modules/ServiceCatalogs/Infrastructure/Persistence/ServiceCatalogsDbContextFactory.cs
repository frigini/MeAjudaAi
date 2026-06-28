using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;

public class ServiceCatalogsDbContextFactory : BaseDesignTimeDbContextFactory<ServiceCatalogsDbContext>
{
    protected override ServiceCatalogsDbContext CreateDbContextInstance(DbContextOptions<ServiceCatalogsDbContext> options)
    {
        return new ServiceCatalogsDbContext(options);
    }
}
