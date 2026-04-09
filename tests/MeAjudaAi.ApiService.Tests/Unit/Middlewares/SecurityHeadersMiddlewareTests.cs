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
    [Fact]
    public void SecurityHeadersMiddleware_ShouldHaveCorrectConstructor()
    {
        // Arrange
        var mockNext = new Mock<RequestDelegate>();
        var mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();

        // Act
        var middleware = new SecurityHeadersMiddleware(mockNext.Object, mockLogger.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void SecurityHeadersMiddleware_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Arrange
        RequestDelegate? next = null;
        var mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecurityHeadersMiddleware(next!, mockLogger.Object));
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
        var mockNext = new Mock<RequestDelegate>();
        var mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();
        var middleware = new SecurityHeadersMiddleware(mockNext.Object, mockLogger.Object);
        
        var mockResponse = new Mock<HttpResponse>();
        var mockContext = new Mock<HttpContext>();
        mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

        // Act
        await middleware.InvokeAsync(mockContext.Object);

        // Assert
        mockResponse.Verify(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()), Times.Once);
        mockNext.Verify(n => n(mockContext.Object), Times.Once);
    }

    [Fact]
    public async Task OnStartingCallback_ShouldAddMissingSecurityHeaders()
    {
        // Arrange
        var mockNext = new Mock<RequestDelegate>();
        var mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();
        var middleware = new SecurityHeadersMiddleware(mockNext.Object, mockLogger.Object);
        
        var context = new DefaultHttpContext();
        Func<object, Task>? onStartingCallback = null;
        object? capturedState = null;

        var mockResponse = new Mock<HttpResponse>();
        mockResponse.Setup(r => r.Headers).Returns(context.Response.Headers);
        mockResponse.Setup(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((cb, state) => { onStartingCallback = cb; capturedState = state; });
            
        var mockContext = new Mock<HttpContext>();
        mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

        // Act
        await middleware.InvokeAsync(mockContext.Object);
        
        // Simula o início da resposta
        onStartingCallback.Should().NotBeNull();
        await onStartingCallback!(capturedState!);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Frame-Options").WhoseValue.Should().ContainSingle("DENY");
        context.Response.Headers.Should().ContainKey("X-Content-Type-Options").WhoseValue.Should().ContainSingle("nosniff");
        context.Response.Headers.Should().ContainKey("Referrer-Policy").WhoseValue.Should().ContainSingle("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task OnStartingCallback_ShouldRemoveXPoweredByHeader()
    {
        // Arrange
        var mockNext = new Mock<RequestDelegate>();
        var mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();
        var middleware = new SecurityHeadersMiddleware(mockNext.Object, mockLogger.Object);
        
        var context = new DefaultHttpContext();
        context.Response.Headers.Append("X-Powered-By", "ASP.NET");
        
        Func<object, Task>? onStartingCallback = null;
        object? capturedState = null;

        var mockResponse = new Mock<HttpResponse>();
        mockResponse.Setup(r => r.Headers).Returns(context.Response.Headers);
        mockResponse.Setup(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((cb, state) => { onStartingCallback = cb; capturedState = state; });
            
        var mockContext = new Mock<HttpContext>();
        mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

        // Act
        await middleware.InvokeAsync(mockContext.Object);
        onStartingCallback.Should().NotBeNull();
        await onStartingCallback!(capturedState!);

        // Assert
        context.Response.Headers.Should().NotContainKey("X-Powered-By");
    }

    [Fact]
    public async Task OnStartingCallback_ShouldNotOverwriteExistingHeaders()
    {
        // Arrange
        var mockNext = new Mock<RequestDelegate>();
        var mockLogger = new Mock<ILogger<SecurityHeadersMiddleware>>();
        var middleware = new SecurityHeadersMiddleware(mockNext.Object, mockLogger.Object);
        
        var context = new DefaultHttpContext();
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        
        Func<object, Task>? onStartingCallback = null;
        object? capturedState = null;

        var mockResponse = new Mock<HttpResponse>();
        mockResponse.Setup(r => r.Headers).Returns(context.Response.Headers);
        mockResponse.Setup(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((cb, state) => { onStartingCallback = cb; capturedState = state; });
            
        var mockContext = new Mock<HttpContext>();
        mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

        // Act
        await middleware.InvokeAsync(mockContext.Object);
        onStartingCallback.Should().NotBeNull();
        await onStartingCallback!(capturedState!);

        // Assert
        context.Response.Headers.Should().ContainKey("X-Frame-Options").WhoseValue.Should().ContainSingle("SAMEORIGIN");
    }
}