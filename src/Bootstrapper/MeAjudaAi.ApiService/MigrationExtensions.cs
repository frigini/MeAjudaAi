using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MeAjudaAi.ApiService;

/// <summary>
/// Extension methods para aplicar migrations dos módulos no startup
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Aplica migrations pendentes de todos os DbContexts dos módulos
    /// </summary>
    public static async Task ApplyModuleMigrationsAsync(this IHost app, CancellationToken cancellationToken = default)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("🔄 Starting migrations for all modules...");

        var dbContextTypes = DiscoverDbContextTypes(logger);
        
        if (dbContextTypes.Count == 0)
        {
            logger.LogWarning("⚠️ No DbContext found for migration");
            return;
        }

        logger.LogInformation("📋 Found {Count} DbContexts for migration", dbContextTypes.Count);

        // Read environment variables once for all DbContexts
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

        // Align with MigrationHostedService: filter only by MeAjudaAi.Modules
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
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
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ Error discovering types in assembly {AssemblyName}", 
                    assembly.FullName);
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
            // Get DbContext from DI container (already has connection string configured)
            var dbContext = services.GetRequiredService(contextType) as DbContext;

            if (dbContext == null)
            {
                throw new InvalidOperationException(
                    $"DbContext {contextType.Name} is not registered in DI container. " +
                    "Ensure the module was registered correctly.");
            }

            // Apply migrations using consolidated logic
            var migrationsApplied = await ApplyPendingMigrationsAsync(
                dbContext, 
                moduleName, 
                logger, 
                cancellationToken);

            // Generate debug script only in development when explicitly enabled and migrations were applied
            if (isDevelopment && enableDebugScripts && migrationsApplied)
            {
                try
                {
                    var createScript = dbContext.Database.GenerateCreateScript();
                    // NOTE: Debug script files accumulate in temp directory.
                    // Cleanup is handled by OS temp directory maintenance.
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
                logger.LogDebug("   - {Migration}", migration);
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
