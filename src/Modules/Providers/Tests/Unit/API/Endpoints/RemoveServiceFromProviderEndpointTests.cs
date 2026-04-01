using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderServices;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class RemoveServiceFromProviderEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();
    private readonly Mock<ISearchProvidersModuleApi> _searchProvidersApiMock = new();
    private readonly Mock<ILogger<RemoveServiceFromProviderEndpoint>> _loggerMock = new();

    [Fact]
    public async Task RemoveServiceAsync_WithValidParams_ShouldReturnNoContent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RemoveServiceFromProviderCommand, Result>(
                It.IsAny<RemoveServiceFromProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _searchProvidersApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var methodInfo = typeof(RemoveServiceFromProviderEndpoint).GetMethod("RemoveServiceAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, serviceId, _commandDispatcherMock.Object, _searchProvidersApiMock.Object, _loggerMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NoContent>();
        _searchProvidersApiMock.Verify(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveServiceAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RemoveServiceFromProviderCommand, Result>(
                It.IsAny<RemoveServiceFromProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(RemoveServiceFromProviderEndpoint).GetMethod("RemoveServiceAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, serviceId, _commandDispatcherMock.Object, _searchProvidersApiMock.Object, _loggerMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<object>>>();
        _searchProvidersApiMock.Verify(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
