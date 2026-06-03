using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.ApiService;

/// <summary>
/// Extension methods para aplicar migrations dos módulos no startup
/// </summary>
[ExcludeFromCodeCoverage]
public static class MigrationExtensions
{
    /// <summary>
    /// Aplica migrations pendentes de todos os DbContexts dos módulos
    /// </summary>
    public static async Task ApplyModuleMigrationsAsync(this IHost app, CancellationToken cancellationToken = default)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("MeAjudaAi.Migrations");
        
        logger.LogInformation("🔄 Starting migrations for all modules...");

        var dbContextTypes = DiscoverDbContextTypes(logger);
        
        // Define a ordem de execução das migrações para respeitar restrições de chave estrangeira (FK)
        // entre tabelas de diferentes módulos no banco de dados compartilhado.
        var modulePriority = new Dictionary<string, int>
        {
            { "Users", 1 },
            { "ServiceCatalogs", 2 },
            { "Locations", 3 },
            { "Documents", 4 },
            { "Providers", 5 },
            { "Communications", 6 },
            { "Ratings", 7 },
            { "Payments", 8 },
            { "Bookings", 9 },
            { "SearchProviders", 10 }
        };

        dbContextTypes = dbContextTypes.OrderBy(t => 
        {
            var moduleName = ExtractModuleName(t);
            return modulePriority.TryGetValue(moduleName, out var p) ? p : 99;
        }).ThenBy(t => t.FullName).ToList();

        if (dbContextTypes.Count == 0)
        {
            logger.LogWarning("⚠️ No DbContext found for migration");
            return;
        }

        logger.LogInformation("📋 Found {Count} DbContexts for migration", dbContextTypes.Count);

        // Lê variáveis de ambiente uma vez para todos os DbContexts
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
        var enableDebugScripts = Environment.GetEnvironmentVariable("ENABLE_MIGRATION_DEBUG_SCRIPTS")
            ?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

        using var scope = app.Services.CreateScope();
        
        foreach (var contextType in dbContextTypes)
        {
            await MigrateDbContextAsync(scope.ServiceProvider, contextType, logger, isDevelopment, enableDebugScripts, cancellationToken);
        }

        logger.LogInformation("✅ All migrations applied successfully!");
    }

    private static List<Type> DiscoverDbContextTypes(ILogger logger)
    {
        var dbContextTypes = new List<Type>();

        // Filtra apenas assemblies de Infrastructure dos módulos, excluindo Tests
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .Where(a => a.FullName?.Contains(".Infrastructure") == true)
            .Where(a => a.FullName?.Contains(".Tests") != true) // Exclui assemblies de teste
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t))
                    .Where(t => t.Name.EndsWith("DbContext"))
                    .ToList();

                dbContextTypes.AddRange(types);

                if (types.Count > 0)
                {
                    logger.LogDebug("✅ Discovered {Count} DbContext(s) in {Assembly}", 
                        types.Count, assembly.GetName().Name);
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                // Alguns tipos falharam ao carregar; use os tipos que foram carregados e registre as exceções do carregador
                var loaded = ex.Types?
                    .Where(t => t != null && t.IsClass && !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t))
                    .Where(t => t!.Name.EndsWith("DbContext"))
                    .Select(t => t!)
                    .ToList();

                if (loaded != null && loaded.Count > 0)
                {
                    dbContextTypes.AddRange(loaded);
                    logger.LogDebug("✅ Discovered {Count} DbContext(s) in {Assembly} (partial load)", 
                        loaded.Count, assembly.GetName().Name);
                }

                logger.LogWarning(ex, "⚠️ Alguns tipos não puderam ser carregados do assembly {AssemblyName}. Verifique LoaderExceptions para detalhes.", assembly.FullName);
                if (ex.LoaderExceptions != null)
                {
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        logger.LogDebug(loaderEx, "LoaderException: {Message}", loaderEx?.Message);
                    }
                }
                }
                catch (FileLoadException ex)
                {
                logger.LogWarning(ex, "⚠️ Falha ao carregar o assembly {AssemblyName}", assembly.FullName);
                // continua com o próximo assembly
                }
                catch (BadImageFormatException ex)
                {
                logger.LogWarning(ex, "⚠️ Formato de imagem inválido para o assembly {AssemblyName}", assembly.FullName);
                // continua com o próximo assembly
                }

        }

        return dbContextTypes;
    }

    private static async Task MigrateDbContextAsync(
        IServiceProvider services, 
        Type contextType, 
        ILogger logger,
        bool isDevelopment,
        bool enableDebugScripts,
        CancellationToken cancellationToken)
    {
        var moduleName = ExtractModuleName(contextType);
        logger.LogInformation("🔧 Applying migrations for {Module}...", moduleName);

        try
        {
            // Obtém DbContext do container DI (já tem a connection string configurada)
            if (services.GetRequiredService(contextType) is not DbContext dbContext)
            {
                throw new InvalidOperationException(
                    $"DbContext {contextType.Name} could not be resolved as DbContext. " +
                    "Ensure the module is registered correctly and derives from DbContext.");
            }

            // Aplica migrations usando lógica consolidada
            var migrationsApplied = await ApplyPendingMigrationsAsync(
                dbContext, 
                moduleName, 
                logger, 
                cancellationToken);

            // Gera script de debug apenas em desenvolvimento quando explicitamente habilitado e migrations foram aplicadas
            if (isDevelopment && enableDebugScripts && migrationsApplied)
            {
                try
                {
                    var createScript = dbContext.Database.GenerateCreateScript();
                    // NOTA: Arquivos de script de debug acumulam no diretório temp.
                    // Limpeza é tratada pela manutenção do diretório temp do SO.
                    var tempFile = Path.Combine(Path.GetTempPath(), $"ef_script_{moduleName}.sql");
                    await File.WriteAllTextAsync(tempFile, createScript, cancellationToken);
                    logger.LogDebug("🔍 {Module}: Reference script saved at: {TempFile}", moduleName, tempFile);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "⚠️ {Module}: Failed to generate debug script", moduleName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error applying migrations for {Module}", moduleName);
            throw new InvalidOperationException(
                $"Failed to apply database migrations for module '{moduleName}' (DbContext: {contextType.Name})",
                ex);
        }
    }

    private static async Task<bool> ApplyPendingMigrationsAsync(
        DbContext dbContext,
        string moduleName,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pendingMigrations.Any())
        {
            logger.LogInformation("📦 {Module}: {Count} pending migrations", moduleName, pendingMigrations.Count);
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("   - Applying {Migration}", migration);
            }

            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("✅ {Module}: Migrations applied successfully", moduleName);
            return true;
        }

        logger.LogInformation("✓ {Module}: No pending migrations", moduleName);
        return false;
    }

    private static string ExtractModuleName(Type contextType)
    {
        var fullName = contextType.FullName ?? contextType.Name;
        var parts = fullName.Split('.');
        
        // Exemplo: MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext
        // Retorna: Documents
        var moduleIndex = Array.IndexOf(parts, "Modules");
        if (moduleIndex >= 0 && moduleIndex + 1 < parts.Length)
        {
            return parts[moduleIndex + 1];
        }

        return contextType.Name.Replace("DbContext", "");
    }
}
