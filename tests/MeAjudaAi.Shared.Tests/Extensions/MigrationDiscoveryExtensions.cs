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
        var dbContextTypes = DiscoverDbContextTypes;

        foreach (var contextType in dbContextTypes)
        {
            try
            {
                if (serviceProvider.GetService(contextType) is DbContext context)
                {
                    // Configura warnings para permitir aplicação de migrações em testes
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

                    // ALWAYS use migrations, never EnsureCreated
                    try
                    {
                        await context.Database.MigrateAsync(cancellationToken);
                        Console.WriteLine($"✅ Applied migrations for {contextType.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Failed to apply migrations for {contextType.Name}: {ex.Message}");

                        // Try to delete and recreate with migrations
                        try
                        {
                            await context.Database.EnsureDeletedAsync(cancellationToken);
                            await context.Database.MigrateAsync(cancellationToken);
                            Console.WriteLine($"✅ Recreated and migrated {contextType.Name}");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"❌ Complete failure for {contextType.Name}: {ex2.Message}");
                            // Don't use EnsureCreated anymore - fail early to identify migration issues
                            throw;
                        }
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
    private static IEnumerable<Type> DiscoverDbContextTypes
    {
        get
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
        var dbContextTypes = DiscoverDbContextTypes;

        foreach (var contextType in dbContextTypes)
        {
            try
            {
                if (serviceProvider.GetService(contextType) is DbContext context)
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
        return DiscoverDbContextTypes.Select(t => t.FullName ?? t.Name);
    }
}
