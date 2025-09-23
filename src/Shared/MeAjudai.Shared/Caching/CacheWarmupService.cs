using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Caching;

/// <summary>
/// Interface para serviços de cache warming.
/// Permite pré-carregar dados críticos no cache.
/// </summary>
public interface ICacheWarmupService
{
    /// <summary>
    /// Realiza o warmup do cache para dados críticos
    /// </summary>
    Task WarmupAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Realiza warmup específico por módulo
    /// </summary>
    Task WarmupModuleAsync(string moduleName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Serviço responsável pelo cache warming dos dados mais acessados.
/// Executado durante a inicialização da aplicação e periodicamente.
/// </summary>
public class CacheWarmupService : ICacheWarmupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmupService> _logger;
    private readonly Dictionary<string, Func<IServiceProvider, CancellationToken, Task>> _warmupStrategies;

    public CacheWarmupService(
        IServiceProvider serviceProvider,
        ILogger<CacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _warmupStrategies = [];
        
        // Registrar estratégias de warmup por módulo
        RegisterWarmupStrategies();
    }

    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting cache warmup for all modules");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var tasks = _warmupStrategies.Values.Select(strategy => 
                ExecuteSafeWarmup(strategy, cancellationToken));
            
            await Task.WhenAll(tasks);
            
            stopwatch.Stop();
            _logger.LogInformation("Cache warmup completed successfully in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed after {Duration}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task WarmupModuleAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        if (!_warmupStrategies.TryGetValue(moduleName, out var strategy))
        {
            _logger.LogWarning("No warmup strategy found for module {ModuleName}", moduleName);
            return;
        }

        _logger.LogInformation("Starting cache warmup for module {ModuleName}", moduleName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await strategy(_serviceProvider, cancellationToken);
            
            stopwatch.Stop();
            _logger.LogInformation("Cache warmup for module {ModuleName} completed in {Duration}ms", 
                moduleName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed for module {ModuleName} after {Duration}ms", 
                moduleName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void RegisterWarmupStrategies()
    {
        // Estratégia para o módulo Users
        _warmupStrategies["Users"] = WarmupUsersModule;
        
        // Futuras estratégias para outros módulos podem ser adicionadas aqui
        // _warmupStrategies["Help"] = WarmupHelpModule;
        // _warmupStrategies["Notifications"] = WarmupNotificationsModule;
    }

    private async Task WarmupUsersModule(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting Users module cache warmup");
        
        using var scope = serviceProvider.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        
        // Cache configurações do sistema relacionadas a usuários
        await WarmupUserSystemConfigurations(cacheService, cancellationToken);
        
        _logger.LogDebug("Users module cache warmup completed");
    }

    private async Task WarmupUserSystemConfigurations(ICacheService cacheService, CancellationToken cancellationToken)
    {
        try
        {
            // Exemplo: cachear configurações que são frequentemente acessadas
            var configKey = "user-system-config";
            
            await cacheService.GetOrCreateAsync(
                configKey,
                async _ =>
                {
                    // Simular carregamento de configurações do sistema
                    // Na implementação real, isso viria de um repositório
                    await Task.Delay(10, cancellationToken); // Simular I/O
                    return new { MaxUsersPerPage = 50, DefaultUserRole = "Customer" };
                },
                TimeSpan.FromHours(6),
                tags: [CacheTags.Configuration, CacheTags.Users],
                cancellationToken: cancellationToken);
                
            _logger.LogDebug("User system configurations warmed up");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to warmup user system configurations");
            // Não re-throw aqui para não quebrar todo o warmup
        }
    }

    private async Task ExecuteSafeWarmup(
        Func<IServiceProvider, CancellationToken, Task> warmupStrategy, 
        CancellationToken cancellationToken)
    {
        try
        {
            await warmupStrategy(_serviceProvider, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warmup strategy failed, continuing with others");
            // Não re-throw para não quebrar outras estratégias
        }
    }
}