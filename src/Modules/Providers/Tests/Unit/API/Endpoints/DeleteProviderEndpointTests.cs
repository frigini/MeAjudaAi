using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class DeleteProviderEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    [Fact]
    public async Task DeleteProviderAsync_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteProviderCommand, Result>(
                It.IsAny<DeleteProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var methodInfo = typeof(DeleteProviderEndpoint).GetMethod("DeleteProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        // HandleNoContent returns Ok(result) on success
        result.Should().BeOfType<Ok<Result<object>>>();
    }

    [Fact]
    public async Task DeleteProviderAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteProviderCommand, Result>(
                It.IsAny<DeleteProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(DeleteProviderEndpoint).GetMethod("DeleteProviderAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<object>>>();
    }
}
