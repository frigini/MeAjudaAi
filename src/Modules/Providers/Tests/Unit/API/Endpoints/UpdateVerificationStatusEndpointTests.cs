using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

[Trait("Category", "Unit")]
public class UpdateVerificationStatusEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    [Fact]
    public async Task UpdateVerificationStatusAsync_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new UpdateVerificationStatusRequest { Status = EVerificationStatus.Verified };
        var providerDto = new ProviderDto { Id = providerId };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<UpdateVerificationStatusCommand, Result<ProviderDto>>(
                It.IsAny<UpdateVerificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        var methodInfo = typeof(UpdateVerificationStatusEndpoint).GetMethod("UpdateVerificationStatusAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task UpdateVerificationStatusAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new UpdateVerificationStatusRequest { Status = EVerificationStatus.Verified };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<UpdateVerificationStatusCommand, Result<ProviderDto>>(
                It.IsAny<UpdateVerificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(UpdateVerificationStatusEndpoint).GetMethod("UpdateVerificationStatusAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, request, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task UpdateVerificationStatusAsync_WhenRequestIsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        var methodInfo = typeof(UpdateVerificationStatusEndpoint).GetMethod("UpdateVerificationStatusAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, null!, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<string>>();
    }
}
