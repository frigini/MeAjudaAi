using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Mocks.Http;

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
    public void ResetAll()
    {
        foreach (var handler in _handlers.Values)
        {
            handler.Reset();
        }
    }
}
