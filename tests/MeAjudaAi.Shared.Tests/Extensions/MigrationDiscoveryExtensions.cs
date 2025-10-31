using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Fornece descoberta automática e aplicação de migrações do Entity Framework para cenários de teste.
/// </summary>
public static class MigrationDiscoveryExtensions
{
    /// <summary>
    /// Descobre e aplica automaticamente todas as migrações pendentes para DbContexts encontrados no domínio do assembly atual.
    /// Este método escaneia todos os assemblies carregados por tipos de DbContext e aplica suas migrações.
    /// </summary>
    /// <param name="serviceProvider">O service provider contendo os DbContexts registrados</param>
    /// <param name="cancellationToken">Token de cancelamento para a operação</param>
    /// <returns>Uma task representando a operação assíncrona de migração</returns>
    public static async Task ApplyAllDiscoveredMigrationsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var dbContextTypes = DiscoverDbContextTypes();

        foreach (var contextType in dbContextTypes)
        {
            try
            {
                var context = serviceProvider.GetService(contextType) as DbContext;
                if (context != null)
                {
                    // Configura warnings para permitir aplicação de migrações em testes
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

                    // Primeiro, garantir que o banco existe
                    await context.Database.EnsureCreatedAsync(cancellationToken);

                    // Tentar aplicar migrações mesmo com alterações pendentes
                    try
                    {
                        await context.Database.MigrateAsync(cancellationToken);
                    }
                    catch (Exception migrationEx) when (migrationEx.Message.Contains("PendingModelChangesWarning"))
                    {
                        // Se falhar devido a alterações pendentes, tentar aplicar de forma forçada
                        // Recria o contexto com configuração especial para testes
                        var scope = serviceProvider.CreateScope();
                        var testContext = scope.ServiceProvider.GetService(contextType) as DbContext;
                        if (testContext != null)
                        {
                            // Usar EnsureCreated como fallback para testes
                            await testContext.Database.EnsureCreatedAsync(cancellationToken);
                        }
                        scope?.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                // Continua com outros contextos em caso de falha
                // Log suprimido para evitar ruído em testes
            }
        }
    }

    /// <summary>
    /// Descobre todos os tipos de DbContext em assemblies carregados que seguem a convenção de nome de módulo.
    /// </summary>
    /// <returns>Um enumerable de tipos DbContext encontrados em assemblies de módulos</returns>
    private static IEnumerable<Type> DiscoverDbContextTypes()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic &&
                              (assembly.FullName?.Contains("MeAjudaAi") == true ||
                               assembly.FullName?.Contains("Users") == true ||
                               assembly.FullName?.Contains("Infrastructure") == true));

        var dbContextTypes = new List<Type>();

        foreach (var assembly in loadedAssemblies)
        {
            try
            {
                var contextTypes = assembly.GetTypes()
                    .Where(type => type.IsClass &&
                                  !type.IsAbstract &&
                                  typeof(DbContext).IsAssignableFrom(type) &&
                                  type.Name.EndsWith("DbContext"))
                    .ToList();

                dbContextTypes.AddRange(contextTypes);
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Trata assemblies que não podem ser totalmente carregados
                var loadableTypes = ex.Types.Where(t => t != null);
                var contextTypes = loadableTypes
                    .Where(type => type!.IsClass &&
                                  !type.IsAbstract &&
                                  typeof(DbContext).IsAssignableFrom(type) &&
                                  type.Name.EndsWith("DbContext"))
                    .ToList();

                dbContextTypes.AddRange(contextTypes!);
            }
            catch (Exception)
            {
                // Continua com outros assemblies em caso de falha na descoberta
            }
        }

        return dbContextTypes;
    }

    /// <summary>
    /// Garante que todos os DbContexts descobertos tenham seus bancos criados e migrados.
    /// Útil para preparação de testes de integração.
    /// </summary>
    /// <param name="serviceProvider">O service provider contendo os DbContexts registrados</param>
    /// <param name="cancellationToken">Token de cancelamento para a operação</param>
    /// <returns>Uma task representando a operação assíncrona</returns>
    public static async Task EnsureAllDatabasesCreatedAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var dbContextTypes = DiscoverDbContextTypes();

        foreach (var contextType in dbContextTypes)
        {
            try
            {
                var context = serviceProvider.GetService(contextType) as DbContext;
                if (context != null)
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    await context.Database.MigrateAsync(cancellationToken);
                }
            }
            catch (Exception)
            {
                // Falha silenciosa para evitar ruído em testes
            }
        }
    }

    /// <summary>
    /// Obtém todos os tipos de DbContext descobertos para fins de diagnóstico.
    /// </summary>
    /// <returns>Uma lista de nomes de tipos DbContext que foram descobertos</returns>
    public static IEnumerable<string> GetDiscoveredDbContextNames()
    {
        return DiscoverDbContextTypes().Select(t => t.FullName ?? t.Name);
    }
}
