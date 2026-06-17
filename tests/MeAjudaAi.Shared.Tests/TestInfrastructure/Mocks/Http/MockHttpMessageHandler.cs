using Moq.Protected;
using System.Net;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Http;

/// <summary>
/// Mock configurável de HttpMessageHandler para simular respostas HTTP em testes.
/// Reutilizável por todos os módulos que fazem chamadas HTTP externas.
/// Suporta tanto API simples (SetResponse/SetException) quanto API fluente (SetupResponse/SetupException).
/// </summary>
public sealed class MockHttpMessageHandler
{
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public MockHttpMessageHandler()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
    }

    /// <summary>
    /// URL da última requisição recebida pelo mock.
    /// </summary>
    public string? LastRequestUri { get; private set; }

    #region API Simples (compatível com testes que usam SetResponse/SetException)

    /// <summary>
    /// Configura uma resposta padrão para qualquer requisição.
    /// </summary>
    public void SetResponse(HttpStatusCode statusCode, string content)
    {
        _mockHandler.Reset();
        LastRequestUri = null;

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                LastRequestUri = req.RequestUri?.ToString();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
    }

    /// <summary>
    /// Configura uma exceção padrão para qualquer requisição.
    /// </summary>
    public void SetException(Exception exception)
    {
        _mockHandler.Reset();
        LastRequestUri = null;

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                LastRequestUri = req.RequestUri?.ToString();
            })
            .ThrowsAsync(exception);
    }

    #endregion

    #region API Fluente (para cenários com múltiplos padrões de URL)

    /// <summary>
    /// Configura uma resposta para um padrão de URL específico.
    /// </summary>
    public MockHttpMessageHandler SetupResponse(
        string urlPattern,
        HttpStatusCode statusCode,
        string content,
        string contentType = "application/json",
        HttpMethod? method = null)
    {
        method ??= HttpMethod.Get;
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                LastRequestUri = req.RequestUri?.ToString();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, contentType)
            });

        return this;
    }

    /// <summary>
    /// Configura uma resposta de erro para um padrão de URL.
    /// </summary>
    public MockHttpMessageHandler SetupErrorResponse(
        string urlPattern,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        HttpMethod? method = null)
    {
        method ??= HttpMethod.Get;
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                LastRequestUri = req.RequestUri?.ToString();
            })
#pragma warning disable CA2000 // HttpResponseMessage em mock é ownership do caller
            .ReturnsAsync(new HttpResponseMessage
#pragma warning restore CA2000
            {
                StatusCode = statusCode
            });

        return this;
    }

    /// <summary>
    /// Configura uma exceção para um padrão de URL.
    /// </summary>
    public MockHttpMessageHandler SetupException(string urlPattern, Exception exception, HttpMethod? method = null)
    {
        method ??= HttpMethod.Get;
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                LastRequestUri = req.RequestUri?.ToString();
            })
            .ThrowsAsync(exception);

        return this;
    }

    /// <summary>
    /// Configura um timeout para um padrão de URL.
    /// </summary>
    public MockHttpMessageHandler SetupTimeout(string urlPattern, HttpMethod? method = null)
    {
        return SetupException(urlPattern, new TaskCanceledException("Request timed out"), method);
    }

    /// <summary>
    /// Retorna o mock configurado do HttpMessageHandler.
    /// </summary>
    public HttpMessageHandler GetHandler() => _mockHandler.Object;

    /// <summary>
    /// Verifica se uma requisição foi feita para um padrão de URL específico.
    /// </summary>
    public void VerifyRequest(string urlPattern, Times times, HttpMethod? method = null)
    {
        method ??= HttpMethod.Get;
        _mockHandler
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.Method == method &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Reseta todas as configurações do mock.
    /// </summary>
    public void Reset()
    {
        _mockHandler.Reset();
        LastRequestUri = null;
    }

    #endregion
}
