using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.ApiService.Middleware;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Middlewares;

public class InspectAuthMiddlewareTests
{
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly InspectAuthMiddleware _middleware;

    public InspectAuthMiddlewareTests()
    {
        _envMock = new Mock<IWebHostEnvironment>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new InspectAuthMiddleware(_nextMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenInProduction_ShouldNotAddDebugHeaders()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
        context.Response.Headers.Should().NotContainKey(AuthConstants.Headers.DebugUser);
        context.Response.Headers.Should().NotContainKey(AuthConstants.Headers.DebugAuthStatus);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotAuthenticated_ShouldAddNotAuthenticatedHeader()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        var responseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();
        responseMock.SetupGet(r => r.Headers).Returns(headers);
        
        Func<Task>? capturedCallback = null;
        responseMock.Setup(r => r.OnStarting(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(callback => capturedCallback = callback);

        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);
        contextMock.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        // Act
        await _middleware.InvokeAsync(contextMock.Object);
        
        // Trigger callback
        if (capturedCallback != null) await capturedCallback();

        // Assert
        headers[AuthConstants.Headers.DebugAuthStatus].ToString().Should().Be("Not Authenticated");
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticated_ShouldAddUserHeaders()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        var responseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();
        responseMock.SetupGet(r => r.Headers).Returns(headers);
        
        Func<Task>? capturedCallback = null;
        responseMock.Setup(r => r.OnStarting(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(callback => capturedCallback = callback);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(AuthConstants.Claims.Permission, "Read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        
        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);
        contextMock.SetupGet(c => c.User).Returns(new ClaimsPrincipal(identity));

        // Act
        await _middleware.InvokeAsync(contextMock.Object);
        
        // Trigger callback
        if (capturedCallback != null) await capturedCallback();

        // Assert
        headers[AuthConstants.Headers.DebugUser].ToString().Should().Be("testuser");
        headers[AuthConstants.Headers.DebugRoles].ToString().Should().Contain("Admin");
        headers[AuthConstants.Headers.DebugPermissions].ToString().Should().Contain("Read");
    }
    
    [Fact]
    public async Task InvokeAsync_WhenManyPermissions_ShouldTruncateHeader()
    {
         // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        var responseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();
        responseMock.SetupGet(r => r.Headers).Returns(headers);
        
        Func<Task>? capturedCallback = null;
        responseMock.Setup(r => r.OnStarting(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(callback => capturedCallback = callback);

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };
        for (int i = 0; i < 200; i++)
        {
            claims.Add(new Claim(AuthConstants.Claims.Permission, $"Permission_{i}"));
        }
        
        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);
        contextMock.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

        // Act
        await _middleware.InvokeAsync(contextMock.Object);
        if (capturedCallback != null) await capturedCallback();

        // Assert
        var permHeader = headers[AuthConstants.Headers.DebugPermissions].ToString();
        permHeader.Should().EndWith("...");
        permHeader.Length.Should().BeGreaterThan(1000);
    }
}
