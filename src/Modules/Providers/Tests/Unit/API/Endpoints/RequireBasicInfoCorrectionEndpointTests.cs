using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class RequireBasicInfoCorrectionEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    [Fact]
    public async Task RequireBasicInfoCorrectionAsync_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new RequireBasicInfoCorrectionRequest { Reason = "Dados incompletos" };
        var context = CreateHttpContextWithUser("admin");

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RequireBasicInfoCorrectionCommand, Result>(
                It.IsAny<RequireBasicInfoCorrectionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var methodInfo = typeof(RequireBasicInfoCorrectionEndpoint).GetMethod("RequireBasicInfoCorrectionAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, request, _commandDispatcherMock.Object, context, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result>>();
    }

    [Fact]
    public async Task RequireBasicInfoCorrectionAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new RequireBasicInfoCorrectionRequest { Reason = "Dados incompletos" };
        var context = CreateHttpContextWithUser("admin");

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RequireBasicInfoCorrectionCommand, Result>(
                It.IsAny<RequireBasicInfoCorrectionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(RequireBasicInfoCorrectionEndpoint).GetMethod("RequireBasicInfoCorrectionAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, request, _commandDispatcherMock.Object, context, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<object>>>();
    }

    [Fact]
    public async Task RequireBasicInfoCorrectionAsync_WhenRequestIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var context = CreateHttpContextWithUser("admin");

        var methodInfo = typeof(RequireBasicInfoCorrectionEndpoint).GetMethod("RequireBasicInfoCorrectionAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, null!, _commandDispatcherMock.Object, context, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }

    private static HttpContext CreateHttpContextWithUser(string name)
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, name) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }
}
