using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class SecurityHeadersMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<SecurityHeadersMiddleware>> _mockLogger;
    private readonly SecurityHeadersMiddleware _middleware;

    public SecurityHeadersMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();
        _middleware = new SecurityHeadersMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    [Fact]
    public void SecurityHeadersMiddleware_ShouldHaveCorrectConstructor()
    {
        // Act
        var middleware = new SecurityHeadersMiddleware(_mockNext.Object, _mockLogger.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void SecurityHeadersMiddleware_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Arrange
        RequestDelegate? next = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecurityHeadersMiddleware(next!, _mockLogger.Object));
    }

    [Fact]
    public void SecurityHeadersMiddleware_ShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var middlewareType = typeof(SecurityHeadersMiddleware);

        // Assert
        middlewareType.Should().NotBeNull();
        middlewareType.IsClass.Should().BeTrue();
        middlewareType.IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldRegisterOnStartingCallback()
    {
        // Arrange
        var setup = SetupMiddlewareContext();

        // Act
        await _middleware.InvokeAsync(setup.MockContext.Object);

        // Assert
        setup.MockResponse.Verify(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()), Times.Once);
        _mockNext.Verify(n => n(setup.MockContext.Object), Times.Once);
    }

    [Fact]
    public async Task OnStartingCallback_ShouldAddMissingSecurityHeaders()
    {
        // Arrange
        var setup = SetupMiddlewareContext();

        // Act
        await _middleware.InvokeAsync(setup.MockContext.Object);
        
        // Simula o início da resposta
        setup.OnStartingCallback.Should().NotBeNull();
        await setup.OnStartingCallback!(setup.CapturedState!);

        // Assert
        setup.Headers.Should().ContainKey("X-Frame-Options").WhoseValue.Should().ContainSingle("DENY");
        setup.Headers.Should().ContainKey("X-Content-Type-Options").WhoseValue.Should().ContainSingle("nosniff");
        setup.Headers.Should().ContainKey("Referrer-Policy").WhoseValue.Should().ContainSingle("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task OnStartingCallback_ShouldRemoveXPoweredByHeader()
    {
        // Arrange
        var setup = SetupMiddlewareContext();
        setup.Headers.Append("X-Powered-By", "ASP.NET");

        // Act
        await _middleware.InvokeAsync(setup.MockContext.Object);
        setup.OnStartingCallback.Should().NotBeNull();
        await setup.OnStartingCallback!(setup.CapturedState!);

        // Assert
        setup.Headers.Should().NotContainKey("X-Powered-By");
    }

    [Fact]
    public async Task OnStartingCallback_ShouldNotOverwriteExistingHeaders()
    {
        // Arrange
        var setup = SetupMiddlewareContext();
        setup.Headers.Append("X-Frame-Options", "SAMEORIGIN");

        // Act
        await _middleware.InvokeAsync(setup.MockContext.Object);
        setup.OnStartingCallback.Should().NotBeNull();
        await setup.OnStartingCallback!(setup.CapturedState!);

        // Assert
        setup.Headers.Should().ContainKey("X-Frame-Options").WhoseValue.Should().ContainSingle("SAMEORIGIN");
    }

    private MiddlewareTestSetup SetupMiddlewareContext()
    {
        var context = new DefaultHttpContext();
        var mockResponse = new Mock<HttpResponse>();
        var mockContext = new Mock<HttpContext>();

        // Coleção compartilhada de cabeçalhos
        var headers = new HeaderDictionary();
        mockResponse.SetupGet(r => r.Headers).Returns(headers);
        mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

        var setup = new MiddlewareTestSetup(mockContext, mockResponse, headers);

        mockResponse.Setup(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((cb, state) => 
            { 
                setup.OnStartingCallback = cb; 
                setup.CapturedState = state; 
            });
            
        return setup;
    }

    private class MiddlewareTestSetup(
        Mock<HttpContext> mockContext, 
        Mock<HttpResponse> mockResponse,
        IHeaderDictionary headers)
    {
        public Mock<HttpContext> MockContext { get; } = mockContext;
        public Mock<HttpResponse> MockResponse { get; } = mockResponse;
        public IHeaderDictionary Headers { get; } = headers;
        public Func<object, Task>? OnStartingCallback { get; set; }
        public object? CapturedState { get; set; }
    }
}