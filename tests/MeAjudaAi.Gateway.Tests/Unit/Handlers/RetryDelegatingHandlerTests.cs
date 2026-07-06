using FluentAssertions;
using MeAjudaAi.Gateway.Handlers;
using MeAjudaAi.Gateway.Options;
using System.Net;

namespace MeAdjudaAi.Gateway.Tests.Unit.Handlers;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class RetryDelegatingHandlerTests
{
    private readonly TrackingHandler _innerHandler = new();

    private RetryDelegatingHandler CreateHandler(GatewayResilienceOptions? options = null)
    {
        options ??= new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 };
        return new RetryDelegatingHandler(options) { InnerHandler = _innerHandler };
    }

    private HttpClient CreateClient(RetryDelegatingHandler handler) =>
        new(handler) { BaseAddress = new Uri("http://localhost") };

    [Fact]
    public async Task SendAsync_ShouldReturnResponseDirectly_WhenStatusIsSuccess()
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler();
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData((HttpStatusCode)429)]
    [InlineData((HttpStatusCode)408)]
    public async Task SendAsync_ShouldRetryOnTransientStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(statusCode));
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(2);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task SendAsync_ShouldNotRetryOnNonTransientStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(statusCode));
        using var handler = CreateHandler();
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(statusCode);
        _innerHandler.CallCount.Should().Be(1);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task SendAsync_ShouldNotRetryOnNonRetryableMethod(string method)
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var handler = CreateHandler();
        using var client = CreateClient(handler);

        // Act
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "/test"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _innerHandler.CallCount.Should().Be(1);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task SendAsync_ShouldRetryOnRetryableMethod(string method)
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), "/test"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task SendAsync_ShouldRetryOnHttpRequestException()
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        _innerHandler.Exceptions.Enqueue(new HttpRequestException("Connection refused"));
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task SendAsync_ShouldRetryOnTaskCanceledException_WhenNotCancelled()
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        _innerHandler.Exceptions.Enqueue(new TaskCanceledException("Timeout"));
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task SendAsync_ShouldThrowOnNonTransientException()
    {
        // Arrange
        _innerHandler.Exceptions.Enqueue(new InvalidOperationException("Something bad"));
        using var handler = CreateHandler();
        using var client = CreateClient(handler);

        // Act
        var act = () => client.GetAsync("/test");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _innerHandler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_ShouldRespectMaxRetryCount()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
            _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 2, RetryBaseDelayMs = 1 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _innerHandler.CallCount.Should().Be(3); // 1 initial + 2 retries
    }

    [Fact]
    public async Task SendAsync_ShouldNotRetry_WhenRetryCountIsZero()
    {
        // Arrange
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 0 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _innerHandler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_ShouldBufferContent_BeforeRetry()
    {
        // Arrange
        var options = new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 };
        options.RetryableMethods.Clear();
        options.RetryableMethods.Add("POST");

        var content = new StringContent("test-payload");
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler(options);
        using var client = CreateClient(handler);

        // Act
        var response = await client.PostAsync("/test", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task SendAsync_ShouldUseCustomRetryableMethods()
    {
        // Arrange - only POST is retryable
        var options = new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 };
        options.RetryableMethods.Clear();
        options.RetryableMethods.Add("POST");

        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
        using var handler = CreateHandler(options);
        using var client = CreateClient(handler);

        // Act
        var response = await client.PostAsync("/test", new StringContent("data"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandler.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnLastResponse_WhenAllRetriesFail()
    {
        // Arrange
        for (var i = 0; i < 5; i++)
            _innerHandler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var handler = CreateHandler(new GatewayResilienceOptions { RetryCount = 2, RetryBaseDelayMs = 1 });
        using var client = CreateClient(handler);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        _innerHandler.CallCount.Should().Be(3);
    }

    private sealed class TrackingHandler : HttpMessageHandler
    {
        public Queue<HttpResponseMessage> Responses { get; } = new();
        public Queue<Exception> Exceptions { get; } = new();
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;

            if (Exceptions.Count > 0)
                throw Exceptions.Dequeue();

            return Task.FromResult(Responses.Dequeue());
        }
    }
}
