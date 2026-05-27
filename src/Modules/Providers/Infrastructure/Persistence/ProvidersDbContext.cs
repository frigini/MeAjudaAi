using System.Reflection;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa IUnitOfWork para gestão de transações e persistência atômica.
/// </remarks>
public partial class ProvidersDbContext(
    DbContextOptions<ProvidersDbContext> options,
    IServiceProvider serviceProvider) : BaseDbContext(options), IProviderUnitOfWork
{
    public DbSet<Provider> Providers => Set<Provider>();

    /// <summary>
    /// Obtém o repositório tipado para um agregado do domínio.
    /// </summary>
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
        {
            return repository;
        }

        // Delegação inteligente: se este DbContext não suporta o agregado, 
        // tenta resolver o repositório a partir do container de DI.
        var externalRepository = serviceProvider.GetService<IRepository<TAggregate, TKey>>();
        if (externalRepository != null)
        {
            return externalRepository;
        }

        throw new NotSupportedException($"Repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name} is not supported by {nameof(ProvidersDbContext)} or any other registered module.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("providers");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
