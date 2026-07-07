using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Helper para inicialização de banco com cache
/// </summary>
public class DatabaseInitializer(
    DatabaseSchemaCacheService cacheService,
    ILogger<DatabaseInitializer> logger)
{

    /// <summary>
    /// Inicializa o banco de dados apenas se necessário (com base no cache)
    /// </summary>
    public async Task<bool> InitializeIfNeededAsync(
        string connectionString,
        string moduleName,
        Func<Task> initializationAction)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(moduleName);
        ArgumentNullException.ThrowIfNull(initializationAction);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Verificar se pode reutilizar schema existente
            if (await cacheService.CanReuseSchemaAsync(connectionString, moduleName))
            {
                logger.LogInformation("[OptimizedInit] Schema reused for {Module} in {ElapsedMs}ms",
                    moduleName, stopwatch.ElapsedMilliseconds);
                return false; // Não precisou inicializar
            }

            // Executar inicialização
            logger.LogInformation("[OptimizedInit] Initializing schema for {Module}...", moduleName);
            await initializationAction();

            // Marcar como inicializado no cache
            await cacheService.MarkSchemaAsInitializedAsync(connectionString, moduleName);

            logger.LogInformation("[OptimizedInit] Schema initialized for {Module} in {ElapsedMs}ms",
                moduleName, stopwatch.ElapsedMilliseconds);

            return true; // Inicializou com sucesso
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[OptimizedInit] Schema initialization failed for {Module}", moduleName);

            // Invalidar cache em caso de erro
            await DatabaseSchemaCacheService.InvalidateCacheAsync(connectionString, moduleName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
