using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class BecomeProviderEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    private static HttpContext CreateContextWithUserIdAndEmail(Guid userId, string email)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim("email", email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    [Fact]
    public async Task BecomeProviderAsync_WithValidData_ShouldReturn201()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContextWithUserIdAndEmail(userId, "joao@test.com");
        var request = new RegisterProviderApiRequest("João", EProviderType.Individual, "12345678901", "11999999999");
        var providerDto = new ProviderDto { Id = Guid.NewGuid(), Name = "João" };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RegisterProviderCommand, Result<ProviderDto>>(
                It.IsAny<RegisterProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        var methodInfo = typeof(BecomeProviderEndpoint).GetMethod("BecomeProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, [context, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<CreatedAtRoute<Response<ProviderDto>>>();
    }

    [Fact]
    public async Task BecomeProviderAsync_WhenEmailMissingFromToken_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim("sub", userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);
        
        var request = new RegisterProviderApiRequest("João", EProviderType.Individual, "12345678901", null);
        
        var methodInfo = typeof(BecomeProviderEndpoint).GetMethod("BecomeProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, [context, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
        _commandDispatcherMock.Verify(x => x.SendAsync<RegisterProviderCommand, Result<ProviderDto>>(
            It.IsAny<RegisterProviderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BecomeProviderAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = CreateContextWithUserIdAndEmail(userId, "joao@test.com");
        var request = new RegisterProviderApiRequest("João", EProviderType.Individual, "12345678901", "11999999999");

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RegisterProviderCommand, Result<ProviderDto>>(
                It.IsAny<RegisterProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(BecomeProviderEndpoint).GetMethod("BecomeProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, [context, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<ProviderDto>>>();
    }
}
