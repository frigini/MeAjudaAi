using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Http;

/// <summary>
/// Builder fluente para configurar HttpClients com mocks em testes.
/// Facilita a criação de cenários de teste com múltiplos HTTP clients mockados.
/// </summary>
public sealed class MockHttpClientBuilder
{
    private readonly IServiceCollection _services;
    private readonly Dictionary<string, MockHttpMessageHandler> _handlers;

    public MockHttpClientBuilder(IServiceCollection services)
    {
        _services = services;
        _handlers = new Dictionary<string, MockHttpMessageHandler>();
    }

    /// <summary>
    /// Adiciona um HttpClient mockado com nome específico.
    /// </summary>
    public MockHttpClientBuilder AddMockedClient(
        string clientName,
        Action<MockHttpMessageHandler>? configure = null)
    {
        var mockHandler = new MockHttpMessageHandler();
        configure?.Invoke(mockHandler);

        _handlers[clientName] = mockHandler;

        _services.AddHttpClient(clientName)
            .ConfigurePrimaryHttpMessageHandler(() => mockHandler.GetHandler());

        return this;
    }

    /// <summary>
    /// Adiciona um HttpClient mockado tipado.
    /// </summary>
    public MockHttpClientBuilder AddMockedClient<TClient>(
        Action<MockHttpMessageHandler>? configure = null)
        where TClient : class
    {
        var clientName = typeof(TClient).Name;
        var mockHandler = new MockHttpMessageHandler();
        configure?.Invoke(mockHandler);

        _handlers[clientName] = mockHandler;

        _services.AddHttpClient<TClient>()
            .ConfigurePrimaryHttpMessageHandler(() => mockHandler.GetHandler());

        return this;
    }

    /// <summary>
    /// Substitui um HttpClient já registrado com um mock handler.
    /// Usado para sobrescrever clientes registrados pelo módulo antes dos mocks.
    /// </summary>
    public MockHttpClientBuilder ReplaceMockedClient<TClient>(
        Action<MockHttpMessageHandler>? configure = null)
        where TClient : class
    {
        var clientName = typeof(TClient).Name;
        var mockHandler = new MockHttpMessageHandler();
        configure?.Invoke(mockHandler);

        _handlers[clientName] = mockHandler;

        var descriptor = new ServiceDescriptor(
            typeof(TClient),
            sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(clientName);
                var loggerFactory = sp.GetService<Microsoft.Extensions.Logging.ILoggerFactory>() 
                    ?? new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory();
                var logger = loggerFactory.CreateLogger(typeof(TClient).Name);
                return Activator.CreateInstance(typeof(TClient), httpClient, logger)!;
            },
            ServiceLifetime.Scoped);

        var existingDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(TClient));
        if (existingDescriptor != null)
        {
            _services.Remove(existingDescriptor);
        }
        _services.Add(descriptor);

        _services.AddHttpClient(clientName)
            .ConfigurePrimaryHttpMessageHandler(() => mockHandler.GetHandler());

        return this;
    }

    /// <summary>
    /// Substitui um HttpClient já registrado por nome com um mock handler.
    /// </summary>
    public MockHttpClientBuilder ReplaceMockedClient(
        string clientName,
        Action<MockHttpMessageHandler>? configure = null)
    {
        var mockHandler = new MockHttpMessageHandler();
        configure?.Invoke(mockHandler);

        _handlers[clientName] = mockHandler;

        _services.AddHttpClient(clientName)
            .ConfigurePrimaryHttpMessageHandler(() => mockHandler.GetHandler());

        return this;
    }

    /// <summary>
    /// Obtém o handler mockado para um cliente específico.
    /// </summary>
    public MockHttpMessageHandler GetHandler(string clientName)
    {
        return _handlers.TryGetValue(clientName, out var handler)
            ? handler
            : throw new InvalidOperationException($"Handler '{clientName}' not found");
    }

    /// <summary>
    /// Obtém o handler mockado para um cliente tipado.
    /// </summary>
    public MockHttpMessageHandler GetHandler<TClient>() where TClient : class
    {
        return GetHandler(typeof(TClient).Name);
    }

    /// <summary>
    /// Reseta todos os mocks configurados.
    /// </summary>
    /// <param name="clearHandlers">Se true, limpa também o dicionário de handlers registrados.</param>
    public void ResetAll(bool clearHandlers = false)
    {
        foreach (var handler in _handlers.Values)
        {
            handler.Reset();
        }

        if (clearHandlers)
        {
            _handlers.Clear();
        }
    }
}
