using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Test-specific IUnitOfWork that resolves the correct DbContext per aggregate type.
/// In production, each module registers its own non-keyed IUnitOfWork (last one wins = Payments).
/// This composite fixes that by finding which DbContext implements IRepository{TAggregate, TKey}.
/// </summary>
internal sealed class CompositeTestUnitOfWork(IServiceProvider serviceProvider) : IUnitOfWork
{
    private static readonly Type[] DbContextTypes =
    [
        typeof(MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext),
        typeof(MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext),
        typeof(MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.BookingsDbContext),
        typeof(MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext),
        typeof(MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext),
        typeof(MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext),
        typeof(MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext),
        typeof(MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext),
        typeof(MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext),
        typeof(MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext),
    ];

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        var repositoryInterfaceType = typeof(IRepository<TAggregate, TKey>);

        foreach (var dbContextType in DbContextTypes)
        {
            if (!repositoryInterfaceType.IsAssignableFrom(dbContextType))
                continue;

            var dbContext = serviceProvider.GetRequiredService(dbContextType);
            return (IRepository<TAggregate, TKey>)dbContext;
        }

        throw new InvalidOperationException(
            $"No DbContext found that implements {repositoryInterfaceType.FullName}");
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int total = 0;
        foreach (var dbContextType in DbContextTypes)
        {
            var service = serviceProvider.GetService(dbContextType);
            if (service is DbContext dbContext)
            {
                total += await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        return total;
    }
}
