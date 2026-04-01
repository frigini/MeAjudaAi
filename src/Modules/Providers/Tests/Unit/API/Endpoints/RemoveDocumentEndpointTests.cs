using System.Reflection;
using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
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
public class RemoveDocumentEndpointTests
{
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock = new();

    [Fact]
    public async Task RemoveDocumentAsync_WithValidParams_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.Identity;
        var providerDto = new ProviderDto { Id = providerId };

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RemoveDocumentCommand, Result<ProviderDto>>(
                It.IsAny<RemoveDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Success(providerDto));

        var methodInfo = typeof(RemoveDocumentEndpoint).GetMethod("RemoveDocumentAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, documentType, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<Ok<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task RemoveDocumentAsync_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.Identity;

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RemoveDocumentCommand, Result<ProviderDto>>(
                It.IsAny<RemoveDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(Error.BadRequest("Erro")));

        var methodInfo = typeof(RemoveDocumentEndpoint).GetMethod("RemoveDocumentAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, documentType, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<BadRequest<Result<ProviderDto>>>();
    }

    [Fact]
    public async Task RemoveDocumentAsync_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.Identity;

        _commandDispatcherMock
            .Setup(x => x.SendAsync<RemoveDocumentCommand, Result<ProviderDto>>(
                It.IsAny<RemoveDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto>.Failure(Error.NotFound("Not Found")));

        var methodInfo = typeof(RemoveDocumentEndpoint).GetMethod("RemoveDocumentAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var task = (Task<IResult>)methodInfo!.Invoke(null, 
            [providerId, documentType, _commandDispatcherMock.Object, CancellationToken.None])!;
        var result = await task;

        // Assert
        result.Should().BeOfType<NotFound<Result<ProviderDto>>>();
    }
}
