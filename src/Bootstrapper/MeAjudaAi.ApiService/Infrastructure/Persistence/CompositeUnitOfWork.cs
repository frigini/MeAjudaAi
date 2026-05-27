using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.ApiService.Infrastructure.Persistence;

/// <summary>
/// Unidade de Trabalho composta para ambiente monolítico.
/// Resolve repositórios de qualquer módulo e garante que as alterações em todos
/// os contextos de banco de dados sejam persistidas.
/// </summary>
public sealed class CompositeUnitOfWork(IServiceProvider serviceProvider) : IUnitOfWork
{
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        // Tenta resolver o repositório diretamente do container de DI.
        // Cada módulo registra seu DbContext como IRepository para seus agregados.
        return serviceProvider.GetRequiredService<IRepository<TAggregate, TKey>>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var usersDb = serviceProvider.GetService<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>();
        var providersDb = serviceProvider.GetService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();
        var docsDb = serviceProvider.GetService<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>();
        var serviceDb = serviceProvider.GetService<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>();
        var locationsDb = serviceProvider.GetService<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>();
        var ratingsDb = serviceProvider.GetService<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>();
        var paymentsDb = serviceProvider.GetService<MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext>();
        var bookingsDb = serviceProvider.GetService<MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.BookingsDbContext>();
        var commsDb = serviceProvider.GetService<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>();
        var searchDb = serviceProvider.GetService<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>();

        var dbContexts = new List<Microsoft.EntityFrameworkCore.DbContext>();
        if (usersDb != null) dbContexts.Add(usersDb);
        if (providersDb != null) dbContexts.Add(providersDb);
        if (docsDb != null) dbContexts.Add(docsDb);
        if (serviceDb != null) dbContexts.Add(serviceDb);
        if (locationsDb != null) dbContexts.Add(locationsDb);
        if (ratingsDb != null) dbContexts.Add(ratingsDb);
        if (paymentsDb != null) dbContexts.Add(paymentsDb);
        if (bookingsDb != null) dbContexts.Add(bookingsDb);
        if (commsDb != null) dbContexts.Add(commsDb);
        if (searchDb != null) dbContexts.Add(searchDb);

        int total = 0;
        using var transaction = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            foreach (var db in dbContexts)
            {
                total += await db.SaveChangesAsync(cancellationToken);
            }
            transaction.Complete();
            return total;
        }
        catch
        {
            throw;
        }
    }
}
