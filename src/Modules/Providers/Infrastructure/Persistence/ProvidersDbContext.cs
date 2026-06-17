using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados para o módulo de prestadores de serviços.
/// </summary>
public partial class ProvidersDbContext : BaseDbContext, IUnitOfWork
{
    private readonly IServiceProvider? _serviceProvider;

    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options) : base(options) { }

    public ProvidersDbContext(
        DbContextOptions<ProvidersDbContext> options,
        IServiceProvider? serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }
    public DbSet<Provider> Providers => Set<Provider>();

    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (this is IRepository<TAggregate, TKey> repository)
        {
            return repository;
        }

        throw new InvalidOperationException($"Repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name} is not supported by {nameof(ProvidersDbContext)}.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Providers);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}