using Aspire.Hosting.Testing;
using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bogus;
using MeAjudaAi.Shared.Serialization;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Fixture compartilhado para otimização máxima de performance em testes de integração
/// Reutiliza containers e mantém cache de schema para reduzir tempo de execução
/// </summary>
public class SharedTestFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim InitializationSemaphore = new(1, 1);
    private static SharedTestFixture? _instance;
    private static readonly object InstanceLock = new();
    
    // Cache de aplicação compartilhada
    private DistributedApplication? _app;
    private bool _isInitialized = false;
    
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

    public async Task InitializeAsync()
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

    public async Task DisposeAsync()
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

/// <summary>
/// Base class ultra-otimizada que usa fixture compartilhado
/// <summary>
/// Base class compartilhada para testes de integração com máxima reutilização de recursos
/// </summary>
public abstract class SharedTestBase : IAsyncLifetime, IClassFixture<SharedTestFixture>
{
    private readonly SharedTestFixture _sharedFixture;
    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    protected SharedTestBase(SharedTestFixture sharedFixture)
    {
        _sharedFixture = sharedFixture;
    }

    public virtual async Task InitializeAsync()
    {
        // Usa o fixture compartilhado que já está inicializado
        await _sharedFixture.InitializeAsync();
        
        // Reutiliza o client HTTP do fixture
        ApiClient = _sharedFixture.GetOrCreateHttpClient("apiservice");
        
        // Verificação rápida de saúde (opcional, só se necessário)
        if (!await _sharedFixture.IsApiHealthyAsync())
        {
            await Task.Delay(1000); // Aguarda brevemente e tenta novamente
            if (!await _sharedFixture.IsApiHealthyAsync())
            {
                throw new InvalidOperationException("API não está saudável no fixture compartilhado");
            }
        }
    }

    // Métodos helper otimizados
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, _sharedFixture.JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PostAsync(requestUri, content, cancellationToken);
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, _sharedFixture.JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PutAsync(requestUri, content, cancellationToken);
    }

    protected async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, _sharedFixture.JsonOptions)!;
    }

    protected void SetAuthorizationHeader(string token)
    {
        ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthorizationHeader()
    {
        ApiClient.DefaultRequestHeaders.Authorization = null;
    }

    public virtual Task DisposeAsync()
    {
        // Não dispose do ApiClient aqui - ele é compartilhado
        // Apenas limpar headers específicos do teste se necessário
        ClearAuthorizationHeader();
        return Task.CompletedTask;
    }
}