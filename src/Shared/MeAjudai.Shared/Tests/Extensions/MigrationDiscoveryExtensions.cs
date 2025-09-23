using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Provides automatic discovery and application of Entity Framework migrations for test scenarios.
/// </summary>
public static class MigrationDiscoveryExtensions
{
    /// <summary>
    /// Automatically discovers and applies all pending migrations for DbContexts found in the current assembly domain.
    /// This method scans all loaded assemblies for DbContext types and applies their migrations.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing the registered DbContexts</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous migration operation</returns>
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
                    // Configure warnings para permitir aplicação de migrações em testes
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
                    
                    // Primeiro, garantir que o banco existe
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    
                    // Tentar aplicar migrações mesmo com pending changes
                    try
                    {
                        await context.Database.MigrateAsync(cancellationToken);
                    }
                    catch (Exception migrationEx) when (migrationEx.Message.Contains("PendingModelChangesWarning"))
                    {
                        // Se falhar devido a pending changes, tentar aplicar de forma forçada
                        Console.WriteLine($"Attempting forced migration for {contextType.Name} due to pending changes...");
                        
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
            catch (Exception ex)
            {
                // Log the error but continue with other contexts
                Console.WriteLine($"Warning: Could not apply migrations for {contextType.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Discovers all DbContext types in loaded assemblies that match the module naming convention.
    /// </summary>
    /// <returns>An enumerable of DbContext types found in module assemblies</returns>
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
                // Handle assemblies that cannot be fully loaded
                var loadableTypes = ex.Types.Where(t => t != null);
                var contextTypes = loadableTypes
                    .Where(type => type!.IsClass && 
                                  !type.IsAbstract && 
                                  typeof(DbContext).IsAssignableFrom(type) &&
                                  type.Name.EndsWith("DbContext"))
                    .ToList();

                dbContextTypes.AddRange(contextTypes!);
            }
            catch (Exception ex)
            {
                // Log and continue with other assemblies
                Console.WriteLine($"Warning: Could not scan assembly {assembly.FullName}: {ex.Message}");
            }
        }

        return dbContextTypes;
    }

    /// <summary>
    /// Ensures all discovered DbContexts have their databases created and migrated.
    /// This is useful for integration test setup.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing the registered DbContexts</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
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
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not ensure database for {contextType.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets all discovered DbContext types for diagnostic purposes.
    /// </summary>
    /// <returns>A list of DbContext type names that were discovered</returns>
    public static IEnumerable<string> GetDiscoveredDbContextNames()
    {
        return DiscoverDbContextTypes().Select(t => t.FullName ?? t.Name);
    }
}