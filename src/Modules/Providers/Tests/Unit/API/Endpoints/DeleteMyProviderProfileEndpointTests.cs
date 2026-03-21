using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para o endpoint DeleteMyProviderProfileEndpoint.
/// </summary>
[Trait("Category", "Unit")]
public class DeleteMyProviderProfileEndpointTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;

    public DeleteMyProviderProfileEndpointTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
    }

    /// <summary>
    /// Testa exclusão de perfil com provider válido deve retornar NoContent.
    /// </summary>
    [Fact]
    public async Task DeleteMyProfileAsync_WithValidProvider_ShouldReturnNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);

        var providerDto = new ProviderDto(
            Id: providerId,
            UserId: userId,
            Name: "Test",
            Slug: "test",
            Type: EProviderType.Individual,
            BusinessProfile: null!,
            Status: EProviderStatus.Active,
            VerificationStatus: EVerificationStatus.Verified,
            Tier: EProviderTier.Standard,
            Documents: new List<DocumentDto>(),
            Qualifications: new List<QualificationDto>(),
            Services: new List<ProviderServiceDto>(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null,
            IsActive: true);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        _commandDispatcherMock
            .Setup(x => x.SendAsync<DeleteMyProviderProfileCommand, Result>(
                It.Is<DeleteMyProviderProfileCommand>(c => c.ProviderId == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var methodInfo = typeof(DeleteMyProviderProfileEndpoint).GetMethod("DeleteMyProfileAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var task = (Task<IResult>)methodInfo!.Invoke(null, new object[] { context, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;
        // Assert
        result.Should().BeOfType<Ok<Result<object>>>();
        _queryDispatcherMock.Verify(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _commandDispatcherMock.Verify(x => x.SendAsync<DeleteMyProviderProfileCommand, Result>(It.IsAny<DeleteMyProviderProfileCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Testa exclusão de perfil quando provider não é encontrado deve retornar NotFound.
    /// </summary>
    [Fact]
    public async Task DeleteMyProfileAsync_WhenProviderNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = EndpointTestHelpers.CreateHttpContextWithUserId(userId);

        _queryDispatcherMock
            .Setup(x => x.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var methodInfo = typeof(DeleteMyProviderProfileEndpoint).GetMethod("DeleteMyProfileAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var task = (Task<IResult>)methodInfo!.Invoke(null, new object[] { context, _queryDispatcherMock.Object, _commandDispatcherMock.Object, CancellationToken.None })!;
        var result = await task;

        result.Should().BeOfType<NotFound<Response<object>>>();
        _commandDispatcherMock.Verify(x => x.SendAsync<DeleteMyProviderProfileCommand, Result>(It.IsAny<DeleteMyProviderProfileCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
