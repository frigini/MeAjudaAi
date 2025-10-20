using System.Collections.Concurrent;
using System.Text.Json;
using Aspire.Hosting;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Fixture compartilhado para otimização máxima de performance em testes de integração
/// Reutiliza containers e mantém cache de schema para reduzir tempo de execução
/// </summary>
public class SharedTestFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim InitializationSemaphore = new(1, 1);
    private static SharedTestFixture? _instance;
    private static readonly Lock InstanceLock = new();

    // Cache de aplicação compartilhada
    private DistributedApplication? _app;
    private bool _isInitialized;

    // Cache de clients HTTP reutilizáveis
    private readonly ConcurrentDictionary<string, HttpClient> _httpClients = new();

    // Configurações otimizadas
    private static readonly TimeSpan InitializationTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan ResourceWaitTimeout = TimeSpan.FromSeconds(120);

    public static SharedTestFixture Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (InstanceLock)
                {
                    _instance ??= new SharedTestFixture();
                }
            }
            return _instance;
        }
    }

    public JsonSerializerOptions JsonOptions { get; } = SerializationDefaults.Api;

    public async ValueTask InitializeAsync()
    {
        if (_isInitialized) return;

        await InitializationSemaphore.WaitAsync();
        try
        {
            if (_isInitialized) return; // Double-check locking

            using var cancellationTokenSource = new CancellationTokenSource(InitializationTimeout);
            var cancellationToken = cancellationTokenSource.Token;

            Console.WriteLine("[SharedFixture] Inicializando aplicação compartilhada...");

            var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>(cancellationToken);

            // Configuração ultra-otimizada para testes
            appHostBuilder.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Error); // Apenas erros críticos
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None); // Sem logs de SQL
                logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Error);
                logging.AddFilter("Aspire.Hosting", LogLevel.Error);
                logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Error);
            });

            // Configuração de resilência super agressiva
            appHostBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler(options =>
                {
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(5);
                    options.Retry.MaxRetryAttempts = 1; // Mínimo para testes
                });
            });

            // Build e start
            _app = await appHostBuilder.BuildAsync(cancellationToken);
            await _app.StartAsync(cancellationToken);

            var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();

            // Aguardar recursos críticos em paralelo
            var postgresTask = resourceNotificationService
                .WaitForResourceAsync("postgres-test", KnownResourceStates.Running)
                .WaitAsync(ResourceWaitTimeout, cancellationToken);

            var apiTask = resourceNotificationService
                .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
                .WaitAsync(ResourceWaitTimeout, cancellationToken);

            await Task.WhenAll(postgresTask, apiTask);

            Console.WriteLine("[SharedFixture] Aplicação compartilhada inicializada com sucesso!");
            _isInitialized = true;
        }
        finally
        {
            InitializationSemaphore.Release();
        }
    }

    public HttpClient GetOrCreateHttpClient(string serviceName)
    {
        return _httpClients.GetOrAdd(serviceName, name =>
        {
            if (_app == null)
                throw new InvalidOperationException("Fixture não foi inicializado");

            var client = _app.CreateHttpClient(name);
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        });
    }

    public async Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetOrCreateHttpClient("apiservice");
            var response = await client.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isInitialized) return;

        Console.WriteLine("[SharedFixture] Disposing aplicação compartilhada...");

        foreach (var client in _httpClients.Values)
        {
            client?.Dispose();
        }
        _httpClients.Clear();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }

        _isInitialized = false;
    }
}
