using System.Reflection;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework para o módulo Providers.
/// </summary>
/// <remarks>
/// Implementa o padrão DbContext do Entity Framework Core para persistência
/// das entidades do domínio de prestadores de serviços.
/// </remarks>
public class ProvidersDbContext : DbContext
{
    /// <summary>
    /// Inicializa uma nova instância do contexto.
    /// </summary>
    /// <param name="options">Opções de configuração do contexto</param>
    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options) : base(options)
    {
    }
    /// <summary>
    /// Conjunto de dados para prestadores de serviços.
    /// </summary>
    public DbSet<Provider> Providers { get; set; } = null!;

    /// <summary>
    /// Configura o modelo de dados durante a criação do contexto.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("providers");

        // Aplica configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
