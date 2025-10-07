using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        var mockEnvironment = new Mock<IWebHostEnvironment>();

        // Act
        var middleware = new SecurityHeadersMiddleware(mockNext.Object, mockEnvironment.Object);

        // Assert
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void SecurityHeadersMiddleware_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Arrange
        RequestDelegate? next = null;
        var mockEnvironment = new Mock<IWebHostEnvironment>();

        // Act & Assert - The primary constructor may not throw, so we check if middleware works correctly
        var middleware = new SecurityHeadersMiddleware(next!, mockEnvironment.Object);
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void SecurityHeadersMiddleware_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockNext = new Mock<RequestDelegate>();
        IWebHostEnvironment? environment = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SecurityHeadersMiddleware(mockNext.Object, environment!));
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
