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
        // Em um ambiente monolítico com múltiplos DbContexts, precisamos salvar todos.
        // Resolvemos todos os contextos que herdam de BaseDbContext (ou DbContext).
        // Nota: No MeAjudaAi, cada módulo tem seu próprio DbContext.
        
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

        int total = 0;
        if (usersDb != null) total += await usersDb.SaveChangesAsync(cancellationToken);
        if (providersDb != null) total += await providersDb.SaveChangesAsync(cancellationToken);
        if (docsDb != null) total += await docsDb.SaveChangesAsync(cancellationToken);
        if (serviceDb != null) total += await serviceDb.SaveChangesAsync(cancellationToken);
        if (locationsDb != null) total += await locationsDb.SaveChangesAsync(cancellationToken);
        if (ratingsDb != null) total += await ratingsDb.SaveChangesAsync(cancellationToken);
        if (paymentsDb != null) total += await paymentsDb.SaveChangesAsync(cancellationToken);
        if (bookingsDb != null) total += await bookingsDb.SaveChangesAsync(cancellationToken);
        if (commsDb != null) total += await commsDb.SaveChangesAsync(cancellationToken);
        if (searchDb != null) total += await searchDb.SaveChangesAsync(cancellationToken);

        return total;
    }
}
