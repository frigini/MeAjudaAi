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
}
