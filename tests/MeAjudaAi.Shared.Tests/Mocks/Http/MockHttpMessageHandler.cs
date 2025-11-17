using System.Net;
using Moq;
using Moq.Protected;

namespace MeAjudaAi.Shared.Tests.Mocks.Http;

/// <summary>
/// Mock configurável de HttpMessageHandler para simular respostas HTTP em testes.
/// Reutilizável por todos os módulos que fazem chamadas HTTP externas.
/// </summary>
public sealed class MockHttpMessageHandler
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content)> _responses;

    public MockHttpMessageHandler()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _responses = new Dictionary<string, (HttpStatusCode, string)>();
    }

    /// <summary>
    /// Configura uma resposta para um padrão de URL específico.
    /// </summary>
    public MockHttpMessageHandler SetupResponse(
        string urlPattern,
        HttpStatusCode statusCode,
        string content,
        string contentType = "application/json")
    {
        _responses[urlPattern] = (statusCode, content);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
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
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode
            });

        return this;
    }

    /// <summary>
    /// Configura uma exceção para um padrão de URL.
    /// </summary>
    public MockHttpMessageHandler SetupException(string urlPattern, Exception exception)
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);

        return this;
    }

    /// <summary>
    /// Configura um timeout para um padrão de URL.
    /// </summary>
    public MockHttpMessageHandler SetupTimeout(string urlPattern)
    {
        return SetupException(urlPattern, new TaskCanceledException("Request timed out"));
    }

    /// <summary>
    /// Retorna o mock configurado do HttpMessageHandler.
    /// </summary>
    public HttpMessageHandler GetHandler() => _mockHandler.Object;

    /// <summary>
    /// Verifica se uma requisição foi feita para um padrão de URL específico.
    /// </summary>
    public void VerifyRequest(string urlPattern, Times times)
    {
        _mockHandler
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(urlPattern)),
                ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Reseta todas as configurações do mock.
    /// </summary>
    public void Reset()
    {
        _mockHandler.Reset();
        _responses.Clear();
    }
}
