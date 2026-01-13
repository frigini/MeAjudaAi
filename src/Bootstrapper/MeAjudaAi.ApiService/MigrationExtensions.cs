using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        using var scope = app.Services.CreateScope();
        
        foreach (var contextType in dbContextTypes)
        {
            await MigrateDbContextAsync(scope.ServiceProvider, contextType, logger, cancellationToken);
        }

        logger.LogInformation("✅ All migrations applied successfully!");
    }

    private static List<Type> DiscoverDbContextTypes(ILogger logger)
    {
        var dbContextTypes = new List<Type>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true && 
                       a.FullName?.Contains("Infrastructure") == true)
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

            // Environment-specific strategy
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isDevelopment)
            {
                // DEVELOPMENT: Apply migrations via EF Core (populates __EFMigrationsHistory)
                logger.LogInformation("🔧 {Module}: Applying migrations in development mode...", moduleName);
                
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
                    
                    // DEBUGGING: Generate script for inspection (optional)
                    var createScript = dbContext.Database.GenerateCreateScript();
                    var tempFile = Path.Combine(Path.GetTempPath(), $"ef_script_{moduleName}.sql");
                    await File.WriteAllTextAsync(tempFile, createScript, cancellationToken);
                    logger.LogDebug("🔍 {Module}: Reference script saved at: {TempFile}", moduleName, tempFile);
                }
                else
                {
                    logger.LogInformation("✓ {Module}: No pending migrations", moduleName);
                }
            }
            else
            {
                // PRODUCTION: Use appropriate migrations
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
                }
                else
                {
                    logger.LogInformation("✓ {Module}: No pending migrations", moduleName);
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
