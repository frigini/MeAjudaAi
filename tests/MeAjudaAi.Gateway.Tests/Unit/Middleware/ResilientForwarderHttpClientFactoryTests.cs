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
}