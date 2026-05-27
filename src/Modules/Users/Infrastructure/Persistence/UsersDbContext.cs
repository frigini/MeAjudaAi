using System.Reflection;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Contexto de banco de dados Entity Framework Core para o módulo Users.
/// </summary>
/// <remarks>
/// Implementa IUnitOfWork e IRepository diretamente para permitir uma arquitetura
/// mais simples sem camadas intermediárias de repositório, conforme Phase 3 do projeto.
/// </remarks>
public partial class UsersDbContext(
    DbContextOptions<UsersDbContext> options,
    IServiceProvider serviceProvider) : BaseDbContext(options), IUserUnitOfWork
{
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
        var externalRepository = serviceProvider.GetService<IRepository<TAggregate, TKey>>();
        if (externalRepository != null)
        {
            return externalRepository;
        }

        throw new NotSupportedException($"Repository for {typeof(TAggregate).Name} with key {typeof(TKey).Name} is not supported by {nameof(UsersDbContext)} or any other registered module.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
