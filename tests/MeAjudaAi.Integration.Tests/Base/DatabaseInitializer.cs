using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Helper para inicialização de banco com cache
/// </summary>
public class DatabaseInitializer
{
    private readonly DatabaseSchemaCacheService _cacheService;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        DatabaseSchemaCacheService cacheService,
        ILogger<DatabaseInitializer> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

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
            if (await _cacheService.CanReuseSchemaAsync(connectionString, moduleName))
            {
                _logger.LogInformation("[OptimizedInit] Schema reused for {Module} in {ElapsedMs}ms",
                    moduleName, stopwatch.ElapsedMilliseconds);
                return false; // Não precisou inicializar
            }

            // Executar inicialização
            _logger.LogInformation("[OptimizedInit] Initializing schema for {Module}...", moduleName);
            await initializationAction();

            // Marcar como inicializado no cache
            await _cacheService.MarkSchemaAsInitializedAsync(connectionString, moduleName);

            _logger.LogInformation("[OptimizedInit] Schema initialized for {Module} in {ElapsedMs}ms",
                moduleName, stopwatch.ElapsedMilliseconds);

            return true; // Inicializou com sucesso
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OptimizedInit] Schema initialization failed for {Module}", moduleName);

            // Invalidar cache em caso de erro
            await _cacheService.InvalidateCacheAsync(connectionString, moduleName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
