using System.Net;
using FluentAssertions;
using MeAjudaAi.Gateway.Middlewares;
using MeAjudaAi.Gateway.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Yarp.ReverseProxy.Forwarder;

namespace MeAjudaAi.Gateway.Tests.Unit.Middleware;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class ResilientForwarderHttpClientFactoryTests
{
    private readonly Mock<ILogger<ResilientForwarderHttpClientFactory>> _loggerMock;

    public ResilientForwarderHttpClientFactoryTests()
    {
        _loggerMock = new Mock<ILogger<ResilientForwarderHttpClientFactory>>();
    }

    private ResilientForwarderHttpClientFactory CreateFactory(GatewayResilienceOptions options)
    {
        return new ResilientForwarderHttpClientFactory(
            Microsoft.Extensions.Options.Options.Create(options),
            _loggerMock.Object);
    }

    [Fact]
    public void CreateHandler_WithRetryEnabled_ReturnsHandlerWithRetry()
    {
        var options = new GatewayResilienceOptions
        {
            RetryCount = 3,
            RetryBaseDelayMs = 100,
            RetryableMethods = ["GET", "HEAD"]
        };

        var factory = CreateFactory(options);
        var handler = factory.CreateHandler(new ForwarderHttpClientContext());

        var delegatingHandler = handler as DelegatingHandler;
        delegatingHandler.Should().NotBeNull();
        delegatingHandler!.InnerHandler.Should().NotBeNull();
        delegatingHandler.InnerHandler.Should().BeOfType<SocketsHttpHandler>();
    }

    [Fact]
    public void CreateHandler_WithRetryDisabled_ReturnsSocketsHttpHandler()
    {
        var options = new GatewayResilienceOptions
        {
            RetryCount = 0,
            TimeoutSeconds = 30
        };

        var factory = CreateFactory(options);
        var handler = factory.CreateHandler(new ForwarderHttpClientContext());

        handler.Should().BeOfType<SocketsHttpHandler>();
    }

    [Fact]
    public void CreateHandler_SocketsHttpHandler_HasCorrectTimeouts()
    {
        var options = new GatewayResilienceOptions
        {
            TimeoutSeconds = 45,
            RetryCount = 0
        };

        var factory = CreateFactory(options);
        var handler = factory.CreateHandler(new ForwarderHttpClientContext()) as SocketsHttpHandler;

        handler.Should().NotBeNull();
        handler!.ConnectTimeout.Should().Be(TimeSpan.FromSeconds(45));
        handler.ResponseDrainTimeout.Should().Be(TimeSpan.FromSeconds(45));
    }

    [Fact]
    public void CreateClient_ReturnsHttpMessageInvoker()
    {
        var options = new GatewayResilienceOptions
        {
            RetryCount = 0,
            TimeoutSeconds = 30
        };

        var factory = CreateFactory(options);
        var client = factory.CreateClient(new ForwarderHttpClientContext());

        client.Should().BeOfType<HttpMessageInvoker>();
    }

    [Fact]
    public void CreateHandler_WithRetryMethods_UsesCustomMethods()
    {
        var options = new GatewayResilienceOptions
        {
            RetryCount = 2,
            RetryableMethods = ["GET", "POST", "PUT"]
        };

        var factory = CreateFactory(options);
        var handler = factory.CreateHandler(new ForwarderHttpClientContext());

        var delegatingHandler = handler as DelegatingHandler;
        delegatingHandler.Should().NotBeNull();
    }

    [Fact]
    public void CreateHandler_RetryableMethods_IncludesExpectedMethods()
    {
        var options = new GatewayResilienceOptions
        {
            RetryCount = 1,
            RetryableMethods = ["GET", "HEAD", "PUT", "DELETE"]
        };

        var factory = CreateFactory(options);
        var handler = factory.CreateHandler(new ForwarderHttpClientContext());

        handler.Should().BeAssignableTo<DelegatingHandler>();
        
        var delegatingHandler = (DelegatingHandler)handler;
        delegatingHandler.InnerHandler.Should().NotBeNull();
    }

    [Fact]
    public void CreateHandler_NonRetryableMethods_ExcludesPost()
    {
        var options = new GatewayResilienceOptions
        {
            RetryCount = 1,
            RetryableMethods = ["GET", "HEAD"]
        };

        var factory = CreateFactory(options);
        var handler = factory.CreateHandler(new ForwarderHttpClientContext());

        handler.Should().BeAssignableTo<DelegatingHandler>();
    }

    [Fact]
    public async Task SendAsync_Get_Retries_UntilSuccess()
    {
        var options = new GatewayResilienceOptions { RetryCount = 2, RetryBaseDelayMs = 1 };
        var factory = CreateFactory(options);

        var handler = factory.CreateHandler(new ForwarderHttpClientContext());
        var delegating = handler as DelegatingHandler;
        delegating.Should().NotBeNull();

        int calls = 0;
        delegating!.InnerHandler = new StubHandler(() =>
        {
            calls++;
            return calls < 3
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var client = new HttpMessageInvoker(delegating);
        var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://unit.test"), CancellationToken.None);

        calls.Should().Be(3);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_Post_DoesNotRetry_ByDefault()
    {
        var options = new GatewayResilienceOptions { RetryCount = 3, RetryBaseDelayMs = 1 };
        var factory = CreateFactory(options);

        var handler = factory.CreateHandler(new ForwarderHttpClientContext());
        var delegating = handler as DelegatingHandler;
        delegating.Should().NotBeNull();

        int calls = 0;
        delegating!.InnerHandler = new StubHandler(() =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        using var client = new HttpMessageInvoker(delegating);
        var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://unit.test"), CancellationToken.None);

        calls.Should().Be(1);
        resp.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task SendAsync_TransientException_Retries_ThenSucceeds()
    {
        var options = new GatewayResilienceOptions { RetryCount = 2, RetryBaseDelayMs = 1 };
        var factory = CreateFactory(options);

        var handler = factory.CreateHandler(new ForwarderHttpClientContext());
        var delegating = handler as DelegatingHandler;
        delegating.Should().NotBeNull();

        int calls = 0;
        delegating!.InnerHandler = new ExceptionThenSuccessHandler(() =>
        {
            calls++;
            if (calls < 3) throw new HttpRequestException("Transient");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var client = new HttpMessageInvoker(delegating);
        var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://unit.test"), CancellationToken.None);

        calls.Should().Be(3);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_AllRetries_Exhausted_Throws()
    {
        var options = new GatewayResilienceOptions { RetryCount = 2, RetryBaseDelayMs = 1 };
        var factory = CreateFactory(options);

        var handler = factory.CreateHandler(new ForwarderHttpClientContext());
        var delegating = handler as DelegatingHandler;
        delegating.Should().NotBeNull();

        delegating!.InnerHandler = new ExceptionThenSuccessHandler(() => throw new HttpRequestException("Always"));

        using var client = new HttpMessageInvoker(delegating);
        var act = async () => await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://unit.test"), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responder;
        public StubHandler(Func<HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder());
    }

    private sealed class ExceptionThenSuccessHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responder;
        public ExceptionThenSuccessHandler(Func<HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder());
    }
}