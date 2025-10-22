using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Fixtures;

/// <summary>
/// Fixture compartilhado para otimizar performance dos testes
/// Reutiliza infraestrutura (containers, banco, etc.) entre múltiplos testes
/// </summary>
public class SharedTestFixture : IAsyncLifetime
{
    private static readonly Lock _lock = new();
    private static SharedTestFixture? _instance;
    private static int _referenceCount;

    public IHost? Host { get; private set; }
    public IServiceProvider Services => Host?.Services ?? throw new InvalidOperationException("Host not initialized");

    /// <summary>
    /// Singleton pattern para garantir uma única instância compartilhada
    /// </summary>
    public static SharedTestFixture GetInstance()
    {
        lock (_lock)
        {
            _instance ??= new SharedTestFixture();
            _referenceCount++;
            return _instance;
        }
    }

    public async ValueTask InitializeAsync()
    {
        if (Host != null) return; // Já inicializado

        var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                // Reduz logging durante testes para melhor performance
                logging.SetMinimumLevel(LogLevel.Warning);
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Warning);
            })
            .ConfigureServices(services =>
            {
                // Configurações compartilhadas para testes
                services.Configure<HostOptions>(options =>
                {
                    // Timeout mais rápido para testes
                    options.ShutdownTimeout = TimeSpan.FromSeconds(5);
                });
            });

        Host = hostBuilder.Build();
        await Host.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        lock (_lock)
        {
            _referenceCount--;
            if (_referenceCount > 0) return; // Ainda há referências ativas

            _instance = null;
        }

        if (Host != null)
        {
            await Host.StopAsync();
            Host.Dispose();
            Host = null;
        }

        GC.SuppressFinalize(this);
    }
}
