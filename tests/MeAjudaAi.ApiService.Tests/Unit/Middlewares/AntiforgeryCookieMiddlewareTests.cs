using MeAjudaAi.ApiService.Middlewares;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class AntiforgeryCookieMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<IAntiforgery> _mockAntiforgery;
    private readonly Mock<ILogger<AntiforgeryCookieMiddleware>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnv;

    public AntiforgeryCookieMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        _mockAntiforgery = new Mock<IAntiforgery>();
        _mockLogger = new Mock<ILogger<AntiforgeryCookieMiddleware>>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
    }

    [Fact]
    public void Constructor_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AntiforgeryCookieMiddleware(null!, _mockAntiforgery.Object, _mockLogger.Object, _mockEnv.Object));
    }

    [Fact]
    public void Constructor_WithNullAntiforgery_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AntiforgeryCookieMiddleware(_mockNext.Object, null!, _mockLogger.Object, _mockEnv.Object));
    }

    [Fact]
    public async Task InvokeAsync_GetRequest_ShouldCallGetAndStoreTokens()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "GET");
        var tokens = new AntiforgeryTokenSet("request-token", "cookie-token", "form-token", "header");
        _mockAntiforgery.Setup(a => a.GetAndStoreTokens(context)).Returns(tokens);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockAntiforgery.Verify(a => a.GetAndStoreTokens(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_GetRequest_ShouldAddXsrfTokenHeader()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "GET");
        var tokens = new AntiforgeryTokenSet("my-xsrf-token", "cookie", "form", "header");
        _mockAntiforgery.Setup(a => a.GetAndStoreTokens(context)).Returns(tokens);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("X-XSRF-TOKEN");
        context.Response.Headers["X-XSRF-TOKEN"].ToString().Should().Be("my-xsrf-token");
    }

    [Fact]
    public async Task InvokeAsync_NonGetRequest_ShouldSkipAntiforgery()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "POST");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockAntiforgery.Verify(a => a.GetAndStoreTokens(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_TestingEnvironment_Error_ShouldSwallow()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "GET");
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        _mockAntiforgery.Setup(a => a.GetAndStoreTokens(context)).Throws(new InvalidOperationException("test error"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert — no exception propagated, next still called
        _mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Production_Error_ShouldRethrow()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "GET");
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var expectedException = new InvalidOperationException("production error");
        _mockAntiforgery.Setup(a => a.GetAndStoreTokens(context)).Throws(expectedException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
        ex.Message.Should().Be("production error");
    }

    [Fact]
    public async Task InvokeAsync_Development_Error_ShouldNotRethrow()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "GET");
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        _mockAntiforgery.Setup(a => a.GetAndStoreTokens(context)).Throws(new InvalidOperationException("dev error"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert — exception swallowed, next still called
        _mockNext.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Always_ShouldCallNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext(method: "GET");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(n => n(context), Times.Once);
    }

    private AntiforgeryCookieMiddleware CreateMiddleware() =>
        new(_mockNext.Object, _mockAntiforgery.Object, _mockLogger.Object, _mockEnv.Object);

    private static DefaultHttpContext CreateHttpContext(string method = "GET") =>
        new() { Request = { Method = method, Path = "/test" } };
}
