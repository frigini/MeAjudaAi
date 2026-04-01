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
public class UpdateProviderProfileEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    [Fact]
    public async Task UpdateProviderProfileAsync_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new UpdateProviderProfileRequest { Name = "New Name" };
        var providerDto = new ProviderDto { Id = providerId, Name = "New Name" };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
                It.IsAny<UpdateProviderProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        var methodInfo = typeof(UpdateProviderProfileEndpoint).GetMethod("UpdateProviderProfileAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task UpdateProviderProfileAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new UpdateProviderProfileRequest { Name = "New Name" };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
                It.IsAny<UpdateProviderProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(UpdateProviderProfileEndpoint).GetMethod("UpdateProviderProfileAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task UpdateProviderProfileAsync_WhenRequestIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        var methodInfo = typeof(UpdateProviderProfileEndpoint).GetMethod("UpdateProviderProfileAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, null!, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}
