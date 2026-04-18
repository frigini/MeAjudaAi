using System.Net;
using Moq;
using Moq.Protected;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Http;

/// <summary>
/// Mock configurável de HttpMessageHandler para simular respostas HTTP em testes.
/// Reutilizável por todos os módulos que fazem chamadas HTTP externas.
/// </summary>
public sealed class MockHttpMessageHandler
{
    private readonly Mock<HttpMessageHandler> _mockHandler;

    public MockHttpMessageHandler()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
    }

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
            .ReturnsAsync(() => new HttpResponseMessage
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
    }
}
