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
        
        logger.LogInformation("🔄 Iniciando migrations de todos os módulos...");

        var dbContextTypes = DiscoverDbContextTypes(logger);
        
        if (dbContextTypes.Count == 0)
        {
            logger.LogWarning("⚠️ Nenhum DbContext encontrado para migração");
            return;
        }

        logger.LogInformation("📋 Encontrados {Count} DbContexts para migração", dbContextTypes.Count);

        using var scope = app.Services.CreateScope();
        
        foreach (var contextType in dbContextTypes)
        {
            await MigrateDbContextAsync(scope.ServiceProvider, contextType, logger, cancellationToken);
        }

        logger.LogInformation("✅ Todas as migrations foram aplicadas com sucesso!");
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
                    logger.LogDebug("✅ Descobertos {Count} DbContext(s) em {Assembly}", 
                        types.Count, assembly.GetName().Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ Erro ao descobrir tipos no assembly {AssemblyName}", 
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
        logger.LogInformation("🔧 Aplicando migrations para {Module}...", moduleName);

        try
        {
            // Obter DbContext do DI container (já tem connection string configurada)
            var dbContext = services.GetRequiredService(contextType) as DbContext;

            if (dbContext == null)
            {
                throw new InvalidOperationException(
                    $"DbContext {contextType.Name} não está registrado no DI container. " +
                    "Certifique-se de que o módulo foi registrado corretamente.");
            }

            // Estratégia diferenciada por ambiente
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isDevelopment)
            {
                // DESENVOLVIMENTO: Aplicar migrations via EF Core (popula __EFMigrationsHistory)
                logger.LogInformation("🔧 {Module}: Aplicando migrations em modo de desenvolvimento...", moduleName);
                
                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("📦 {Module}: {Count} migrations pendentes", moduleName, pendingMigrations.Count);
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogDebug("   - {Migration}", migration);
                    }
                    
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    logger.LogInformation("✅ {Module}: Migrations aplicadas com sucesso", moduleName);
                    
                    // DEBUGGING: Gerar script para inspeção (opcional)
                    var createScript = dbContext.Database.GenerateCreateScript();
                    var tempFile = Path.Combine(Path.GetTempPath(), $"ef_script_{moduleName}.sql");
                    await File.WriteAllTextAsync(tempFile, createScript, cancellationToken);
                    logger.LogDebug("🔍 {Module}: Script de referência salvo em: {TempFile}", moduleName, tempFile);
                }
                else
                {
                    logger.LogInformation("✓ {Module}: Nenhuma migration pendente", moduleName);
                }
            }
            else
            {
                // PRODUÇÃO: Usar migrations apropriadas
                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("📦 {Module}: {Count} migrations pendentes", moduleName, pendingMigrations.Count);
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogDebug("   - {Migration}", migration);
                    }

                    await dbContext.Database.MigrateAsync(cancellationToken);
                    logger.LogInformation("✅ {Module}: Migrations aplicadas com sucesso", moduleName);
                }
                else
                {
                    logger.LogInformation("✓ {Module}: Nenhuma migration pendente", moduleName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro ao aplicar migrations para {Module}", moduleName);
            throw new InvalidOperationException(
                $"Falha ao aplicar migrations do banco de dados para o módulo '{moduleName}' (DbContext: {contextType.Name})",
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
