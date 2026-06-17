using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados Entity Framework Core para o módulo Users.
/// </summary>
public partial class UsersDbContext : BaseDbContext, IUnitOfWork
{
    private readonly IServiceProvider? _serviceProvider;

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }
    
    public UsersDbContext(DbContextOptions<UsersDbContext> options, IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    public DbSet<User> Users => Set<User>();

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
        var externalRepository = _serviceProvider?.GetService<IRepository<TAggregate, TKey>>();
        if (externalRepository != null)
        {
            return externalRepository;
        }

        throw new InvalidOperationException($"Repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name} is not supported by {nameof(UsersDbContext)}.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Users);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}


