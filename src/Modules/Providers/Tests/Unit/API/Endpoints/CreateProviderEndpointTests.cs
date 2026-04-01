using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class CreateProviderEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    [Fact]
    public async Task CreateProviderAsync_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateProviderRequest { Name = "Provider", UserId = Guid.NewGuid() };
        var providerDto = new ProviderDto { Id = Guid.NewGuid(), Name = "Provider" };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
                It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        var methodInfo = typeof(CreateProviderEndpoint).GetMethod("CreateProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<CreatedAtRoute<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task CreateProviderAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateProviderRequest { Name = "Provider", UserId = Guid.NewGuid() };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
                It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(CreateProviderEndpoint).GetMethod("CreateProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task CreateProviderAsync_WhenRequestIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var methodInfo = typeof(CreateProviderEndpoint).GetMethod("CreateProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [null!, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}
