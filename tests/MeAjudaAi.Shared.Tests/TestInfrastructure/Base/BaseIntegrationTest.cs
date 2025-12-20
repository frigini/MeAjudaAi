using MeAjudaAi.Shared.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

/// <summary>
/// Classe base genérica para testes de integração com containers compartilhados.
/// Reduz significativamente o tempo de execução dos testes evitando criação/destruição de containers.
/// </summary>
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private ServiceProvider? _serviceProvider;
    private static bool _containersStarted;
    private static readonly Lock _startupLock = new();

    protected IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");

    /// <summary>
    /// Configurações específicas do teste (deve ser implementado pelos módulos)
    /// </summary>
    protected abstract TestInfrastructureOptions GetTestOptions();

    /// <summary>
    /// Configura serviços específicos do módulo (deve ser implementado pelos módulos)
    /// </summary>
    protected abstract void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options);

    /// <summary>
    /// Executa setup específico do módulo após a inicialização (opcional)
    /// </summary>
    protected virtual Task OnModuleInitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;

    public async ValueTask InitializeAsync()
    {
        // CRÍTICO: Garante que os containers sejam iniciados ANTES de qualquer configuração de serviços
        await EnsureContainersStartedAsync();

        // Configura serviços para este teste específico
        var services = new ServiceCollection();
        var testOptions = GetTestOptions();

        // Usa containers compartilhados - adiciona como singletons
        services.AddSingleton(SharedTestContainers.PostgreSql);

        // Configurar logging otimizado para testes
        services.AddLogging(builder =>
        {
            var silentMode = Environment.GetEnvironmentVariable("TEST_SILENT_LOGGING");
            if (!string.IsNullOrEmpty(silentMode) && silentMode.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                builder.ConfigureSilentLogging();
            }
            else
            {
                builder.ConfigureTestLogging();
            }
        });

        // Configura serviços específicos do módulo
        ConfigureModuleServices(services, testOptions);

        _serviceProvider = services.BuildServiceProvider();

        // Setup específico do módulo
        await OnModuleInitializeAsync(_serviceProvider);

        // Aplica automaticamente todas as migrações descobertas
        await SharedTestContainers.ApplyAllMigrationsAsync(_serviceProvider);

        // Setup adicional específico do teste
        await OnInitializeAsync();
    }

    private static async Task EnsureContainersStartedAsync()
    {
        // Double-check locking pattern para garantir thread safety
        if (_containersStarted) return;

        lock (_startupLock)
        {
            if (_containersStarted) return;
            _containersStarted = true;
        }

        Console.WriteLine("Starting shared containers...");

        // Inicia containers fora do lock
        await SharedTestContainers.StartAllAsync();

        Console.WriteLine("Shared containers started successfully!");
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup específico do teste
        await OnDisposeAsync();

        // Limpa dados sem parar containers (muito mais rápido)
        var testOptions = GetTestOptions();
        var schema = testOptions.Database?.Schema;
        await SharedTestContainers.CleanupDataAsync(schema);

        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Setup adicional específico do teste (sobrescrever se necessário)
    /// </summary>
    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Cleanup específico do teste (sobrescrever se necessário)
    /// </summary>
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Cria um escopo de serviços para o teste
    /// </summary>
    protected IServiceScope CreateScope() => ServiceProvider.CreateScope();

    /// <summary>
    /// Obtém um serviço específico
    /// </summary>
    protected T GetService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    /// <summary>
    /// Obtém um serviço específico do escopo
    /// </summary>
    protected T GetScopedService<T>(IServiceScope scope) where T : notnull =>
        scope.ServiceProvider.GetRequiredService<T>();
}
