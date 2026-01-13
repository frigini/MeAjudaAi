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
                // DESENVOLVIMENTO: Gerar SQL do modelo e executar diretamente
                logger.LogInformation("🔧 {Module}: Criando schema e tabelas a partir do modelo EF Core...", moduleName);
                
                const int maxRetries = 3;
                var retryCount = 0;
                var success = false;

                while (!success && retryCount < maxRetries)
                {
                    try
                    {
                        // Gerar script SQL do modelo EF Core
                        var createScript = dbContext.Database.GenerateCreateScript();
                        
                        logger.LogInformation("📝 {Module}: Script SQL gerado com {Length} caracteres", 
                            moduleName, createScript.Length);
                        
                        // DEBUGGING: Salvar script em arquivo temporário
                        var tempFile = Path.Combine(Path.GetTempPath(), $"ef_script_{moduleName}.sql");
                        await File.WriteAllTextAsync(tempFile, createScript, cancellationToken);
                        logger.LogWarning("🔍 {Module}: Script salvo em: {TempFile}", moduleName, tempFile);
                        
                        // Executar o script usando conexão direta (ExecuteSqlRaw não suporta múltiplos comandos DDL)
                        var connection = dbContext.Database.GetDbConnection();
                        await connection.OpenAsync(cancellationToken);
                        
                        try
                        {
                            using var command = connection.CreateCommand();
#pragma warning disable CA2100 // SQL vem do EF Core GenerateCreateScript(), não de input do usuário
                            command.CommandText = createScript;
#pragma warning restore CA2100
                            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                            
                            logger.LogInformation("✅ {Module}: Script executado ({Rows} rows affected)", 
                                moduleName, rowsAffected);
                                
                            // Verificar se as tabelas foram realmente criadas
                            // Extrair nome do schema do SQL gerado (busca por CREATE SCHEMA ou FROM pg_namespace)
                            var schemaMatch = System.Text.RegularExpressions.Regex.Match(
                                createScript, 
                                @"nspname = '([^']+)'|CREATE SCHEMA\s+([^\s;]+)", 
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            
                            var actualSchemaName = schemaMatch.Success 
                                ? (schemaMatch.Groups[1].Value.Length > 0 ? schemaMatch.Groups[1].Value : schemaMatch.Groups[2].Value)
                                : moduleName.ToLowerInvariant();
                            
                            using var verifyCmd = connection.CreateCommand();
#pragma warning disable CA2100 // Query do information_schema é segura
                            verifyCmd.CommandText = $@"
                                SELECT COUNT(*) 
                                FROM information_schema.tables 
                                WHERE table_schema = '{actualSchemaName}'";
#pragma warning restore CA2100
                            var tableCount = Convert.ToInt32(await verifyCmd.ExecuteScalarAsync(cancellationToken));
                            
                            logger.LogInformation("✅ {Module}: {TableCount} tabelas encontradas no schema '{Schema}'", 
                                moduleName, tableCount, actualSchemaName);
                                
                            if (tableCount == 0)
                            {
                                throw new InvalidOperationException(
                                    $"Script executado mas nenhuma tabela foi criada no schema '{actualSchemaName}'. " +
                                    $"Verifique o arquivo: {tempFile}");
                            }
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                        
                        success = true;
                    }
                    catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P07" || pgEx.SqlState == "42710")
                    {
                        // 42P07 = duplicate_table, 42710 = duplicate_object
                        logger.LogInformation("✓ {Module}: Schema e tabelas já existem", moduleName);
                        success = true;
                    }
                    catch (Exception ex) when (ex.Message.Contains("transient") || 
                                              ex.InnerException?.Message.Contains("connection was aborted") == true ||
                                              ex is Npgsql.NpgsqlException)
                    {
                        retryCount++;
                        if (retryCount < maxRetries)
                        {
                            logger.LogWarning("⚠️ {Module}: Erro transiente na tentativa {Retry}/{Max}, aguardando 2s...", 
                                moduleName, retryCount, maxRetries);
                            await Task.Delay(2000, cancellationToken);
                        }
                        else
                        {
                            logger.LogError(ex, "❌ {Module}: Falha após {Max} tentativas", moduleName, maxRetries);
                            throw;
                        }
                    }
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
