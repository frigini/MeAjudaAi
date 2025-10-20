using System.Net.Http.Headers;
using System.Text.Json;
using Aspire.Hosting;
using Bogus;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Base class focada em performance para testes de integração críticos
/// Otimizada para reduzir timeouts e acelerar execução
/// </summary>
public abstract class PerformanceTestBase : IAsyncLifetime
{
    private DistributedApplication _app = null!;

    protected HttpClient ApiClient { get; private set; } = null!;
    protected HttpClient KeycloakClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    // Timeouts otimizados
    protected static readonly TimeSpan AppStartTimeout = TimeSpan.FromMinutes(2); // Reduzido de 5 para 2 minutos
    protected static readonly TimeSpan ResourceTimeout = TimeSpan.FromSeconds(90); // Reduzido de 5 minutos para 90 segundos
    protected static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(30);

    protected JsonSerializerOptions JsonOptions { get; } = SerializationDefaults.Api;

    public virtual async ValueTask InitializeAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource(AppStartTimeout);
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            // Configurar AppHost com timeouts otimizados
            var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>(cancellationToken);

            // Configuração mínima de logging para reduzir overhead
            appHostBuilder.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning); // Apenas warnings e erros
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error); // Menos logs do EF
                logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                logging.AddFilter("Aspire", LogLevel.Error); // Menos logs do Aspire
            });

            // Configuração de resilência otimizada
            appHostBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler(options =>
                {
                    // Configuração mais agressiva para testes
                    options.TotalRequestTimeout.Timeout = DefaultRequestTimeout;
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                    options.Retry.MaxRetryAttempts = 2; // Reduzido de padrão
                });
            });

            // Build e start da aplicação
            _app = await appHostBuilder.BuildAsync(cancellationToken);
            await _app.StartAsync(cancellationToken);

            // Esperar apenas pelos recursos críticos com timeout reduzido
            var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
            var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

            // Esperar PostgreSQL (crítico) - skip container wait in CI
            if (!isCI)
            {
                await resourceNotificationService
                    .WaitForResourceAsync("postgres-local", KnownResourceStates.Running)
                    .WaitAsync(ResourceTimeout, cancellationToken);
            }

            // Esperar API Service (crítico)
            await resourceNotificationService
                .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
                .WaitAsync(ResourceTimeout, cancellationToken);

            // Criar clients HTTP
            ApiClient = _app.CreateHttpClient("apiservice");
            ApiClient.Timeout = DefaultRequestTimeout;

            // Configurar Keycloak client se disponível
            try
            {
                KeycloakClient = _app.CreateHttpClient("keycloak");
                KeycloakClient.Timeout = DefaultRequestTimeout;
            }
            catch
            {
                // Se Keycloak não estiver disponível, criar um client dummy
                KeycloakClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
            }

            // Verificação de health mais rápida
            await WaitForApiHealthAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await DisposeAsync();
            throw new InvalidOperationException($"Falha na inicialização dos testes: {ex.Message}", ex);
        }
    }

    private async Task WaitForApiHealthAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        const int delayMs = 2000; // 2 segundos entre tentativas

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var response = await ApiClient.GetAsync("/health", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return; // API está saudável
                }
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                // Log apenas na última tentativa
                if (attempt == maxAttempts)
                {
                    throw new InvalidOperationException($"API não respondeu após {maxAttempts} tentativas: {ex.Message}");
                }
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException($"API não ficou saudável após {maxAttempts} tentativas");
    }

    // Métodos helper otimizados
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PostAsync(requestUri, content, cancellationToken);
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PutAsync(requestUri, content, cancellationToken);
    }

    protected async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, JsonOptions)!;
    }

    protected void SetAuthorizationHeader(string token)
    {
        ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            KeycloakClient?.Dispose();
            ApiClient?.Dispose();

            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }
        catch (Exception)
        {
            // Ignorar erros de dispose durante cleanup
        }
    }

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(Uri requestUri, T value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Base class básica para testes rápidos que não precisam de toda a infraestrutura
/// </summary>
public abstract class BasicTestBase : IAsyncLifetime
{
    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    private DistributedApplication _app = null!;

    protected static readonly TimeSpan SimpleTimeout = TimeSpan.FromSeconds(60);

    protected JsonSerializerOptions JsonOptions { get; } = SerializationDefaults.Api;

    public virtual async ValueTask InitializeAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource(SimpleTimeout);
        var cancellationToken = cancellationTokenSource.Token;

        var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>(cancellationToken);

        // Configuração mínima
        appHostBuilder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Error); // Apenas erros
        });

        _app = await appHostBuilder.BuildAsync(cancellationToken);
        await _app.StartAsync(cancellationToken);

        // Esperar apenas o mínimo necessário
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(SimpleTimeout, cancellationToken);

        ApiClient = _app.CreateHttpClient("apiservice");
        ApiClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            ApiClient?.Dispose();
            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }
        catch
        {
            // Ignorar erros durante cleanup
        }
    }
}
